using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Route53Domains;
using Amazon.Route53Domains.Model;
using Microsoft.Extensions.Logging;
using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Aws.Route53.DomainsClientUtil.Abstract;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Delay;

namespace Soenneker.Aws.Route53.Domains;

/// <inheritdoc cref="IAwsRoute53DomainsUtil"/>
public sealed class AwsRoute53DomainsUtil : IAwsRoute53DomainsUtil
{
    private readonly IRoute53DomainsClientUtil _domainsClientUtil;
    private readonly ILogger<AwsRoute53DomainsUtil> _logger;

    private const int _initialPollIntervalMs = 1000; // Start with 1 second
    private const int _maxPollIntervalMs = 30000; // Max 30 seconds between polls
    private const int _maxRetries = 60; // Maximum number of retries (1 hour with max interval)

    public AwsRoute53DomainsUtil(IRoute53DomainsClientUtil domainsClientUtil, ILogger<AwsRoute53DomainsUtil> logger)
    {
        _domainsClientUtil = domainsClientUtil;
        _logger = logger;
    }

    public async ValueTask Register(string domainName, int durationInYears, ContactDetail contact, bool wait = false,
        CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));
        if (durationInYears < 1)
            throw new ArgumentException("Duration must be at least 1 year.", nameof(durationInYears));

        var request = new RegisterDomainRequest
        {
            DomainName = domainName,
            DurationInYears = durationInYears,
            AdminContact = contact,
            RegistrantContact = contact,
            TechContact = contact,
            PrivacyProtectAdminContact = true,
            PrivacyProtectRegistrantContact = true,
            PrivacyProtectTechContact = true
        };

        try
        {
            _logger.LogInformation("[Register] Initiating registration for domain {Domain} with {Years} year(s) duration", domainName, durationInYears);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            RegisterDomainResponse? response = await client.RegisterDomainAsync(request, cancellationToken).NoSync();
            _logger.LogInformation("[Register] Operation submitted for domain {Domain}. OperationId: {OperationId}", domainName, response.OperationId);

            if (wait)
            {
                _logger.LogDebug("[Register] Waiting for operation completion for domain {Domain}", domainName);
                await WaitForOperation(response.OperationId, cancellationToken);
                _logger.LogInformation("[Register] Domain {Domain} registration completed successfully", domainName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Register] Failed to register domain {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask UpdateNameservers(string domainName, List<string> nameservers, bool wait = false, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));
        if (nameservers == null || nameservers.Count == 0)
            throw new ArgumentException("At least one nameserver must be provided.", nameof(nameservers));

        var parsedNameservers = new List<Nameserver>(nameservers.Count);

        for (var i = 0; i < nameservers.Count; i++)
        {
            string ns = nameservers[i];
            if (ns.IsNullOrWhiteSpace())
                continue;

            parsedNameservers.Add(new Nameserver { Name = ns });
        }

        var request = new UpdateDomainNameserversRequest
        {
            DomainName = domainName,
            Nameservers = parsedNameservers
        };

        try
        {
            _logger.LogInformation("[Nameservers] Updating nameservers for {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            UpdateDomainNameserversResponse? response = await client.UpdateDomainNameserversAsync(request, cancellationToken).NoSync();
            _logger.LogInformation("[Nameservers] Operation submitted. OperationId: {OperationId}", response.OperationId);

            if (wait)
                await WaitForOperation(response.OperationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Nameservers] Failed to update nameservers for {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask DisableAutoRenew(string domainName, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        var request = new DisableDomainAutoRenewRequest
        {
            DomainName = domainName
        };

        try
        {
            _logger.LogInformation("[AutoRenew] Disabling auto-renew for {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            DisableDomainAutoRenewResponse? response = await client.DisableDomainAutoRenewAsync(request, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AutoRenew] Failed to disable auto-renew for {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask EnableAutoRenew(string domainName, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        var request = new EnableDomainAutoRenewRequest
        {
            DomainName = domainName
        };

        try
        {
            _logger.LogInformation("[AutoRenew] Enabling auto-renew for {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            EnableDomainAutoRenewResponse? response = await client.EnableDomainAutoRenewAsync(request, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AutoRenew] Failed to enable auto-renew for {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask<GetDomainDetailResponse> Get(string domainName, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        try
        {
            _logger.LogInformation("[GetDetail] Retrieving detailed information for domain {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            var request = new GetDomainDetailRequest {DomainName = domainName};
            GetDomainDetailResponse? response = await client.GetDomainDetailAsync(request, cancellationToken).NoSync();

            _logger.LogInformation("[GetDetail] Retrieved details for domain {Domain}", domainName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetDetail] Failed to get details for domain {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask UpdateContact(string domainName, ContactDetail adminContact, ContactDetail registrantContact, ContactDetail techContact,
        bool wait = false, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        var request = new UpdateDomainContactRequest
        {
            DomainName = domainName,
            AdminContact = adminContact,
            RegistrantContact = registrantContact,
            TechContact = techContact,
        };

        try
        {
            _logger.LogInformation("[UpdateContact] Updating contact info for {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            UpdateDomainContactResponse? response = await client.UpdateDomainContactAsync(request, cancellationToken).NoSync();
            _logger.LogInformation("[UpdateContact] Operation submitted. OperationId: {OperationId}", response.OperationId);

            if (wait)
                await WaitForOperation(response.OperationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateContact] Failed to update contact for {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask<List<DomainSummary>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("[GetAll] Retrieving list of domains");
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            var domains = new List<DomainSummary>();
            string? nextPageMarker = null;

            do
            {
                var request = new ListDomainsRequest {Marker = nextPageMarker};
                ListDomainsResponse? response = await client.ListDomainsAsync(request, cancellationToken).NoSync();
                domains.AddRange(response.Domains);
                nextPageMarker = response.NextPageMarker;
            } while (!string.IsNullOrEmpty(nextPageMarker));

            _logger.LogInformation("[GetAll] Retrieved {Count} domains", domains.Count);
            return domains;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetAll] Failed to retrieve domains: {Error}", ex.Message);
            throw;
        }
    }

    public async ValueTask<bool> IsAvailable(string domainName, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        try
        {
            _logger.LogInformation("[Availability] Checking availability for domain {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            var request = new CheckDomainAvailabilityRequest {DomainName = domainName};
            CheckDomainAvailabilityResponse? response = await client.CheckDomainAvailabilityAsync(request, cancellationToken).NoSync();

            _logger.LogInformation("[Availability] Domain {Domain} availability: {Available}", domainName, response.Availability);
            return response.Availability == DomainAvailability.AVAILABLE;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Availability] Failed to check availability for domain {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask<List<OperationSummary>> ListOperations(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("[ListOperations] Retrieving list of operations");
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            var operations = new List<OperationSummary>();
            string? nextPageMarker = null;

            do
            {
                var request = new ListOperationsRequest {Marker = nextPageMarker};
                ListOperationsResponse? response = await client.ListOperationsAsync(request, cancellationToken).NoSync();
                operations.AddRange(response.Operations);
                nextPageMarker = response.NextPageMarker;
            } while (!string.IsNullOrEmpty(nextPageMarker));

            _logger.LogInformation("[ListOperations] Retrieved {Count} operations", operations.Count);
            return operations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListOperations] Failed to retrieve operations: {Error}", ex.Message);
            throw;
        }
    }

    public async ValueTask<GetOperationDetailResponse> GetOperationDetail(string operationId, CancellationToken cancellationToken = default)
    {
        if (operationId.IsNullOrWhiteSpace())
            throw new ArgumentException("OperationId must be provided.", nameof(operationId));

        try
        {
            _logger.LogInformation("[OperationDetail] Retrieving details for operation {OperationId}", operationId);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            var request = new GetOperationDetailRequest {OperationId = operationId};
            GetOperationDetailResponse? response = await client.GetOperationDetailAsync(request, cancellationToken).NoSync();

            _logger.LogInformation("[OperationDetail] Retrieved details for operation {OperationId}. Status: {Status}", operationId, response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OperationDetail] Failed to get details for operation {OperationId}: {Error}", operationId, ex.Message);
            throw;
        }
    }

    public async ValueTask AddDsRecord(string domainName, int flags, int algorithm, string publicKey, bool wait = false,  CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));
        if (publicKey.IsNullOrWhiteSpace())
            throw new ArgumentException("PublicKey must be provided.", nameof(publicKey));

        var request = new AssociateDelegationSignerToDomainRequest
        {
            DomainName = domainName,
            SigningAttributes = new DnssecSigningAttributes
            {
                Flags = flags,
                Algorithm = algorithm,
                PublicKey = publicKey
            }
        };

        try
        {
            _logger.LogInformation("[DSRecord] Adding DS record for domain {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            AssociateDelegationSignerToDomainResponse? response = await client.AssociateDelegationSignerToDomainAsync(request, cancellationToken).NoSync();
            _logger.LogInformation("[DSRecord] Operation submitted. OperationId: {OperationId}", response.OperationId);

            if (wait)
                await WaitForOperation(response.OperationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DSRecord] Failed to add DS record for domain {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    public async ValueTask RemoveDsRecord(string domainName, bool wait = false, CancellationToken cancellationToken = default)
    {
        if (domainName.IsNullOrWhiteSpace())
            throw new ArgumentException("Domain name must be provided.", nameof(domainName));

        var request = new DisassociateDelegationSignerFromDomainRequest
        {
            DomainName = domainName
        };

        try
        {
            _logger.LogInformation("[DSRecord] Removing DS record for domain {Domain}", domainName);
            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            DisassociateDelegationSignerFromDomainResponse? response =
                await client.DisassociateDelegationSignerFromDomainAsync(request, cancellationToken).NoSync();
            _logger.LogInformation("[DSRecord] Operation submitted. OperationId: {OperationId}", response.OperationId);

            if (wait)
                await WaitForOperation(response.OperationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DSRecord] Failed to remove DS record for domain {Domain}: {Error}", domainName, ex.Message);
            throw;
        }
    }

    private async ValueTask WaitForOperation(string operationId, CancellationToken cancellationToken)
    {
        if (operationId.IsNullOrWhiteSpace())
            throw new ArgumentException("OperationId must be provided.", nameof(operationId));

        _logger.LogDebug("[Wait] Beginning to poll for operation {OperationId}", operationId);

        var retryCount = 0;
        int currentPollInterval = _initialPollIntervalMs;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (retryCount >= _maxRetries)
            {
                var msg = $"Operation {operationId} timed out after {_maxRetries} retries";
                _logger.LogError("[Wait] {Message}", msg);
                throw new TimeoutException(msg);
            }

            AmazonRoute53DomainsClient client = await _domainsClientUtil.Get(cancellationToken).NoSync();
            GetOperationDetailResponse? detailResponse = await client.GetOperationDetailAsync(
                                                                         new GetOperationDetailRequest {OperationId = operationId}, cancellationToken)
                                                                     .NoSync();

            OperationStatus? status = detailResponse.Status;
            _logger.LogDebug("[Wait] Operation {OperationId} status: {Status} (Attempt {Attempt} of {MaxRetries})", operationId, status, retryCount + 1,
                _maxRetries);

            if (status == OperationStatus.SUCCESSFUL)
            {
                _logger.LogInformation("[Wait] Operation {OperationId} completed successfully", operationId);
                return;
            }

            if (status == OperationStatus.FAILED || status == OperationStatus.ERROR)
            {
                string msg = detailResponse.Message ?? "Unknown AWS error";
                _logger.LogError("[Wait] Operation {OperationId} failed: {Error}", operationId, msg);
                throw new InvalidOperationException($"AWS operation failed: {msg}");
            }

            // If still in SUBMITTED or IN_PROGRESS, wait and poll again with exponential backoff
            await DelayUtil.Delay(currentPollInterval, _logger, cancellationToken);

            // Increase the poll interval exponentially, but cap it at MaxPollIntervalMs
            currentPollInterval = Math.Min(currentPollInterval * 2, _maxPollIntervalMs);
            retryCount++;
        }
    }
}