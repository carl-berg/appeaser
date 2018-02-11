using System.Threading.Tasks;

namespace Appeaser
{
    public interface ISimpleMediator
    {
        TResponse Request<TResponse>(IRequest<TResponse> query);
        Task<TResponse> Request<TResponse>(IAsyncRequest<TResponse> query);
    }
}