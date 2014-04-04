namespace Appeaser
{
    public interface IQueryHandler<in TQuery, out TReturn>
        where TQuery : IQuery<TReturn>
    {
        TReturn Handle(TQuery request);
    }
}