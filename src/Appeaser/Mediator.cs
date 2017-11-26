using System;
using System.Diagnostics;
using System.Reflection;
using Appeaser.Exceptions;

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
        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetRuntimeMethod("Handle", new[] { parameter.GetType() });
            return (TReturn)method.Invoke(handler, new[] { parameter });
        }
    }
}