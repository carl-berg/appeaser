using Appeaser.Diagnostics;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;

namespace Appeaser.Microsoft.ApplicationInsights.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a diagnostic interceptor and configures the <see cref="DependencyTrackingTelemetryModule"/> to include it as a dependency.
        /// </summary>
        public static IServiceCollection ConfigureApplicationInsightsDiagnosticsForAppeaser(this IServiceCollection services)
        {
            services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<DiagonsticInterceptor>());
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((depModule, options) =>
            {
                depModule.IncludeDiagnosticSourceActivities.Add(DiagonsticInterceptor.DiagnosticSourceName);
            });

            return services;
        }
    }
}
