using System;
using System.Collections.Generic;
using System.Threading;
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
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(IAsyncCommandHandler<Async.Command, UnitType>))))
                .Returns(new Async.Handler());
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

        [Fact]
        public void TestInterceptionInvocationOrder()
        {
            var first = new FirstInterceptor();
            var second = new SecondInterceptor();
            var settings = new MediatorSettings()
                .AddInterceptor<FirstInterceptor>()
                .AddInterceptor<SecondInterceptor>();

            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(FirstInterceptor)))).Returns(first);
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(SecondInterceptor)))).Returns(second);

            var mediator = new Mediator(_handler, settings);

            mediator.Send(new CommandFeature.Command());

            var requests = first.ContextCopy["RequestInvocation"] as List<string>;
            var responses = first.ContextCopy["ResponseInvocation"] as List<string>;

            Assert.Equal(new[] { "first", "second" }, requests);
            Assert.Equal(new[] { "second", "first" }, responses);
        }

        [Fact]
        public async Task TestInterceptionAsynchronisity()
        {
            var interceptor = new Async.RequestInterceptor();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(Async.RequestInterceptor)))).Returns(interceptor);
            var settings = new MediatorSettings().AddRequestInterceptor<Async.RequestInterceptor>();
            var mediator = new Mediator(_handler, settings);

            var command = new Async.Command();
            var task = mediator.Send(command);
            if (!await command.WaitAsync2(2000))
            {
                Assert.False(true);
            }

            command.Release();
            await task;

            Assert.True(interceptor.HasBeenIntercepted);
            Assert.True(command.HandlerHasBeenInvoked);
        }

        public class Interceptor : IRequestInterceptor, IResponseInterceptor
        {
            public bool RequestWasIntercepted { get; set; }
            public bool ResponseWasIntercepted { get; set; }

            public Task InterceptAsync(IRequestInterceptionContext context)
            {
                Intercept(context);
                return Task.CompletedTask;
            }

            public Task InterceptAsync(IResponseInterceptionContext context)
            {
                Intercept(context);
                return Task.CompletedTask;
            }

            public void Intercept(IRequestInterceptionContext context)
            {
                RequestWasIntercepted = true;
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                ResponseWasIntercepted = true;
            }
        }

        public class RequestInterceptor : IRequestInterceptor
        {
            public bool HasBeenIntercepted { get; set; }
            public object Request { get; set; }

            public Task InterceptAsync(IRequestInterceptionContext context)
            {
                Intercept(context);
                return Task.CompletedTask;
            }

            public void Intercept(IRequestInterceptionContext context)
            {
                Request = context.Request;
                HasBeenIntercepted = true;
            }
        }

        public class ResponseInterceptor : IResponseInterceptor
        {
            public IResponseInterceptionContext Context { get; set; }

            public Task InterceptAsync(IResponseInterceptionContext context)
            {
                Intercept(context);
                return Task.CompletedTask;
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                Context = context;
                if (context.Response is CommandFeature.Response response)
                {
                    response.AlteredByInterception = true;
                }
            }
        }

        public class ExceptionInterceptor : IResponseInterceptor
        {
            public Exception CaughtException { get; private set; }

            public Task InterceptAsync(IResponseInterceptionContext context)
            {
                Intercept(context);
                return Task.CompletedTask;
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                CaughtException = context.Exception;
            }
        }

        public class FirstInterceptor : NamedInterceptor
        {
            public FirstInterceptor() : base("first") { }
        }

        public class SecondInterceptor : NamedInterceptor
        {
            public SecondInterceptor() : base("second") { }
        }

        public abstract class NamedInterceptor : IRequestInterceptor, IResponseInterceptor
        {
            private readonly string _name;
            public NamedInterceptor(string name) => _name = name;

            public IDictionary<string, object> ContextCopy { get; set; }

            public void Intercept(IRequestInterceptionContext context)
            {
                ContextCopy = context.Context;
                ContextCopy["RequestInvocation"] = AddToList("RequestInvocation", _name);
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                ContextCopy = context.Context;
                ContextCopy["ResponseInvocation"] = AddToList("ResponseInvocation", _name);
            }

            public Task InterceptAsync(IRequestInterceptionContext context) => Task.CompletedTask;

            public Task InterceptAsync(IResponseInterceptionContext context) => Task.CompletedTask;

            private List<string> AddToList(string key, string value)
            {
                var list = ContextCopy.TryGetValue(key, out object v) && v is List<string> l ? l : new List<string>();
                list.Add(value);
                return list;
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

        public class Async
        {
            public class Command : IAsyncCommand<UnitType>
            {
                private SemaphoreSlim _s;
                private SemaphoreSlim _s2;

                public Command()
                {
                    _s = new SemaphoreSlim(0, 1);
                    _s2 = new SemaphoreSlim(0, 1);
                }

                public bool HandlerHasBeenInvoked { get; set; } = false;

                public void Release()
                {
                    _s.Release();
                }

                public void Release2()
                {
                    _s2.Release();
                }

                public async Task<bool> WaitAsync(int timeout)
                {
                    return await _s.WaitAsync(timeout);
                }

                public async Task<bool> WaitAsync2(int timeout)
                {
                    return await _s2.WaitAsync(timeout);
                }
            }

            public class Handler : IAsyncCommandHandler<Command, UnitType>
            {
                public async Task<UnitType> Handle(Command command)
                {
                    if (await command.WaitAsync(2000))
                    {
                        command.HandlerHasBeenInvoked = true;
                    }

                    return UnitType.Default;
                }
            }

            public class RequestInterceptor : IRequestInterceptor
            {
                public bool HasBeenIntercepted { get; set; }
                public Command Request { get; set; }

                public async Task InterceptAsync(IRequestInterceptionContext context)
                {
                    Request = (Command)context.Request;
                    Request.Release2();
                    if (await Request.WaitAsync(5000))
                    {
                        HasBeenIntercepted = true;
                        Request.Release();
                    }
                }

                public void Intercept(IRequestInterceptionContext context) { }
            }
        }
    }
}
