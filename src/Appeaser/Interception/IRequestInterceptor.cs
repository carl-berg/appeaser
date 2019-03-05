using System.Threading.Tasks;

namespace Appeaser.Interception
{
    public interface IRequestInterceptor
    {
        void Intercept(IRequestInterceptionContext context);
        Task InterceptAsync(IRequestInterceptionContext context);
    }
}
