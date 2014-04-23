![Appeaser](https://raw.githubusercontent.com/carl-berg/appeaser/master/res/icon_256.png)
----------
A mediator implementation in C#, inspired by [a blog post series](http://lostechies.com/jimmybogard/2013/12/19/put-your-controllers-on-a-diet-posts-and-commands/) from Jimmy Bogard. The mediator pattern helps you keep a clean abstraction between your command and queries and where they are executed from. It also helps keeping dependances from bleeding through your abstractions as the handlers are separated from their commands and queries.

## Using the mediator
Executing a query:

	var something = mediator.Request(new GetSomething());
	
Executing a command:

	var result = mediator.Send(new DoSomething());


## Queries
	public class GetSomething : IQuery<Something>
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

## Query Handler
	public class SomethingQueryHandler : IQueryHandler<GetSomething, Something>
	{
		public SomethingHandler( /* interesting dependencies here */)
		{
			// ...
		}

		public Something Handle(GetSomething query)
		{
			// ...
		}
	}

## Command Handler
	public class SomethingCommandHandler : ICommandHandler<DoSomething, SomethingResult>
	{
		public SomethingHandler(/* interesting dependencies here */)
		{
			// ...
		}

		public SomethingResult Handle(DoSomething command)
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