using Appeaser.Interception;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Appeaser.OpenTelemetry.Instrumentation;

public class OpenTelemetryInterceptor : IRequestInterceptor, IResponseInterceptor
{
    public const string ActivitySourceName = "Appeaser";
    private static readonly ActivitySource _source = new(ActivitySourceName);

    public void Intercept(IRequestInterceptionContext context)
    {
        var requestName = GetRequestName(context.RequestType);
        if (_source.StartActivity(requestName) is { } activity)
        {
            activity.AddTag("appeaser.request.type", context.RequestType.FullName);
            activity.AddTag("appeaser.handler.type", context.HandlerType.FullName);
            context.Set("Activity", activity);
        }
    }

    public Task InterceptAsync(IRequestInterceptionContext context)
    {
        Intercept(context);
        return Task.CompletedTask;
    }

    public void Intercept(IResponseInterceptionContext context)
    {
        if (context.Get<Activity>("Activity") is { } activity)
        {
            activity.AddTag("appeaser.response.type", context.ResponseType.FullName);
            if (context.Exception is { } ex)
            {
#if NET9_0_OR_GREATER
                activity.AddException(ex);
#else
                activity.AddTag("appeaser.exception.type", context.Exception.GetType().FullName);
                activity.AddTag("appeaser.exception.message", context.Exception.Message);
#endif

                activity.AddTag("appeaser.success", bool.FalseString);
                activity.SetStatus(ActivityStatusCode.Error, context.Exception.Message);
            }
            else
            {
                activity.AddTag("appeaser.success", bool.TrueString);
                activity.SetStatus(ActivityStatusCode.Ok);
            }

            activity.Stop();
            activity.Dispose();
        }
    }

    public Task InterceptAsync(IResponseInterceptionContext context)
    {
        Intercept(context);
        return Task.CompletedTask;
    }

    protected static string GetRequestName(Type requestType, string? suffix = null)
    {
        var name = requestType.Name;
        var text = suffix is null ? name : $"{name}.{suffix}";
        if (requestType.IsNested && requestType.DeclaringType is { })
        {
            return GetRequestName(requestType.DeclaringType, text);
        }

        return text;
    }
}
