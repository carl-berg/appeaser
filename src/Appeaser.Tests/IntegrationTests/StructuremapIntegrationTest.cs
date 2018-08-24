using System;
using System.Threading.Tasks;
using StructureMap;
using Xunit;

namespace Appeaser.Tests
{
    public class StructuremapIntegrationTest : TestBase
    {
        private Container _container;

        public StructuremapIntegrationTest()
        {
            _container = new Container(configure =>
            {
                configure.For<IMediatorHandlerFactory>().Use<StructuremapMediatorHandlerFactory>().Singleton();
                configure.For<IMediatorSettings>().Use<MediatorSettings>();
                configure.For<IMediator>().Use<Mediator>();
                configure.Scan(s =>
                {
                    s.AssemblyContainingType<StructuremapIntegrationTest>();
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
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

        public class StructuremapMediatorHandlerFactory : IMediatorHandlerFactory
        {
            private readonly IContainer _container;

            public StructuremapMediatorHandlerFactory(IContainer container)
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
