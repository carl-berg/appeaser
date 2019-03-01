namespace Appeaser
{
    public interface IRequestInterceptor
    {
        void Intercept(IRequestInterceptionContext context);
    }
}
