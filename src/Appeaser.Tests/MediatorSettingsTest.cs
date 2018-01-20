using Appeaser.Exceptions;
using System;
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
            Assert.Throws<MediatorQueryException>(() => mediator.Request(new TestFeature.Query()));
        }

        [Fact]
        public void Test_Exception_Wrapping_Can_Be_Disabled()
        {
            var mediator = new Mediator(_handlerFactory, new TestMediatorSettings { WrapExceptions = false });
            Assert.Throws<Exception>(() => mediator.Request(new TestFeature.Query()));
        }

        public class TestFeature
        {
            public class Query : IQuery<Result> { }

            public class Handler : IQueryHandler<Query, Result>
            {
                public Result Handle(Query request)
                {
                    throw new Exception("Expected excepton");
                }
            }

            public class Result { }
        }
    }
}
