using System.Threading.Tasks;

namespace Appeaser
{
    public interface IAsyncRequestHandler<in TRequest, TReturn>
        where TRequest : IAsyncRequest<TReturn>
    {
        Task<TReturn> Handle(TRequest request);
    }
}