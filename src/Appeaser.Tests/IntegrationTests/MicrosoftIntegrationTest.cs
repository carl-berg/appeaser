using System.Linq;
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
        public void HasAddedHandlerToServiceCollection()
        {
            var registeredHandler =_services.FirstOrDefault(dep => dep.ImplementationType == typeof(TestFeature.Handler));
            Assert.NotNull(registeredHandler);
        }

        [Fact]
        public void ProviderCanResolveRequestHandler()
        {
            var resolver = _provider.GetService<IMediatorResolver>();
            var resolvedHandler = resolver.GetHandler(typeof(IRequestHandler<TestFeature.Request, UnitType>));
            Assert.NotNull(resolvedHandler);
        }

        [Fact]
        public void TestInvokeMediator()
        {
            var mediator = _provider.GetService<ISimpleMediator>();
            var response = mediator.Request(new TestFeature.Request());
            Assert.Equal(UnitType.Default, response);
        }

        public class TestFeature
        {
            public class Request : IRequest<UnitType> { }

            public class Handler : IRequestHandler<Request, UnitType>
            {
                public UnitType Handle(Request request) => UnitType.Default;
            }
        }
    }
}
