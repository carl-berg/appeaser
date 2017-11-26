using System;
using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Exceptions;

namespace Appeaser
{
    public class Mediator : IMediator
    {
        protected readonly IMediatorHandlerFactory HandlerFactory;
        protected readonly IMediatorSettings Settings;

        public Mediator(IMediatorHandlerFactory handlerFactory, IMediatorSettings settings = null)
        {
            HandlerFactory = handlerFactory;
            Settings = settings ?? new MediatorSettings();
        }

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
                if (ex is MediatorQueryException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorQueryException(ex, query.GetType());
            }
        }

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
                if (ex is MediatorQueryException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorQueryException(ex, query.GetType());
            }
        }

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
                if (ex is MediatorCommandException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorCommandException(ex, command.GetType());
            }
        }

        public virtual async Task<TResult> Send<TResult>(IAsyncCommand<TResult> command)
        {
            try
            {
                var handler = GetHandler<TResult>(typeof(IAsyncCommandHandler<,>), command);
                if (handler == null)
                {
                    throw new MediatorCommandException("No command handler of type {0} could be found", command.GetType());
                }

                return await InvokeHandlerAsync<TResult>(handler, command);
            }
            catch (Exception ex)
            {
                if (ex is MediatorCommandException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorCommandException(ex, command.GetType());
            }
        }

        protected virtual object GetHandler<TResponse>(Type handlerType, object parameter)
        {
            var requestType = typeof(TResponse);
            var parameterType = parameter.GetType();
            var requestingHandlerType = handlerType.MakeGenericType(parameterType, requestType);
            return HandlerFactory.GetHandler(requestingHandlerType);
        }

        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetRuntimeMethod("Handle", new[] { parameter.GetType() });
            try
            { 
                return (TReturn)method.Invoke(handler, new[] { parameter });
            }
            catch(TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        protected virtual async Task<TReturn> InvokeHandlerAsync<TReturn>(object handler, object parameter)
        {
            var method = handler.GetType().GetRuntimeMethod("Handle", new[] { parameter.GetType() });
            try
            { 
                return await(Task<TReturn>)method.Invoke(handler, new[] { parameter });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}