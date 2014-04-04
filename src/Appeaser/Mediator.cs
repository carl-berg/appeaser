using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Appeaser.Exceptions;
using Appeaser.Injection;

namespace Appeaser
{
    public class Mediator : IMediator
    {
        protected readonly IMediatorHandlerFactory HandlerFactory;
        protected readonly Type OpenQueryHandlerType = typeof(IQueryHandler<,>);
        protected readonly Type OpenCommandHandlerType = typeof(ICommandHandler<,>);

        public Mediator(IMediatorHandlerFactory handlerFactory)
        {
            HandlerFactory = handlerFactory;
        }

        [DebuggerStepThrough]
        public virtual TResponse Request<TResponse>(IQuery<TResponse> query)
        {
            try
            {
                var requestType = typeof(TResponse);
                var queryType = query.GetType();
                var requestingHandlerType = OpenQueryHandlerType.MakeGenericType(queryType, requestType);
                var handler = HandlerFactory.GetHandler(requestingHandlerType);
                if (handler == null)
                {
                    throw new MediatorQueryException("No query handler of type {0} could be found", query.GetType());
                }

                var handlerContext = new MediatorInjectionContext(handler) { QueryType = queryType, HandlerType = handler.GetType() };
                Inject<IMediatorCommandHandlerInjector>(handlerContext);

                var queryContext = new MediatorInjectionContext(query) { QueryType = queryType, HandlerType = handler.GetType() };
                Inject<IMediatorQueryInjection>(queryContext);
                
                return InvokeHandler<TResponse>(handler, query);
            }
            catch (Exception ex)
            {
                if (ex is MediatorQueryException)
                {
                    throw;
                }

                throw new MediatorQueryException(ex, query.GetType());
            }
        }

        [DebuggerStepThrough]
        public virtual TResult Send<TResult>(ICommand<TResult> command)
        {
            try
            {
                var returnType = typeof(TResult);
                var commandType = command.GetType();
                var requestingHandlerType = OpenCommandHandlerType.MakeGenericType(commandType, returnType);
                var handler = HandlerFactory.GetHandler(requestingHandlerType);
                if (handler == null)
                {
                    throw new MediatorCommandException("No command handler of type {0} could be found", command.GetType());
                }

                var handlerContext = new MediatorInjectionContext(handler) { CommandType = commandType, HandlerType = handler.GetType() };
                Inject<IMediatorCommandHandlerInjector>(handlerContext);

                var queryContext = new MediatorInjectionContext(command) { CommandType = commandType, HandlerType = handler.GetType() };
                Inject<IMediatorCommandInjector>(queryContext);

                return InvokeHandler<TResult>(handler, command);
            }
            catch (Exception ex)
            {
                if (ex is MediatorCommandException)
                {
                    throw;
                }

                throw new MediatorCommandException(ex, command.GetType());
            }
        }

        [DebuggerStepThrough]
        protected void Inject<TInjectionType>(IMediatorInjectionContext context) where TInjectionType : IMediatorInjector
        {
            foreach (var injector in HandlerFactory.GetInjectors<TInjectionType>())
            {
                injector.Inject(context);
            }
        }

        [DebuggerStepThrough]
        protected virtual void InjectMediator(object handler)
        {
            var mediatorProperties = handler.GetType()
                                            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                            .Where(x => x.PropertyType == typeof(IMediator));
            foreach (var mediatorProperty in mediatorProperties)
            {
                mediatorProperty.SetValue(handler, this);
            }
        }

        [DebuggerStepThrough]
        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, new[] { parameter.GetType() }, null);
            return (TReturn)method.Invoke(handler, new[] { parameter });
        }
    }
}