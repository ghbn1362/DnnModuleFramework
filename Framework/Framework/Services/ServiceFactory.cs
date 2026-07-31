using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.Framework.Services
{
    /// <summary>
    /// Simple service resolver. Replace with DI container registration in future.
    /// </summary>
    public static class ServiceFactory
    {
        private static readonly Dictionary<Type, object> _overrides = new Dictionary<Type, object>();

        public static void Register<TService>(TService instance) where TService : class
        {
            _overrides[typeof(TService)] = instance;
        }

        public static TService Get<TService>(ModuleDefinition definition) where TService : class
        {
            if (_overrides.ContainsKey(typeof(TService)))
                return _overrides[typeof(TService)] as TService;

            // fallback to default concrete implementations
            if (typeof(TService) == typeof(ITemplateService)) return new TemplateService(definition) as TService;
            if (typeof(TService) == typeof(ITokenService)) return new TokenService() as TService;
            if (typeof(TService) == typeof(IManifestService)) return new ManifestService() as TService;
            if (typeof(TService) == typeof(IResourceService)) return new ResourceService(definition) as TService;
            if (typeof(TService) == typeof(IEnvironmentService)) return new EnvironmentService(definition) as TService;
            if (typeof(TService) == typeof(IDeviceDetectionService)) return new DeviceDetectionService() as TService;
            if (typeof(TService) == typeof(ILocalizationService)) return new LocalizationService(definition) as TService;
            if (typeof(TService) == typeof(IDashboardService)) return new DashboardService(definition) as TService;

            throw new InvalidOperationException($"No default implementation for {typeof(TService).FullName}");
        }
    }
}