using Appeaser.Interception;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Appeaser.Microsoft.ApplicationInsights
{
    public class ExceptionInterceptor : IResponseInterceptor
    {
        private readonly IOptions<TelemetryConfiguration> _config;
        private readonly IOptions<AppeaserApplicationInsightsConfiguration> _options;

        public ExceptionInterceptor(IOptions<TelemetryConfiguration> config, IOptions<AppeaserApplicationInsightsConfiguration> options)
        {
            _config = config;
            _options = options;
        }

        public void Intercept(IResponseInterceptionContext context)
        {
            if (context.Exception != null && _options.Value.TrackExceptions)
            {
                new TelemetryClient(_config.Value).TrackException(context.Exception);
            }
        }

        public Task InterceptAsync(IResponseInterceptionContext context)
        {
            Intercept(context);
            return Task.CompletedTask;
        }
    }
}
