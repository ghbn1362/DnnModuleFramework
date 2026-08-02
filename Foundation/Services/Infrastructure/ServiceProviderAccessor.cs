using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Modules.Foundation.Services.Infrastructure
{
    public static class ServiceProviderAccessor
    {
        private static IServiceProvider _provider;
        public static void SetServiceProvider(IServiceProvider provider) => _provider = provider;
        public static T GetService<T>() => (T)_provider?.GetService(typeof(T));
        public static object GetService(Type serviceType) => _provider?.GetService(serviceType);
    }
}
