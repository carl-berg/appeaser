using System.Threading.Tasks;

namespace Appeaser.Interception
{
    public interface IResponseInterceptor
    {
        void Intercept(IResponseInterceptionContext context);
        Task InterceptAsync(IResponseInterceptionContext context);
    }
}
