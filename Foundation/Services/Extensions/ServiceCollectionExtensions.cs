using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Modules.Foundation.Services.Contracts;
using DotNetNuke.Modules.Foundation.Services.Adapters;
using DotNetNuke.Modules.Foundation.Services.Implementations;

namespace DotNetNuke.Modules.Foundation.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFoundationServices(this IServiceCollection services)
        {
            // Adapters
            services.AddSingleton<IDnnFacade, DnnFacade>();

            // Manifest service
            services.AddScoped<IManifestService, ManifestService>();

            // TODO: register caching, logging, template services here

            return services;
        }
    }
}
