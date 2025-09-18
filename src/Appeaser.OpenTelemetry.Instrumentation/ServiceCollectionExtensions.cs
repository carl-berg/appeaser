using Appeaser;
using Appeaser.OpenTelemetry.Instrumentation;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTelemetry.Trace;

public static class ServiceCollectionExtensions
{
    public static TracerProviderBuilder AddAppeaserInstrumentation(this TracerProviderBuilder builder) => builder.ConfigureServices(services =>
    {
        services.AddSingleton<OpenTelemetryInterceptor>();
        services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<OpenTelemetryInterceptor>());
        builder.AddSource(OpenTelemetryInterceptor.ActivitySourceName);
    });
}