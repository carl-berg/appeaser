using System;
using Appeaser.Diagnostics;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;

namespace Appeaser.Microsoft.ApplicationInsights.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a diagnostic interceptor and configures <see cref="DependencyTrackingTelemetryModule"/> to include it as a dependency.
        /// Can also tracks exceptions, but is turned off by default.
        /// </summary>
        public static IServiceCollection ConfigureApplicationInsightsDiagnosticsForAppeaser(this IServiceCollection services, Action<AppeaserApplicationInsightsConfiguration> configure = null)
        {
            services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<DiagonsticInterceptor>());
            services.Configure<MediatorSettings>(cfg => cfg.AddResponseInterceptor<ExceptionInterceptor>());
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((depModule, options) =>
            {
                depModule.IncludeDiagnosticSourceActivities.Add(DiagonsticInterceptor.DiagnosticSourceName);
            });

            services.AddOptions<MediatorSettings>();
            if (configure != null)
            {
                services.Configure(configure);
            }

            return services;
        }
    }
}
