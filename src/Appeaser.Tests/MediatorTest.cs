using Appeaser.Exceptions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Appeaser.Tests
{
    [TestFixture]
    public class MediatorTest
    {
        private TestHandlerFactory _handlerFactory;

        public MediatorTest()
        {
            _handlerFactory = new TestHandlerFactory()
                .AddHandler<TestFeature.Handler>();
        }

        [Test]
        public void Can_Resolve_Query()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = mediator.Request(new TestFeature.Query());
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task Can_Resolve_Async_Query()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = await mediator.Request(new TestFeature.AsyncQuery());
            Assert.IsNotNull(result);
        }

        [Test]
        public void Can_Resolve_Command()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = mediator.Send(new TestFeature.Command());
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task Can_Resolve_Async_Command()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = await mediator.Send(new TestFeature.AsyncCommand());
            Assert.IsNotNull(result);
        }

        [Test]
        public void Calling_Unregistered_Handler_Throws_Exception()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorQueryException>(() => mediator.Request(new AnotherTestFeature.Query()));
        }

        public class TestFeature
        {
            public class Query : IQuery<Result> { }
            public class AsyncQuery : IAsyncQuery<Result> { }
            public class Command : ICommand<Result> { }
            public class AsyncCommand : IAsyncCommand<Result> { }

            public class Handler : 
                IQueryHandler<Query, Result>,
                IAsyncQueryHandler<AsyncQuery, Result>,
                ICommandHandler<Command, Result>,
                IAsyncCommandHandler<AsyncCommand, Result>
            {
                public Result Handle(Query request)
                {
                    return new Result();
                }

                public Task<Result> Handle(AsyncQuery request)
                {
                    return Task.FromResult(new Result());
                }

                public Result Handle(Command request)
                {
                    return new Result();
                }

                public Task<Result> Handle(AsyncCommand request)
                {
                    return Task.FromResult(new Result());
                }
            }

            public class Result { }
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
