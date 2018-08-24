using System;
using System.Threading.Tasks;
using StructureMap;
using Xunit;

namespace Appeaser.Tests
{
    public class StructuremapIntegrationTest
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
                    s.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncQueryHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncCommandHandler<,>));
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
        public void Can_Resolve_Request_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Request(new TestFeatureOne.Request());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Request_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = await mediator.Request(new TestFeatureOne.AsyncRequest());
            Assert.NotNull(result);
        }

        [Fact]
        public void Can_Resolve_Command_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Send(new TestFeatureTwo.Command());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Command_Handlers()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = await mediator.Send(new TestFeatureTwo.AsyncCommand());
            Assert.NotNull(result);
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

        public class TestFeatureOne
        {
            public class Request : IQuery<UnitType> { }
            public class AsyncRequest : IAsyncQuery<UnitType> { }

            public class Handler : IQueryHandler<Request, UnitType>, IAsyncQueryHandler<AsyncRequest, UnitType>
            {
                public UnitType Handle(Request request) => UnitType.Default;

                public async Task<UnitType> Handle(AsyncRequest request) => await Task.FromResult(UnitType.Default);
            }
        }

        public class TestFeatureTwo
        {
            public class Command : ICommand<UnitType> { }
            public class AsyncCommand : IAsyncCommand<UnitType> { }

            public class Handler : ICommandHandler<Command, UnitType>, IAsyncCommandHandler<AsyncCommand, UnitType>
            {
                public UnitType Handle(Command command) => UnitType.Default;

                public async Task<UnitType> Handle(AsyncCommand command) => await Task.FromResult(UnitType.Default);
            }
        }
    }
}
