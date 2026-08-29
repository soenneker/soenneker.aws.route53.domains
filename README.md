[![](https://img.shields.io/nuget/v/soenneker.aws.route53.domains.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.aws.route53.domains/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.aws.route53.domains/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.aws.route53.domains/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.aws.route53.domains.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.aws.route53.domains/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.aws.route53.domains/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.aws.route53.domains/actions/workflows/codeql.yml)

# Soenneker.Aws.Route53.Domains

High-level AWS Route 53 Domains operations for registration, availability, contacts, nameservers, auto-renewal, and DNSSEC delegation-signer records.

## Installation

```bash
dotnet add package Soenneker.Aws.Route53.Domains
```

## Configuration and registration

```json
{
  "Aws": {
    "AccessKey": "access-key-id",
    "SecretKey": "secret-access-key"
  }
}
```

```csharp
using Soenneker.Aws.Route53.Domains.Registrars;

builder.Services.AddAwsRoute53DomainsUtilAsSingleton();
```

The AWS identity needs Route 53 Domains permissions for every operation your application calls. Store credentials in a secret provider.

## Check availability

```csharp
using Soenneker.Aws.Route53.Domains.Abstract;

public sealed class DomainService(IAwsRoute53DomainsUtil domains)
{
    public ValueTask<bool> IsAvailable(
        string domainName,
        CancellationToken cancellationToken) =>
        domains.IsAvailable(domainName, cancellationToken);
}
```

`false` means AWS returned any status other than `AVAILABLE`; it does not distinguish unavailable, reserved, or unsupported TLD responses.

## Register a domain

```csharp
using Amazon.Route53Domains.Model;

var contact = new ContactDetail
{
    ContactType = ContactType.PERSON,
    FirstName = "Ada",
    LastName = "Lovelace",
    Email = "domains@example.com",
    PhoneNumber = "+1.2065550100",
    AddressLine1 = "123 Example Street",
    City = "Seattle",
    State = "WA",
    CountryCode = "US",
    ZipCode = "98101"
};

await domains.Register(
    "example.com",
    durationInYears: 1,
    contact,
    wait: true,
    cancellationToken);
```

Registration uses the same contact for admin, registrant, and technical roles and enables privacy protection for all three. Confirm that this matches the TLD's rules and your legal requirements before submitting the request.

## Other operations

- `Get()` and `GetAll()` return domain details and the complete paginated domain list.
- `UpdateNameservers()` replaces the domain's nameserver set; blank entries are ignored and at least one non-blank value is required.
- `UpdateContact()` submits separate admin, registrant, and technical contacts.
- `EnableAutoRenew()` and `DisableAutoRenew()` change renewal behavior.
- `ListOperations()` and `GetOperationDetail()` expose AWS operation state.
- `AddDsRecord()` and `RemoveDsRecord()` manage the Route 53 Domains delegation-signer association.

## Asynchronous AWS operations

Mutating methods with a `wait` parameter default to `false`. In that mode, completion means AWS accepted the operation; the requested change may still fail later. With `wait: true`, the utility polls until AWS reports success or failure, cancellation is requested, or the polling limit is reached.

Domain registration and configuration calls can incur charges or interrupt DNS. Cancellation stops local waiting but cannot cancel or roll back an operation already accepted by AWS.
