using System.Threading.Tasks;

namespace Appeaser
{
    public interface IAsyncCommandHandler<in TCommand, TResult>
        where TCommand : IAsyncCommand<TResult>
    {
        Task<TResult> Handle(TCommand command);
    }
}