using StructureMap;
using System;
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
        public void TestSomething()
        {
            var mediator = _container.GetInstance<IMediator>();
            var result = mediator.Request(new Feature.Query());
            Assert.Equal(UnitType.Default, result);
        }

        public class Feature
        {
            public class Query : IQuery<UnitType> { }

            public class Handler : IQueryHandler<Query, UnitType>
            {
                public UnitType Handle(Query request)
                {
                    return UnitType.Default;
                }
            }
        }

        public class StructuremapRegistry : Registry
        {
            public StructuremapRegistry()
            {
                For<IMediatorHandlerFactory>().Use<StructuremapHandlerFactory>();
                For<IMediatorSettings>().Use<MediatorSettings>();
                For<IMediator>().Use<Mediator>();
                Scan(s =>
                {
                    s.AssemblyContainingType<StructuremapRegistry>();
                    s.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncQueryHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncCommandHandler<,>));
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
