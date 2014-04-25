# ![Appeaser](https://raw.githubusercontent.com/carl-berg/appeaser/master/res/icon_256.png)

A mediator implementation in C#, inspired by [a blog post series](http://lostechies.com/jimmybogard/2013/12/19/put-your-controllers-on-a-diet-posts-and-commands/) from Jimmy Bogard. The mediator pattern helps you keep a clean abstraction between your command and queries and where they are executed from. It also helps keeping dependances from bleeding through your abstractions as the handlers are separated from their commands and queries.

## Using the mediator
Executing a query:

	var something = mediator.Request(new GetSomething());

	//Async version
	var result = await mediator.Request(new GetSomethingAsync());
	
Executing a command:

	var result = mediator.Send(new DoSomething());

	//Async version
	var result = await mediator.Send(new DoSomethingAsync());


## Queries
	public class GetSomething : IQuery<Something>
	{
		public GetSomething(/* query parameters */)
		{
			// ...
		}
	}

	//Async version
	public class GetSomethingAsync : IAsyncQuery<Something>
	{
		public GetSomething(/* query parameters */)
		{
			// ...
		}
	}


## Commands
	public class DoSomething : ICommand<Something>
	{
		public DoSomething(/* command parameters */)
		{
			// ...
		}
	}

	//Async version
	public class DoSomethingAsync : IAsyncCommand<Something>
	{
		public DoSomething(/* command parameters */)
		{
			// ...
		}
	}

## Query Handler
	public class SomethingQueryHandler :  
		IQueryHandler<GetSomething, Something>,
		IAsyncQueryHandler<GetSomethingAsync, Something>
	{
		public SomethingHandler( /* interesting dependencies here */)
		{
			// ...
		}

		public Something Handle(GetSomething query)
		{
			// ...
		}

		public async Task<Something> Handle(GetSomethingAsync query)
		{
			// ...
		}
	}

## Command Handler
	public class SomethingCommandHandler :  
		ICommandHandler<DoSomething, SomethingResult>,
		IAsyncCommandHandler<DoSomethingAsync, SomethingResult>
	{
		public SomethingHandler(/* interesting dependencies here */)
		{
			// ...
		}

		public SomethingResult Handle(DoSomething command)
		{
			// ...
		}

		public async Task<SomethingResult> Handle(DoSomethingAsync command)
		{
			// ...
		}
	}

## Dependency injection
The appeaser library is ment to be used together with dependency injection. As I am not a big believer in adding a dependency resolver for each dependency injection library out there (but in the future I might implement built in dependency injection of some kind), you can easily handle it yourself by implementing the `IMediatorHandlerFactory` interface like this:

    public class StructuremapMediatorHandlerFactory : IMediatorHandlerFactory
    {
        private readonly IContainer _container;

        public StructuremapMediatorHandlerFactory(IContainer container)
        {
            _container = container;
        }

        public object GetHandler(Type handlerType)
        {
            return _container.TryGetInstance(handlerType);
        }
    }

... and in your structuremap registry, scan your handlers like this:

	public class MyStructuremapRegistry : Registry
	{
		public MyStructuremapRegistry()
		{
            For<IMediator>().Use<Mediator>();
            For<IMediatorHandlerFactory>().Use<StructuremapMediatorHandlerFactory>();
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
                s.ConnectImplementationsToTypesClosing(typeof(IAsyncQueryHandler<,>));
                s.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>));
                s.ConnectImplementationsToTypesClosing(typeof(IAsyncCommandHandler<,>));
            });			
		}
	}