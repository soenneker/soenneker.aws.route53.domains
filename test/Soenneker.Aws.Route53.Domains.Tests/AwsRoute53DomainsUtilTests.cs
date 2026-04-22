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
    public void Default()
    {

    }
}
