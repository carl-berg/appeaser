namespace Appeaser
{
    public interface ICommandHandler<in TCommand, out TResult> 
        : IRequestHandler<TCommand, TResult>
        where TCommand : ICommand<TResult> { }
}