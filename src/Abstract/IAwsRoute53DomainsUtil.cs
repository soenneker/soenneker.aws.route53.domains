using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Route53Domains.Model;

namespace Soenneker.Aws.Route53.Domains.Abstract;

/// <summary>
/// Defines high-level operations for AWS Route 53 Domains.
/// </summary>
public interface IAwsRoute53DomainsUtil
{
    /// <summary>
    /// Initiates a domain registration request.
    /// </summary>
    /// <param name="domainName">The fully qualified domain name to register.</param>
    /// <param name="durationInYears">The registration period (in years).</param>
    /// <param name="contact">Contact details for admin, registrant, and tech.</param>
    /// <param name="wait">
    /// If true, polls AWS until the registration operation completes or fails.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask Register(string domainName, int durationInYears, ContactDetail contact, bool wait = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the nameservers for an existing domain.
    /// </summary>
    /// <param name="domainName">The domain whose nameservers will be updated.</param>
    /// <param name="nameservers">
    /// A list of hostnames (e.g. "ns-123.awsdns-45.org") to assign as nameservers.
    /// </param>
    /// <param name="wait">
    /// If true, polls AWS until the update operation completes or fails.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask UpdateNameservers(string domainName, List<string> nameservers, bool wait = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables automatic renewal for the specified domain.
    /// </summary>
    /// <param name="domainName">The domain to disable auto-renew on.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask DisableAutoRenew(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables automatic renewal for the specified domain.
    /// </summary>
    /// <param name="domainName">The domain to enable auto-renew on.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask EnableAutoRenew(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches detailed information about a domain.
    /// </summary>
    /// <param name="domainName">The domain to retrieve details for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="GetDomainDetailResponse"/> containing admin, registrant, tech contacts,
    /// nameservers, expiration date, and more.
    /// </returns>
    ValueTask<GetDomainDetailResponse> Get(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates contact information (admin, registrant, tech) for a domain.
    /// </summary>
    /// <param name="domainName">The domain to update contacts for.</param>
    /// <param name="adminContact">New admin contact detail.</param>
    /// <param name="registrantContact">New registrant contact detail.</param>
    /// <param name="techContact">New tech contact detail.</param>
    /// <param name="wait">
    /// If true, polls AWS until the contact update operation completes or fails.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask UpdateContact(string domainName, ContactDetail adminContact, ContactDetail registrantContact, ContactDetail techContact, bool wait = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all domains under the AWS account.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A list of <see cref="DomainSummary"/>, each containing domain name and creation date.
    /// </returns>
    ValueTask<List<DomainSummary>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a domain is available for registration.
    /// </summary>
    /// <param name="domainName">The domain to check availability for.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the domain can be registered; otherwise false.</returns>
    ValueTask<bool> IsAvailable(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all operations (registration, transfer, update) performed recently.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A list of <see cref="OperationSummary"/> with OperationId and status.
    /// </returns>
    ValueTask<List<OperationSummary>> ListOperations(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the status and details of a specific operation.
    /// </summary>
    /// <param name="operationId">The identifier of the operation to query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="GetOperationDetailResponse"/> including status, submission date, and message.
    /// </returns>
    ValueTask<GetOperationDetailResponse> GetOperationDetail(string operationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Associates a DS (Delegation Signer) record with the given domain in Route 53 Domains.
    /// </summary>
    /// <param name="domainName">The domain to update (e.g. "example.com").</param>
    /// <param name="flags">DNSSEC Flags (257 for KSK, 256 for ZSK).</param>
    /// <param name="algorithm">DNSSEC algorithm number (e.g. 8 = RSA/SHA-256, 13 = ECDSAP256SHA256).</param>
    /// <param name="publicKey">Base64-encoded DNSKEY public key bytes.</param>
    /// <param name="wait">If true, poll until the operation completes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask AddDsRecord(string domainName, int flags, int algorithm, string publicKey, bool wait = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a DS record from a domain.
    /// </summary>
    /// <param name="domainName">The domain to remove the DS record from.</param>
    /// <param name="wait">
    /// If true, polls AWS until the operation completes or fails.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    ValueTask RemoveDsRecord(string domainName, bool wait = false, CancellationToken cancellationToken = default);
}