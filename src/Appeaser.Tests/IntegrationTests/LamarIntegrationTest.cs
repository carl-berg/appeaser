using System;
using System.Threading.Tasks;
using Lamar;
using Xunit;

namespace Appeaser.Tests.IntegrationTests
{
    public class LamarIntegrationTest : IntegrationTestBase
    {
        private Container _container;

        public LamarIntegrationTest()
        {
            _container = new Container(configure =>
            {
                configure.For<IMediatorHandlerFactory>().Use<LamarMediatorHandlerFactory>();
                configure.For<IMediator>().Use<Mediator>();
                configure.For<ISimpleMediator>().Use<Mediator>();
                configure.Scan(s =>
                {
                    s.AssemblyContainingType<LamarIntegrationTest>();
                    s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                });
            });
        }

        [Fact]
        public void Can_Resolve_Mediator()
        {
            var mediator = _container.GetInstance<IMediator>();
            Assert.NotNull(mediator);
            Assert.IsType<Mediator>(mediator);
        }

        [Fact]
        public void Can_Resolve_Simple_Mediator()
        {
            var mediator = _container.GetInstance<ISimpleMediator>();
            Assert.NotNull(mediator);
            Assert.IsType<Mediator>(mediator);
        }

        [Fact]
        public void Can_Resolve_Query_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Request(new QueryFeature.Query());
            Assert.Equal(UnitType.Default, result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Query_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = await mediator.Request(new QueryFeature.AsyncQuery());
            Assert.Equal(UnitType.Default, result);
        }

        [Fact]
        public void Can_Resolve_Command_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Send(new CommandFeature.Command());
            Assert.Equal(UnitType.Default, result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Command_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = await mediator.Send(new CommandFeature.AsyncCommand());
            Assert.Equal(UnitType.Default, result);
        }

        [Fact]
        public void Configuration_disposes_disposables_in_nested_container()
        {
            TestDisposable result;
            using (var nestedContainer = _container.GetNestedContainer())
            {
                var mediator = nestedContainer.GetInstance<ISimpleMediator>();
                result = mediator.Request(new DisposableFeature.Request());
            }

            Assert.True(result.IsDisposed);
        }

        public class LamarMediatorHandlerFactory : IMediatorHandlerFactory
        {
            private readonly IServiceContext _serviceContext;

            public LamarMediatorHandlerFactory(IServiceContext serviceContext)
            {
                _serviceContext = serviceContext;
            }

            public object GetHandler(Type handlerType)
            {
                return _serviceContext.TryGetInstance(handlerType);
            }
        }
    }
}
