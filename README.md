[![](https://img.shields.io/nuget/v/soenneker.aws.route53.domains.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.aws.route53.domains/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.aws.route53.domains/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.aws.route53.domains/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.aws.route53.domains.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.aws.route53.domains/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.aws.route53.domains/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.aws.route53.domains/actions/workflows/codeql.yml)

# Soenneker.Aws.Route53.Domains

Defines high-level operations for AWS Route 53 Domains.

## Install

```bash
dotnet add package Soenneker.Aws.Route53.Domains
```

## Quick start

```csharp
using Soenneker.Aws.Route53.Domains.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddAwsRoute53DomainsUtilAsSingleton();
```

Adds `IAwsRoute53DomainsUtil` as a singleton service.

## What you get

- `IAwsRoute53DomainsUtil` — Defines high-level operations for AWS Route 53 Domains.
- `AwsRoute53DomainsUtilRegistrar` — A utility library for AWS Route53 domain related operations.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IAwsRoute53DomainsUtil.Register(domainName, durationInYears, contact, wait, cancellationToken)` | Initiates a domain registration request. | A task that completes when callback registration is finished. |
| `IAwsRoute53DomainsUtil.UpdateNameservers(domainName, nameservers, wait, cancellationToken)` | Updates the nameservers for an existing domain. | A task that completes when the nameservers update is complete. |
| `IAwsRoute53DomainsUtil.Get(domainName, cancellationToken)` | Fetches detailed information about a domain. | A `GetDomainDetailResponse` containing admin, registrant, tech contacts, nameservers, expiration date, and more. |
| `IAwsRoute53DomainsUtil.UpdateContact(domainName, adminContact, registrantContact, techContact, wait, cancellationToken)` | Updates contact information (admin, registrant, tech) for a domain. | A task that completes when the contact update is complete. |
| `IAwsRoute53DomainsUtil.GetAll(cancellationToken)` | Lists all domains under the AWS account. | A list of `DomainSummary`, each containing domain name and creation date. |
| `IAwsRoute53DomainsUtil.IsAvailable(domainName, cancellationToken)` | Checks if a domain is available for registration. | True if the domain can be registered; otherwise false. |
| `IAwsRoute53DomainsUtil.ListOperations(cancellationToken)` | Lists all operations (registration, transfer, update) performed recently. | A list of `OperationSummary` with OperationId and status. |
| `IAwsRoute53DomainsUtil.GetOperationDetail(operationId, cancellationToken)` | Retrieves the status and details of a specific operation. | A `GetOperationDetailResponse` including status, submission date, and message. |
| `IAwsRoute53DomainsUtil.AddDsRecord(domainName, flags, algorithm, publicKey, wait, cancellationToken)` | Associates a DS (Delegation Signer) record with the given domain in Route 53 Domains. | A task that completes when the ds record addition is complete. |
| `IAwsRoute53DomainsUtil.RemoveDsRecord(domainName, wait, cancellationToken)` | Removes a DS record from a domain. | A task that completes when the ds record removal is complete. |
| `AwsRoute53DomainsUtilRegistrar.AddAwsRoute53DomainsUtilAsSingleton(services)` | Adds `IAwsRoute53DomainsUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `AwsRoute53DomainsUtilRegistrar.AddAwsRoute53DomainsUtilAsScoped(services)` | Adds `IAwsRoute53DomainsUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
