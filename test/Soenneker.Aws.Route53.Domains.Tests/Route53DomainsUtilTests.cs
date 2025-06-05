using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Aws.Route53.Domains.Tests;

[Collection("Collection")]
public sealed class Route53DomainsUtilTests : FixturedUnitTest
{
    private readonly IRoute53DomainsUtil _util;

    public Route53DomainsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IRoute53DomainsUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
