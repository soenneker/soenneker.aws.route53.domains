using System;
using System.Collections.Generic;
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
    public async Task Rejects_nameserver_lists_without_a_value()
    {
        Func<Task> act = async () => await _util.UpdateNameservers(
            "example.com",
            new List<string> {" ", "\t"});

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task Rejects_null_registration_contact()
    {
        Func<Task> act = async () => await _util.Register(
            "example.com",
            1,
            null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
