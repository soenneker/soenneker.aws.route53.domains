using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Aws.Route53.DomainsClientUtil.Registrars;

namespace Soenneker.Aws.Route53.Domains.Registrars;

/// <summary>
/// A utility library for AWS Route53 domain related operations
/// </summary>
public static class AwsRoute53DomainsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IAwsRoute53DomainsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddAwsRoute53DomainsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddRoute53DomainsClientUtilAsSingleton().TryAddSingleton<IAwsRoute53DomainsUtil, AwsRoute53DomainsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IAwsRoute53DomainsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddAwsRoute53DomainsUtilAsScoped(this IServiceCollection services)
    {
        services.AddRoute53DomainsClientUtilAsSingleton().TryAddScoped<IAwsRoute53DomainsUtil, AwsRoute53DomainsUtil>();

        return services;
    }
}