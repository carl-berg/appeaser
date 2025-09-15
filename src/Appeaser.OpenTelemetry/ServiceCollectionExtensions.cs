using Appeaser;
using Appeaser.OpenTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppeaserOpenTelemetry(this IServiceCollection services) 
        => services.Configure<MediatorSettings>(cfg => cfg.AddInterceptor<OpenTelemetryInterceptor>());
}