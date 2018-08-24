namespace Appeaser
{
    public interface IAsyncQueryHandler<in TQuery, TReturn> 
        : IAsyncRequestHandler<TQuery, TReturn>
        where TQuery : IAsyncQuery<TReturn> { }
}