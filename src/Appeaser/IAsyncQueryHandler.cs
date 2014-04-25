using System.Threading.Tasks;

namespace Appeaser
{
    public interface IAsyncQueryHandler<in TQuery, TReturn>
        where TQuery : IAsyncQuery<TReturn>
    {
        Task<TReturn> Handle(TQuery request);
    }
}