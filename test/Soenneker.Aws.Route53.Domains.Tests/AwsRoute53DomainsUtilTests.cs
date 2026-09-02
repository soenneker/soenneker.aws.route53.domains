using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Aws.Route53.Domains.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class AwsRoute53DomainsUtilTests : HostedUnitTest
{
    private readonly IAwsRoute53DomainsUtil _util;

    public AwsRoute53DomainsUtilTests(Host host) : base(host)
    {
        _util = Resolve<IAwsRoute53DomainsUtil>(true);
    }

    [Test]
    public async Task Rejects_nameserver_lists_without_a_value(CancellationToken cancellationToken)
    {
        Func<Task> act = async () => await _util.UpdateNameservers(
            "example.com",
            new List<string> {" ", "\t"},
            cancellationToken: cancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task Rejects_null_registration_contact(CancellationToken cancellationToken)
    {
        Func<Task> act = async () => await _util.Register(
            "example.com",
            1,
            null!,
            cancellationToken: cancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
