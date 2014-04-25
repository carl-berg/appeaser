using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Exceptions;

namespace Appeaser
{
    public class Mediator : IMediator
    {
        protected readonly IMediatorHandlerFactory HandlerFactory;

        public Mediator(IMediatorHandlerFactory handlerFactory)
        {
            HandlerFactory = handlerFactory;
        }

        [DebuggerStepThrough]
        public virtual TResponse Request<TResponse>(IQuery<TResponse> query)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IQueryHandler<,>), query);
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
        public virtual async Task<TResponse> Request<TResponse>(IAsyncQuery<TResponse> query)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IAsyncQueryHandler<,>), query);
                if (handler == null)
                {
                    throw new MediatorQueryException("No query handler of type {0} could be found", query.GetType());
                }

                return await InvokeHandlerAsync<TResponse>(handler, query);
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
                var handler = GetHandler<TResult>(typeof(ICommandHandler<,>), command);
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
        public virtual async Task<TResult> Send<TResult>(IAsyncCommand<TResult> command)
        {
            try
            {
                var handler = GetHandler<TResult>(typeof(ICommandHandler<,>), command);
                if (handler == null)
                {
                    throw new MediatorCommandException("No command handler of type {0} could be found", command.GetType());
                }

                return await InvokeHandlerAsync<TResult>(handler, command);
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
        protected virtual object GetHandler<TResponse>(Type handlerType, object parameter)
        {
            var requestType = typeof(TResponse);
            var parameterType = parameter.GetType();
            var requestingHandlerType = handlerType.MakeGenericType(parameterType, requestType);
            return HandlerFactory.GetHandler(requestingHandlerType);
        }

        [DebuggerStepThrough]
        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, new[] { parameter.GetType() }, null);
            return (TReturn)method.Invoke(handler, new[] { parameter });
        }

        [DebuggerStepThrough]
        protected virtual async Task<TReturn> InvokeHandlerAsync<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, new[] { parameter.GetType() }, null);
            return await(Task<TReturn>)method.Invoke(handler, new[] { parameter });
        }
    }
}