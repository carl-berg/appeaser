using NUnit.Framework;

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
        public void Can_Resolve_Command()
        {
            var mediator = new Mediator(_handlerFactory);
            var result = mediator.Send(new TestFeature.Command());
            Assert.IsNotNull(result);
        }

        public class TestFeature
        {
            public class Query : IQuery<Result>, IAsyncQuery<Result> { }
            public class Command : ICommand<Result> { }

            public class Handler : 
                IQueryHandler<Query, Result>,
                ICommandHandler<Command, Result>
            {
                public Result Handle(Query request)
                {
                    return new Result();
                }

                public Result Handle(Command request)
                {
                    return new Result();
                }
            }

            public class Result { }
        }
    }
}
