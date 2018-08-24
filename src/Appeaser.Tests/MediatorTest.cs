using System.Threading.Tasks;
using Appeaser.Exceptions;
using Xunit;

namespace Appeaser.Tests
{
    public class MediatorTest : TestBase
    {
        private TestHandlerFactory _handlerFactory;

        public MediatorTest()
        {
            _handlerFactory = new TestHandlerFactory()
                .AddHandler<RequestFeature.Handler>()
                .AddHandler<QueryFeature.Handler>()
                .AddHandler<CommandFeature.Handler>();
        }

        [Fact]
        public void Can_Resolve_Query()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = mediator.Request(new QueryFeature.Query());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Query()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = await mediator.Request(new QueryFeature.AsyncQuery());
            Assert.NotNull(result);
        }

        [Fact]
        public void Can_Resolve_Command()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = mediator.Send(new CommandFeature.Command());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Can_Resolve_Async_Command()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = await mediator.Send(new CommandFeature.AsyncCommand());
            Assert.NotNull(result);
        }

        [Fact]
        public void Calling_Unregistered_Handler_Throws_Exception()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorQueryException>(() => mediator.Request(new AnotherTestFeature.Query()));
        }

        public class AnotherTestFeature
        {
            public class Query : IQuery<Result> { }

            public class Handler : IQueryHandler<Query, Result>
            {
                public Result Handle(Query query)
                {
                    return new Result();
                }
            }

            public class Result { }
        }
    }
}
