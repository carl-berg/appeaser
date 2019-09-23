using Appeaser.Diagnostics;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;

namespace Appeaser.Microsoft.ApplicationInsights
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a diagnostic interceptor and configures <see cref="DependencyTrackingTelemetryModule"/> to include it as a dependency.
        /// </summary>
        public static IServiceCollection ConfigureApplicationInsightsDiagnosticsForAppeaser(this IServiceCollection services, DependencyTrackingTelemetryModule depModule)
        {
            services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<DiagonsticInterceptor>());
            depModule.IncludeDiagnosticSourceActivities.Add(DiagonsticInterceptor.DiagnosticSourceName);
            return services;
        }
    }
}
