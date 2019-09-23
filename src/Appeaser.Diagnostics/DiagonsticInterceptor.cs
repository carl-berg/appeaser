using Appeaser.Interception;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Appeaser.Diagnostics
{
    /// <summary>
    /// Adds diagnostic activity tracing using a Diagnostic Listener. Can be used for dependency tracking with Application Insights
    /// </summary>
    public class DiagonsticInterceptor : IRequestInterceptor, IResponseInterceptor
    {
        public const string DiagnosticSourceName = "Appeaser";
        private static DiagnosticSource _logger = new DiagnosticListener(DiagnosticSourceName);
        private readonly bool _useFullRequestTypeNameAsActivityName;

        public DiagonsticInterceptor(bool useFullRequestTypeNameAsActivityName = false)
        {
            _useFullRequestTypeNameAsActivityName = useFullRequestTypeNameAsActivityName;
        }

        public void Intercept(IRequestInterceptionContext context)
        {
            if (_logger.IsEnabled("Request"))
            {
                var name = GetRequestName(context.RequestType);
                var activity = new Activity(name)
                    .AddTag("RequestType", context.RequestType.FullName)
                    .AddTag("HandlerType", context.HandlerType.FullName);
                context.Set("Activity", activity);
                _logger.StartActivity(activity, null);
                _logger.Write("RequestStart", new { context });
            }
        }

        public Task InterceptAsync(IRequestInterceptionContext context)
        {
            Intercept(context);
            return Task.CompletedTask;
        }

        public void Intercept(IResponseInterceptionContext context)
        {
            if (_logger.IsEnabled("Request"))
            {
                var activity = context.Get<Activity>("Activity");
                activity.AddTag("ResponseType", context.ResponseType.FullName);

                if (context.Exception != null)
                {
                    _logger.Write("RequestFailed", context.Exception);
                    activity.AddTag("ExceptionType", context.Exception.GetType().FullName);
                    activity.AddTag("ExceptionMessage", context.Exception.Message);
                }

                _logger.Write("RequestEnd", new { context });
                _logger.StopActivity(activity, null);
            }
        }

        public Task InterceptAsync(IResponseInterceptionContext context)
        {
            Intercept(context);
            return Task.CompletedTask;
        }

        protected string GetRequestName(Type requestType, string suffix = null)
        {
            if (_useFullRequestTypeNameAsActivityName)
            {
                return requestType.FullName;
            }

            var typeName = requestType.Name;
            var name = suffix == null ? typeName : $"{typeName}.{suffix}";
            if (requestType.IsNested)
            {
                return GetRequestName(requestType.DeclaringType, name);
            }

            return name;
        }
    }
}
