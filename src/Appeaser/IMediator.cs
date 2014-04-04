namespace Appeaser
{
    public interface IMediator
    {
        TResponse Request<TResponse>(IQuery<TResponse> query);
        TResult Send<TResult>(ICommand<TResult> query);
    }
}