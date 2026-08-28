using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Libvips.Util.Abstract;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Process.Registrars;

namespace Soenneker.Libvips.Util.Registrars;

/// <summary>
/// Registers the cross-platform managed libvips utility.
/// </summary>
public static class LibvipsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ILibvipsUtil"/> and its dependencies as singleton services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection, enabling fluent registration.</returns>
    public static IServiceCollection AddLibvipsUtilAsSingleton(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDirectoryUtilAsSingleton()
                .AddFileUtilAsSingleton()
                .AddProcessUtilAsSingleton()
                .TryAddSingleton<ILibvipsUtil, LibvipsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ILibvipsUtil"/> and its dependencies as scoped services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection, enabling fluent registration.</returns>
    public static IServiceCollection AddLibvipsUtilAsScoped(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDirectoryUtilAsScoped()
                .AddFileUtilAsScoped()
                .AddProcessUtilAsScoped()
                .TryAddScoped<ILibvipsUtil, LibvipsUtil>();

        return services;
    }
}
