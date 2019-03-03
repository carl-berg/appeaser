using System.Threading.Tasks;

namespace Appeaser.Interception
{
    public interface IRequestInterceptor
    {
        Task Intercept(IRequestInterceptionContext context);
    }
}
