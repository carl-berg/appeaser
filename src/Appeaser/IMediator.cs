using System.Threading.Tasks;

namespace Appeaser
{
    public interface IMediator
    {
        TResponse Request<TResponse>(IQuery<TResponse> query);
        Task<TResponse> Request<TResponse>(IAsyncQuery<TResponse> query);

        TResult Send<TResult>(ICommand<TResult> query);
        Task<TResult> Send<TResult>(IAsyncCommand<TResult> query);
    }
}