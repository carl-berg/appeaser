using Appeaser.Diagnostics;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Appeaser.Microsoft.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a diagnostic interceptor and configures <see cref="DependencyTrackingTelemetryModule"/> to include it as a dependency.
        /// Can also tracks exceptions, but is turned off by default.
        /// </summary>
        public static IServiceCollection ConfigureApplicationInsightsDiagnosticsForAppeaser(this IServiceCollection services, DependencyTrackingTelemetryModule depModule, Action<AppeaserApplicationInsightsConfiguration> configure = null)
        {
            services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<DiagonsticInterceptor>());
            services.Configure<MediatorSettings>(cfg => cfg.AddResponseInterceptor<ExceptionInterceptor>());

            services.AddOptions<MediatorSettings>();
            if (configure != null)
            {
                services.Configure(configure);
            }
            
            depModule.IncludeDiagnosticSourceActivities.Add(DiagonsticInterceptor.DiagnosticSourceName);
           
            return services;
        }
    }
}
