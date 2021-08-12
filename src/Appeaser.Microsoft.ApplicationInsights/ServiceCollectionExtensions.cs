using System;
using Appeaser.Diagnostics;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            services.AddTransient(sp => new DiagonsticInterceptor(sp.GetRequiredService<IOptions<AppeaserApplicationInsightsConfiguration>>().Value.UseFullRequestTypeNameAsActivityName));
            services.AddTransient<ExceptionInterceptor>();
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
