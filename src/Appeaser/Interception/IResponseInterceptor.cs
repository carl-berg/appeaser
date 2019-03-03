using System.Threading.Tasks;

namespace Appeaser.Interception
{
    public interface IResponseInterceptor
    {
        Task Intercept(IResponseInterceptionContext context);
    }
}
