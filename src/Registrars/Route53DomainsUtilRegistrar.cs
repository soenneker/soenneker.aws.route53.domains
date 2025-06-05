using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Aws.Route53.Domains.Abstract;
using Soenneker.Aws.Route53.DomainsClientUtil.Registrars;

namespace Soenneker.Aws.Route53.Domains.Registrars;

/// <summary>
/// A utility library for AWS Route53 domain related operations
/// </summary>
public static class Route53DomainsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IRoute53DomainsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddRoute53DomainsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddRoute53DomainsClientUtilAsSingleton().TryAddSingleton<IRoute53DomainsUtil, Route53DomainsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IRoute53DomainsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddRoute53DomainsUtilAsScoped(this IServiceCollection services)
    {
        services.AddRoute53DomainsClientUtilAsSingleton().TryAddScoped<IRoute53DomainsUtil, Route53DomainsUtil>();

        return services;
    }
}