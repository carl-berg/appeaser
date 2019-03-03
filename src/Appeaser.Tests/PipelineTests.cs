using System;
using System.Threading.Tasks;
using Appeaser.Interception;
using FakeItEasy;
using Xunit;

namespace Appeaser.Tests
{
    public class PipelineTests
    {
        private IMediatorHandlerFactory _handler;

        public PipelineTests()
        {
            _handler = A.Fake<IMediatorHandlerFactory>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ICommandHandler<CommandFeature.Command, CommandFeature.Response>))))
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

            Assert.Same(command, interceptor.Context.Request);
            Assert.Same(response, interceptor.Context.Response);
            Assert.True(response.AlteredByInterception);
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

            // Assert interceptor that handles both request and response is not constructed twice
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(Interceptor))))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void TestReponseExceptionInterception()
        {
            var interceptor = new ResponseInterceptor();
            var settings = new MediatorSettings { WrapExceptions = false }.AddResponseInterceptor<ResponseInterceptor>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ResponseInterceptor)))).Returns(interceptor);
            var mediator = new Mediator(_handler, settings);

            var command = new CommandFeature.Command { TriggerException = true };

            Assert.Throws<ArgumentException>(() => mediator.Send(command));

            Assert.Same(command, interceptor.Context.Request);
            Assert.IsType<ArgumentException>(interceptor.Context.Exception);
        }

        public class Interceptor : IRequestInterceptor, IResponseInterceptor
        {
            public bool RequestWasIntercepted { get; set; }
            public bool ResponseWasIntercepted { get; set; }

            public Task Intercept(IRequestInterceptionContext context)
            {
                RequestWasIntercepted = true;
                return Task.CompletedTask;
            }

            public Task Intercept(IResponseInterceptionContext context)
            {
                ResponseWasIntercepted = true;
                return Task.CompletedTask;
            }
        }

        public class RequestInterceptor : IRequestInterceptor
        {
            public bool HasBeenIntercepted { get; set; }
            public object Request { get; set; }

            public Task Intercept(IRequestInterceptionContext context)
            {
                Request = context.Request;
                HasBeenIntercepted = true;
                return Task.CompletedTask;
            }
        }

        public class ResponseInterceptor : IResponseInterceptor
        {
            public IResponseInterceptionContext Context { get; set; }

            public Task Intercept(IResponseInterceptionContext context)
            {
                Context = context;
                if (context.Response is CommandFeature.Response response)
                {
                    response.AlteredByInterception = true;
                }

                return Task.CompletedTask;
            }
        }

        public class ExceptionInterceptor : IResponseInterceptor
        {
            public Exception CaughtException { get; private set; }

            public Task Intercept(IResponseInterceptionContext context)
            {
                CaughtException = context.Exception;
                return Task.CompletedTask;
            }
        }

        public class CommandFeature
        {
            public class Command : ICommand<Response>
            {
                public bool TriggerException { get; set; }
            }

            public class Handler : ICommandHandler<Command, Response>
            {
                public Response Handle(Command command)
                {
                    if (command.TriggerException)
                    {
                        throw new ArgumentException();
                    }

                    return new Response();
                }
            }

            public class Response
            {
                public bool AlteredByInterception { get; set; }
            }
        }
    }
}
