using FakeItEasy;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Appeaser.Tests
{
    public class PipelineTests
    {
        private IMediatorHandlerFactory _handler;

        public PipelineTests()
        {
            _handler = A.Fake<IMediatorHandlerFactory>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ICommandHandler<CommandFeature.Command, UnitType>))))
                .Returns(new CommandFeature.Handler());
        }

        [Fact]
        public void TestRequestInterception()
        {
            var interceptor = new RequestInterceptor();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(RequestInterceptor)))).Returns(interceptor);
            var settings = new MediatorSettings().AddRequestInterceptor<RequestInterceptor>();
            var mediator = new Mediator(_handler, settings);

            var command = new CommandFeature.Command();
            mediator.Send(command);

            Assert.True(interceptor.HasBeenIntercepted);
            Assert.Same(command, interceptor.Request);
        }

        [Fact]
        public void TestResponseInterception()
        {
            var interceptor = new ResponseInterceptor();
            var settings = new MediatorSettings().AddResponseInterceptor<ResponseInterceptor>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ResponseInterceptor)))).Returns(interceptor);
            var mediator = new Mediator(_handler, settings);

            var command = new CommandFeature.Command();
            var response = mediator.Send(command);

            Assert.True(interceptor.HasBeenIntercepted);
            Assert.Same(command, interceptor.Request);
            Assert.Same(response, interceptor.Response);
        }

        [Fact]
        public void TestInterception()
        {
            var interceptor = new Interceptor();
            var settings = new MediatorSettings().AddInterceptor<Interceptor>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(Interceptor)))).Returns(interceptor);
            var mediator = new Mediator(_handler, settings);

            mediator.Send(new CommandFeature.Command());

            Assert.True(interceptor.RequestWasIntercepted);
            Assert.True(interceptor.ResponseWasIntercepted);
        }

        public class Interceptor : IRequestInterceptor, IResponseInterceptor
        {
            public void Intercept(IRequestInterceptionContext context)
            {
                RequestWasIntercepted = true;
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                ResponseWasIntercepted = true;
            }

            public bool RequestWasIntercepted { get; set; }
            public bool ResponseWasIntercepted { get; set; }
        }

        public class RequestInterceptor : IRequestInterceptor
        {
            public void Intercept(IRequestInterceptionContext context)
            {
                Request = context.Request;
                HasBeenIntercepted = true;
            }

            public bool HasBeenIntercepted { get; set; }
            public object Request { get; set; }
        }

        public class ResponseInterceptor : IResponseInterceptor
        {
            public void Intercept(IResponseInterceptionContext context)
            {
                Request = context.Request;
                Response = context.Response;
                HasBeenIntercepted = true;
            }

            public bool HasBeenIntercepted { get; set; }
            public object Request { get; set; }
            public object Response { get; set; }
        }

        public class CommandFeature
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
