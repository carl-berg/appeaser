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
        public void Test_Exception_Is_Wrapped_By_Default()
        {
            var mediator = new Mediator(_handlerFactory);
            Assert.Throws<MediatorRequestException>(() => mediator.Request(new TestFeature.Request()));
        }

        [Fact]
        public void Test_Exception_Wrapping_Can_Be_Disabled()
        {
            var mediator = new Mediator(_handlerFactory, new TestMediatorSettings { WrapExceptions = false });
            Assert.Throws<Exception>(() => mediator.Request(new TestFeature.Request()));
        }

        public class TestFeature
        {
            public class Request : IRequest<Result> { }

            public class Handler : IRequestHandler<Request, Result>
            {
                public Result Handle(Request request)
                {
                    throw new Exception("Expected excepton");
                }
            }

            public class Result { }
        }
    }
}
