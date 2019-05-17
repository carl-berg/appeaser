using System.Linq;
using System.Threading.Tasks;
using Appeaser.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Appeaser.Tests.IntegrationTests
{
    public class MicrosoftDependencyInjectionTest
    {
        private IServiceCollection _services;
        private ServiceProvider _provider;

        public MicrosoftDependencyInjectionTest()
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
        public void ProviderCanResolveRequestHandler()
        {
            var resolver = _provider.GetService<IMediatorResolver>();
            var requestHandler = resolver.GetHandler(typeof(IRequestHandler<TestRequestFeature.Request, UnitType>));
            var asyncRequestHandler = resolver.GetHandler(typeof(IAsyncRequestHandler<TestRequestFeature.AsyncRequest, UnitType>));
            var queryHandler = resolver.GetHandler(typeof(IQueryHandler<TestQueryFeature.Request, UnitType>));
            var asyncQueryHandler = resolver.GetHandler(typeof(IAsyncQueryHandler<TestQueryFeature.AsyncRequest, UnitType>));
            var commandHandler = resolver.GetHandler(typeof(ICommandHandler<TestCommandFeature.Request, UnitType>));
            var asyncCommandHandler = resolver.GetHandler(typeof(IAsyncCommandHandler<TestCommandFeature.AsyncRequest, UnitType>));
            Assert.NotNull(requestHandler);
            Assert.NotNull(asyncRequestHandler);
            Assert.NotNull(queryHandler);
            Assert.NotNull(asyncQueryHandler);
            Assert.NotNull(commandHandler);
            Assert.NotNull(asyncCommandHandler);
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
            public class AsyncRequest : IAsyncRequest<UnitType> { }

            public class Handler : IRequestHandler<Request, UnitType>, IAsyncRequestHandler<AsyncRequest, UnitType>
            {
                public UnitType Handle(Request request) => UnitType.Default;
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
}
