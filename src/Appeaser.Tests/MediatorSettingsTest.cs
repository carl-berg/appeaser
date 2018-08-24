using System;
using Appeaser.Exceptions;
using Xunit;

namespace Appeaser.Tests
{
    public class MediatorSettingsTest
    {
        private TestHandlerFactory _handlerFactory;

        public MediatorSettingsTest()
        {
            _handlerFactory = new TestHandlerFactory()
                .AddHandler<TestFeature.Handler>();
        }

        [Fact]
        public void Test_Request_Exception_Is_Wrapped_By_Default()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorRequestException>(() => mediator.Request(new TestFeature.Request()));
        }

        [Fact]
        public void Test_Query_Exception_Is_Wrapped_By_Default()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorQueryException>(() => mediator.Request(new TestFeature.Query()));
        }

        [Fact]
        public void Test_Command_Exception_Is_Wrapped_By_Default()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorCommandException>(() => mediator.Send(new TestFeature.Command()));
        }

        [Fact]
        public void Test_Request_Exception_Wrapping_Can_Be_Disabled()
        {
            var mediator = new Mediator(_handlerFactory, new TestMediatorSettings { WrapExceptions = false });
            Assert.Throws<Exception>(() => mediator.Request(new TestFeature.Request()));
        }

        [Fact]
        public void Test_Query_Exception_Wrapping_Can_Be_Disabled()
        {
            var mediator = new Mediator(_handlerFactory, new TestMediatorSettings { WrapExceptions = false });
            Assert.Throws<Exception>(() => mediator.Request(new TestFeature.Query()));
        }

        [Fact]
        public void Test_Command_Exception_Wrapping_Can_Be_Disabled()
        {
            var mediator = new Mediator(_handlerFactory, new TestMediatorSettings { WrapExceptions = false });
            Assert.Throws<Exception>(() => mediator.Request(new TestFeature.Command()));
        }

        public class TestFeature
        {
            public class Request : IRequest<Result> { }
            public class Query : IQuery<Result> { }
            public class Command : ICommand<Result> { }

            public class Handler : 
                IRequestHandler<Request, Result>,
                IQueryHandler<Query, Result>,
                ICommandHandler<Command, Result>
            {
                public Result Handle(Request request) => throw new Exception("Expected excepton");
                public Result Handle(Query query) => throw new Exception("Expected excepton");
                public Result Handle(Command command) => throw new Exception("Expected excepton");
            }

            public class Result { }
        }
    }
}
