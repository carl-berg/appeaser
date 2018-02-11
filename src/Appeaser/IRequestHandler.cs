namespace Appeaser
{
    public interface IRequestHandler<in TRequest, out TReturn>
        where TRequest : IRequest<TReturn>
    {
        TReturn Handle(TRequest request);
    }
}