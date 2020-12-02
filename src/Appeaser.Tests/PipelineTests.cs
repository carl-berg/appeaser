using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Appeaser.Interception;
using FakeItEasy;
using Shouldly;
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
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(IAsyncRequestHandler<DiagnosticActivity.AsyncRequest, string>))))
                .Returns(new DiagnosticActivity.Handler());
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(IRequestHandler<DiagnosticActivity.Request, string>))))
                .Returns(new DiagnosticActivity.Handler());
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
        public void TestInterceptionAsync()
        {
            var interceptor = new Interceptor();
            var settings = new MediatorSettings().AddInterceptor<Interceptor>();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(Interceptor)))).Returns(interceptor);
            var mediator = new Mediator(_handler, settings);

            mediator.Send(new CommandFeature.AsyncCommand());

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

            var requests = first.ContextCopy.Get<List<string>>("RequestInvocation");
            var responses = first.ContextCopy.Get<List<string>>("ResponseInvocation");

            Assert.Equal(new[] { "first", "second" }, requests);
            Assert.Equal(new[] { "second", "first" }, responses);
            Assert.Equal(first.ContextCopy.Get<string>("Scope"), second.ContextCopy.Get<string>("Scope"));
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

        [Fact]
        public void TestDiagnosticActivityInterception()
        {
            var rootActivity = new Activity("Root").Start();
            var interceptor = new DiagnosticActivity.Interceptor();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(DiagnosticActivity.Interceptor)))).Returns(interceptor);
            var settings = new MediatorSettings().AddInterceptor<DiagnosticActivity.Interceptor>();
            var mediator = new Mediator(_handler, settings);

            var activityId = mediator.Request(new DiagnosticActivity.Request());

            activityId.ShouldNotBe(rootActivity.Id);

            rootActivity.Stop();
        }

        [Fact]
        public async Task TestAsyncDiagnosticActivityInterception()
        {
            var rootActivity = new Activity("Root").Start();
            var interceptor = new DiagnosticActivity.Interceptor();
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(DiagnosticActivity.Interceptor)))).Returns(interceptor);
            var settings = new MediatorSettings().AddInterceptor<DiagnosticActivity.Interceptor>();
            var mediator = new Mediator(_handler, settings);

            var activityId = await mediator.Request(new DiagnosticActivity.AsyncRequest());

            activityId.ShouldNotBe(rootActivity.Id);

            rootActivity.Stop();
        }

        [Fact]
        public void Test_RequestInterception_Resolution_Fails()
        {
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(RequestInterceptor)))).Returns(null);
            var settings = new MediatorSettings{ WrapExceptions = false }.AddRequestInterceptor<RequestInterceptor>();
            var mediator = new Mediator(_handler, settings);

            Assert.Throws<MediatorInterceptionResolutionException>(() => mediator.Send(new CommandFeature.Command()));
        }

        [Fact]
        public void Test_ResponseInterception_Resolution_Fails()
        {
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ResponseInterceptor)))).Returns(null);
            var settings = new MediatorSettings { WrapExceptions = false }.AddResponseInterceptor<ResponseInterceptor>();
            var mediator = new Mediator(_handler, settings);

            Assert.Throws<MediatorInterceptionResolutionException>(() => mediator.Send(new CommandFeature.Command()));
        }

        [Fact]
        public void Test_Async_RequestInterception_Resolution_Fails()
        {
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(RequestInterceptor)))).Returns(null);
            var settings = new MediatorSettings { WrapExceptions = false }.AddRequestInterceptor<RequestInterceptor>();
            var mediator = new Mediator(_handler, settings);

            Assert.ThrowsAsync<MediatorInterceptionResolutionException>(() => mediator.Send(new CommandFeature.AsyncCommand()));
        }

        [Fact]
        public void Test_Async_ResponseInterception_Resolution_Fails()
        {
            A.CallTo(() => _handler.GetHandler(A<Type>.That.IsEqualTo(typeof(ResponseInterceptor)))).Returns(null);
            var settings = new MediatorSettings { WrapExceptions = false }.AddResponseInterceptor<ResponseInterceptor>();
            var mediator = new Mediator(_handler, settings);

            Assert.ThrowsAsync<MediatorInterceptionResolutionException>(() => mediator.Send(new CommandFeature.AsyncCommand()));
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

            public IContext ContextCopy { get; set; }

            public void Intercept(IRequestInterceptionContext context)
            {
                ContextCopy = context;
                AddToList("RequestInvocation", _name);
            }

            public void Intercept(IResponseInterceptionContext context)
            {
                ContextCopy = context;
                AddToList("ResponseInvocation", _name);
            }

            public Task InterceptAsync(IRequestInterceptionContext context) => Task.CompletedTask;

            public Task InterceptAsync(IResponseInterceptionContext context) => Task.CompletedTask;

            private void AddToList(string key, string value)
            {
                var list = ContextCopy.Get<List<string>>(key) ?? new List<string>();
                list.Add(value);
                ContextCopy.Set(key, list);
            }
        }

        public class CommandFeature
        {
            public class Command : ICommand<Response>
            {
                public bool TriggerException { get; set; }
            }

            public class AsyncCommand : IAsyncCommand<Response>
            {
                public bool TriggerException { get; set; }
            }

            public class Handler : 
                ICommandHandler<Command, Response>,
                IAsyncCommandHandler<AsyncCommand, Response>
            {
                public Response Handle(Command command)
                {
                    if (command.TriggerException)
                    {
                        throw new ArgumentException();
                    }

                    return new Response();
                }

                public Task<Response> Handle(AsyncCommand command)
                {
                    if (command.TriggerException)
                    {
                        throw new ArgumentException();
                    }

                    return Task.FromResult(new Response());
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

        public class DiagnosticActivity
        {
            public class Request : IRequest<string> { }
            public class AsyncRequest : IAsyncRequest<string> { }

            public class Handler :
                IRequestHandler<Request, string>,
                IAsyncRequestHandler<AsyncRequest, string>
            {
                public string Handle(Request request) => Activity.Current.Id;

                public Task<string> Handle(AsyncRequest request)
                {
                    return Task.FromResult(Activity.Current.Id);
                }
            }

            public class Interceptor : IRequestInterceptor, IResponseInterceptor
            {
                public void Intercept(IRequestInterceptionContext context)
                {
                    context.Set("Activity", new Activity("Child").Start());
                }

                public void Intercept(IResponseInterceptionContext context)
                {
                    context.Get<Activity>("Activity").Stop();
                }

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
            }
        }

    }

}
