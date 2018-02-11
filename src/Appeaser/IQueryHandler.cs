namespace Appeaser
{
    public interface IQueryHandler<in TQuery, out TReturn> 
        : IRequestHandler<TQuery, TReturn>
        where TQuery : IQuery<TReturn> { }
}