using System;
using StructureMap;
using Xunit;

namespace Appeaser.Tests
{
    public partial class DependencyInjectionTests
    {
        private Container _container;

        public DependencyInjectionTests()
        {
            _container = new Container(new StructuremapRegistry());
        }

        [Fact]
        public void TestMediatorResolution()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Request(new Feature.Query());
            Assert.Equal(UnitType.Default, result);
        }

        [Fact]
        public void TestSimpleMediatorResolution()
        {
            var mediator = _container.GetInstance<ISimpleMediator>();
            var result = mediator.Request(new Feature.Query());
            Assert.Equal(UnitType.Default, result);
        }

        public class Feature
        {
            public class Query : Request, IQuery<UnitType> { }

            public class Command : Request, ICommand<UnitType> { }

            public class Request : IRequest<UnitType> { }

            public class Handler : 
                IRequestHandler<Request, UnitType>,
                IQueryHandler<Query, UnitType>,
                ICommandHandler<Command, UnitType>
            {
                public UnitType Handle(Request request)
                {
                    return UnitType.Default;
                }

                public UnitType Handle(Query q) => Handle(q);

                public UnitType Handle(Command c) => Handle(c);
            }
        }

        public class StructuremapRegistry : Registry
        {
            public StructuremapRegistry()
            {
                For<IMediatorHandlerFactory>().Use<StructuremapHandlerFactory>();
                For<IMediatorSettings>().Use<MediatorSettings>();
                For<ISimpleMediator>().Use<Mediator>();
                For<IMediator>().Use<Mediator>();
                Scan(s =>
                {
                    s.AssemblyContainingType<StructuremapRegistry>();
                    s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                });
            }
        }

        public class StructuremapHandlerFactory : IMediatorHandlerFactory
        {
            private readonly IContainer _container;

            public StructuremapHandlerFactory(IContainer container)
            {
                _container = container;
            }

            public object GetHandler(Type handlerType)
            {
                return _container.GetInstance(handlerType);
            }
        }
    }
}
