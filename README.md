# ![Appeaser](https://raw.githubusercontent.com/carl-berg/appeaser/master/res/icon_256.png)

A mediator implementation in C#, inspired by [a blog post series](http://lostechies.com/jimmybogard/2013/12/19/put-your-controllers-on-a-diet-posts-and-commands/) from Jimmy Bogard. The mediator pattern helps you keep a clean abstraction between your command and queries and where they are executed from. It also helps keeping dependances from bleeding through your abstractions as the handlers are separated from their commands and queries.

## Using the mediator
Executing a query:

	var something = mediator.Request(new GetSomething.Query());

	// async version
	var result = await mediator.Request(new GetSomething.AsyncQuery());
	
Executing a command:

	var result = mediator.Send(new DoSomething.Command());

	// async version
	var result = await mediator.Send(new DoSomething.AsyncCommand());

## Queries

    public class GetSomething
	{
        public class Query : IQuery<Result>
        {
            public Query(/* query parameters */)
            {
                // ...
            }
        }

        //Async version
        public class AsyncQuery : IAsyncQuery<Result>
        {
            public AsyncQuery(/* query parameters */)
            {
                // ...
            }
        }

        public class Handler :  
            IQueryHandler<Query, Result>,
            IAsyncQueryHandler<AsyncQuery, Result>
        {
            public SomethingHandler( /* interesting dependencies here */)
            {
                // ...
            }

            public Result Handle(Query query)
            {
                // ...
            }

            public async Task<Result> Handle(AsyncQuery query)
            {
                // ...
            }
        }

        public class Result { }
    }

## Commands

    public class DoSomething
    {
        public class Command : ICommand<Result>
        {
            public Command(/* command parameters */)
            {
                // ...
            }
        }

        public class AsyncCommand : IAsyncCommand<Result>
        {
            public AsyncCommand(/* command parameters */)
            {
                // ...
            }
        }

        public class Handler :  
            ICommandHandler<Command, Result>,
            IAsyncCommandHandler<AsyncCommand, Result>
        {
            public Handler(/* interesting dependencies here */)
            {
                // ...
            }

            public Result Handle(Command command)
            {
                // ...
            }

            public async Task<Result> Handle(AsyncCommand command)
            {
                // ...
            }
        }

        public class Result { }
    }

## Simple Mediator
The mediator also implements a simpler interface `ISimpleMediator` in case you find the Command/Query names to obtuse and simply want to use a single request method. The simple mediator can be used like this:

    var something = await mediator.Request(new Feature.Request());

with something like this as the implementation:

    public class Feture
    {
        public class Request : IAsyncRequest<Response> { }

	    public class Handler : IAsyncRequestHandler<Request, Response>
	    {
            public Response Handle(Request request) => new Response();
	    }

	    public class Response { }
    }

## Dependency injection
The appeaser library is ment to be used together with dependency injection. As I am not a big believer in adding a dependency resolver for each dependency injection library out there (but in the future I might implement built in dependency injection of some kind), you can easily handle it yourself by implementing the `IMediatorHandlerFactory` interface like this:

    public class MediatorHandlerFactory : IMediatorHandlerFactory
    {
        private readonly IContainer _container;

        public MediatorHandlerFactory(IContainer container)
        {
            _container = container;
        }

        public object GetHandler(Type handlerType)
        {
            return _container.TryGetInstance(handlerType);
        }
    }

... and your your ioc configuration could look like this

	// For Lamar
	public class MyLamarRegistry : ServiceRegistry
	{
	    public MyLamarRegistry()
	    {
            For<IMediator>().Use<Mediator>();
            For<IMediatorHandlerFactory>().Use<MediatorHandlerFactory>();
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
            });			
	    }
	}

	// For StructureMap
	public class MyStructuremapRegistry : Registry
	{
	    public MyStructuremapRegistry()
	    {
            For<IMediator>().Use<Mediator>();
            For<IMediatorSettings>().Use<MediatorSettings>();
            For<IMediatorHandlerFactory>().Use<MediatorHandlerFactory>();
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
            });			
        }
	}

## Interception
Appeaser provides an api for intercepting requests, starting with version 2.2. This can be used for cross cutting concerns like Logging, Validation, Authorization or whatever you want to wrap around your request/response pipeline. Interceptors can be added via mediator settings like so:

	var settings = new MediatorSettings()
        .AddRequestInterceptor<RequestInterceptor>()
        .AddResponseInterceptor<ResponseInterceptor>()
        .AddInterceptor<RequestAndResponseInterceptor>();

    var mediator = new Mediator(handlerFactory, settings);

Any interceptor dependencies will be resolved via the handler factory. If you have a need to separate handler and interceptor resolution there's an option to supply a `IMediatorResolver` instead of a `IMediatorHandlerFactory`.

An interceptor can be implemented like this:

    public class LoggingInterceptor : IRequestInterceptor, IReponseInterceptor
    {
        private ILog _log;
        public LoggingInterceptor(ILog log) => _log = log;

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
            context.Set("Start", DateTime.Now);
            _log.Debug("Executing {Request}", context.Request);
        }

        public void Intercept(IResponseInterceptionContext context)
        {
            if (context.Exception is null)
            {
                _log.Debug(
                    "Executed {Request} successfully and returned {Response} after {Elapsed}", 
                    context.Request, 
                    context.Response, 
                    DateTime.Now - context.Get<DateTime>("Start"));
            }
            else
            {
                _log.Debug("Execution of {Request} failed with {Exception}", context.Exception);
            }
        }
    }

When implementing interceptors that are invoked both on request and response, _be aware_ that a new instance of the class is created on both request and response invocations. If you need to keep state between these invocations, the context provides a key/value -store to store whatever you need to share in between invocations (see example above).