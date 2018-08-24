using System.Threading.Tasks;

namespace Appeaser.Tests
{
    public abstract class TestBase
    {
        public class QueryFeature
        {
            public class Query : IQuery<UnitType> { }
            public class AsyncQuery : IAsyncQuery<UnitType> { }

            public class Handler : IQueryHandler<Query, UnitType>, IAsyncQueryHandler<AsyncQuery, UnitType>
            {
                public UnitType Handle(Query request) => UnitType.Default;

                public async Task<UnitType> Handle(AsyncQuery request) => await Task.FromResult(UnitType.Default);
            }
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

        public class RequestFeature
        {
            public class Request : IRequest<UnitType> { }
            public class AsyncRequest : IAsyncRequest<UnitType> { }

            public class Handler : IRequestHandler<Request, UnitType>, IAsyncRequestHandler<AsyncRequest, UnitType>
            {
                public UnitType Handle(Request command) => UnitType.Default;

                public async Task<UnitType> Handle(AsyncRequest command) => await Task.FromResult(UnitType.Default);
            }
        }
    }
}
