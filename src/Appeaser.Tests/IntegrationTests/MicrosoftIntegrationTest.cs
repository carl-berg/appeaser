using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Interception;
using Appeaser.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Appeaser.Tests.IntegrationTests
{
    public static class MicrosoftDependencyInjectionTest
    { 
        public class ServiceRegistration
        {
            private IServiceCollection _services;
            private ServiceProvider _provider;

            public ServiceRegistration()
            {
                _services = new ServiceCollection().AddAppeaser();
                _provider = _services.BuildServiceProvider();
            }

            [Fact]
            public void CanResolveMediatorResolver()
            {
                var resolver = _provider.GetService<IMediatorResolver>();
                Assert.NotNull(resolver);
            }

            [Fact]
            public void CanResolveMediator()
            {
                var mediator = _provider.GetService<ISimpleMediator>();
                Assert.NotNull(mediator);
            }

            [Fact]
            public void HasAddedRequestHandlerToServiceCollection()
            {
                var registeredHandler =_services.FirstOrDefault(dep => dep.ImplementationType == typeof(TestRequestFeature.Handler));
                Assert.NotNull(registeredHandler);
            }

            [Fact]
            public void HasAddedQueryHandlerToServiceCollection()
            {
                var registeredHandler = _services.FirstOrDefault(dep => dep.ImplementationType == typeof(TestQueryFeature.Handler));
                Assert.NotNull(registeredHandler);
            }

            [Fact]
            public void HasAddedCommandHandlerToServiceCollection()
            {
                var registeredHandler = _services.FirstOrDefault(dep => dep.ImplementationType == typeof(TestCommandFeature.Handler));
                Assert.NotNull(registeredHandler);
            }

            [Fact]
            public void ProviderCanResolveRequestHandlers()
            {
                var resolver = _provider.GetService<IMediatorResolver>();
                var requestHandler = resolver.GetHandler(typeof(IRequestHandler<TestRequestFeature.Request, UnitType>));
                var asyncRequestHandler = resolver.GetHandler(typeof(IAsyncRequestHandler<TestRequestFeature.AsyncRequest, UnitType>));
                Assert.NotNull(requestHandler);
                Assert.NotNull(asyncRequestHandler);
            }

            [Fact]
            public void ProviderCanResolveQueryHandlers()
            {
                var resolver = _provider.GetService<IMediatorResolver>();
                var queryHandler = resolver.GetHandler(typeof(IQueryHandler<TestQueryFeature.Request, UnitType>));
                var asyncQueryHandler = resolver.GetHandler(typeof(IAsyncQueryHandler<TestQueryFeature.AsyncRequest, UnitType>));
                Assert.NotNull(queryHandler);
                Assert.NotNull(asyncQueryHandler);
            }

            [Fact]
            public void ProviderCanResolveCommandHandlers()
            {
                var resolver = _provider.GetService<IMediatorResolver>();
                var commandHandler = resolver.GetHandler(typeof(ICommandHandler<TestCommandFeature.Request, UnitType>));
                var asyncCommandHandler = resolver.GetHandler(typeof(IAsyncCommandHandler<TestCommandFeature.AsyncRequest, UnitType>));
                Assert.NotNull(commandHandler);
                Assert.NotNull(asyncCommandHandler);
            }

            [Fact]
            public void ProviderCanResolveMultipleHandlersOfSameTypeInTheSameClass()
            {
                var resolver = _provider.GetService<IMediatorResolver>();
                var requestHandler = resolver.GetHandler(typeof(IRequestHandler<TestRequestFeature.Request2, UnitType>));
                Assert.NotNull(requestHandler);
            }

            [Fact]
            public void TestInvokeMediator()
            {
                var mediator = _provider.GetService<ISimpleMediator>();
                var response = mediator.Request(new TestRequestFeature.Request());
                Assert.Equal(UnitType.Default, response);
            }


            public class TestRequestFeature
            {
                public class Request : IRequest<UnitType> { }
                public class Request2 : IRequest<UnitType> { }
                public class AsyncRequest : IAsyncRequest<UnitType> { }

                public class Handler : IRequestHandler<Request, UnitType>, IAsyncRequestHandler<AsyncRequest, UnitType>, IRequestHandler<Request2, UnitType>
                {
                    public UnitType Handle(Request request) => UnitType.Default;
                    public UnitType Handle(Request2 request) => UnitType.Default;
                    public Task<UnitType> Handle(AsyncRequest request) => Task.FromResult(UnitType.Default);
                }
            }

            public class TestQueryFeature
            {
                public class Request : IQuery<UnitType> { }
                public class AsyncRequest : IAsyncQuery<UnitType> { }

                public class Handler : IQueryHandler<Request, UnitType>, IAsyncQueryHandler<AsyncRequest, UnitType>
                {
                    public UnitType Handle(Request request) => UnitType.Default;
                    public Task<UnitType> Handle(AsyncRequest request) => Task.FromResult(UnitType.Default);
                }
            }

            public class TestCommandFeature
            {
                public class Request : ICommand<UnitType> { }
                public class AsyncRequest : IAsyncCommand<UnitType> { }

                public class Handler : ICommandHandler<Request, UnitType>, IAsyncCommandHandler<AsyncRequest, UnitType>
                {
                    public UnitType Handle(Request request) => UnitType.Default;
                    public Task<UnitType> Handle(AsyncRequest request) => Task.FromResult(UnitType.Default);
                }
            }
        }

        public class OptionsConfiguration
        {
            [Fact]
            public void CanResolveMediatorSettings()
            {
                var services = new ServiceCollection()
                    .AddAppeaser()
                    .BuildServiceProvider();

                var options = services.GetService<IOptions<MediatorSettings>>();

                options.ShouldNotBeNull();
                options.Value.ShouldNotBeNull();
            }

            [Fact]
            public void CanCanfigureMediatorSettingsBeforeAppeaserIsRegistered()
            {
                var services = new ServiceCollection()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                    .AddAppeaser()
                    .BuildServiceProvider();

                var options = services.GetService<IOptions<MediatorSettings>>();

                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
            }

            [Fact]
            public void CanCanfigureMediatorSettingsAfterAppeaserIsRegistered()
            {
                var services = new ServiceCollection()
                    .AddAppeaser()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                    .BuildServiceProvider();

                var options = services.GetService<IOptions<MediatorSettings>>();

                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
            }


            [Fact]
            public void CanCanfigureMediatorSettingsBeforeAndAfterAppeaserIsRegistered()
            {
                var services = new ServiceCollection()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                    .AddAppeaser()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorB>())
                    .BuildServiceProvider();

                var options = services.GetService<IOptions<MediatorSettings>>();

                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorB));
            }

            [Fact]
            public void CanCanfigureMediatorSettingsBeforeInAndAfterAppeaserIsRegistered()
            {
                var services = new ServiceCollection()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                    .AddAppeaser(c => c.AddRequestInterceptor<InterceptorB>())
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorC>())
                    .BuildServiceProvider();

                var options = services.GetService<IOptions<MediatorSettings>>();

                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorB));
                options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorC));
            }

            [Fact]
            public void AllConfiguredMediatorSettingsAreResolvedInMediator()
            {
                var services = new ServiceCollection()
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                    .AddAppeaser(c => c.AddRequestInterceptor<InterceptorB>())
                    .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorC>())
                    .BuildServiceProvider();

                var mediator = (Mediator)services.GetService<IMediator>();
                var settings = typeof(Mediator)
                    .GetField("Settings", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(mediator) as IMediatorSettings;

                settings.ShouldNotBeNull("Oops, settings were not properly resolved");
                settings.RequestInterceptors.ShouldContain(typeof(InterceptorA));
                settings.RequestInterceptors.ShouldContain(typeof(InterceptorB));
                settings.RequestInterceptors.ShouldContain(typeof(InterceptorC));
            }

            public class InterceptorA : IRequestInterceptor
            {
                public void Intercept(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }

                public Task InterceptAsync(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }
            }

            public class InterceptorB : IRequestInterceptor
            {
                public void Intercept(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }

                public Task InterceptAsync(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }
            }

            public class InterceptorC : IRequestInterceptor
            {
                public void Intercept(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }

                public Task InterceptAsync(IRequestInterceptionContext context)
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}
