namespace Appeaser
{
    public interface IAsyncCommandHandler<in TCommand, TResult> 
        : IAsyncRequestHandler<TCommand, TResult>
        where TCommand : IAsyncCommand<TResult> { }
}