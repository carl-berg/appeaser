namespace Appeaser
{
    public interface IResponseInterceptor
    {
        void Intercept(IResponseInterceptionContext context);
    }
}
