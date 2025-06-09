using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Aws.Route53.Domains.Tests;

[Collection("Collection")]
public sealed class AwsRoute53DomainsUtilTests : FixturedUnitTest
{
    private readonly IAwsRoute53DomainsUtil _util;

    public AwsRoute53DomainsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IAwsRoute53DomainsUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
