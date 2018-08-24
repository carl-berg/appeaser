using System;
using System.Threading.Tasks;
using Lamar;
using Xunit;

namespace Appeaser.Tests
{
    public class LamarIntegrationTest : TestBase
    {
        private Container _container;

        public LamarIntegrationTest()
        {
            _container = new Container(configure =>
            {
                configure.ForSingletonOf<IMediatorHandlerFactory>().Use<LamarMediatorHandlerFactory>();
                configure.For<IMediator>().Use<Mediator>();
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

        public class LamarMediatorHandlerFactory : IMediatorHandlerFactory
        {
            private readonly IContainer _container;

            public LamarMediatorHandlerFactory(IContainer container)
            {
                _container = container;
            }

            public object GetHandler(Type handlerType)
            {
                return _container.TryGetInstance(handlerType);
            }
        }
    }
}
