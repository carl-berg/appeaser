using System;
using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Exceptions;
using Appeaser.Interception;

namespace Appeaser
{
    public class Mediator : IMediator, ISimpleMediator
    {
        protected readonly IMediatorHandlerFactory Resolver;
        protected readonly IMediatorSettings Settings;
        private readonly MediatorInterceptor _interceptor;

        public Mediator(IMediatorHandlerFactory handlerFactory) : this(handlerFactory, new MediatorSettings()) { }

        public Mediator(IMediatorResolver resolver) : this(resolver, new MediatorSettings()) { }

        public Mediator(IMediatorHandlerFactory handlerFactory, IMediatorSettings settings)
        {
            Resolver = handlerFactory;
            Settings = settings ?? new MediatorSettings();
            _interceptor = new MediatorInterceptor(settings, handlerFactory);
        }

        public Mediator(IMediatorResolver resolver, IMediatorSettings settings)
        {
            Resolver = resolver;
            Settings = settings ?? new MediatorSettings();
            _interceptor = new MediatorInterceptor(settings, resolver);
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

        public virtual Task<TResponse> Request<TResponse>(IAsyncQuery<TResponse> query)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IAsyncQueryHandler<,>), query);
                if (handler == null)
                {
                    throw new MediatorQueryException("No query handler of type {0} could be found", query.GetType());
                }

                return InvokeHandlerAsync<TResponse>(handler, query);
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

        public virtual TResponse Request<TResponse>(IRequest<TResponse> request)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IRequestHandler<,>), request);
                if (handler == null)
                {
                    throw new MediatorRequestException("No request handler of type {0} could be found", request.GetType());
                }

                return InvokeHandler<TResponse>(handler, request);
            }
            catch (Exception ex)
            {
                if (ex is MediatorRequestException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorRequestException(ex, request.GetType());
            }
        }

        public virtual Task<TResponse> Request<TResponse>(IAsyncRequest<TResponse> request)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IAsyncRequestHandler<,>), request);
                if (handler == null)
                {
                    throw new MediatorRequestException("No request handler of type {0} could be found", request.GetType());
                }

                return InvokeHandlerAsync<TResponse>(handler, request);
            }
            catch (Exception ex)
            {
                if (ex is MediatorRequestException || !Settings.WrapExceptions)
                {
                    throw;
                }

                throw new MediatorRequestException(ex, request.GetType());
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

        public virtual Task<TResult> Send<TResult>(IAsyncCommand<TResult> command)
        {
            try
            {
                var handler = GetHandler<TResult>(typeof(IAsyncCommandHandler<,>), command);
                if (handler == null)
                {
                    throw new MediatorCommandException("No command handler of type {0} could be found", command.GetType());
                }

                return InvokeHandlerAsync<TResult>(handler, command);
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
            return Resolver.GetHandler(requestingHandlerType);
        }

        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            return _interceptor.InvokeHandler(
                handler, 
                parameter, 
                scope =>
                {
                    var method = scope.HandlerType.GetRuntimeMethod("Handle", new[] { scope.RequestType });
                    return (TReturn)method.Invoke(handler, new[] { parameter });
                });
        }

        protected virtual Task<TReturn> InvokeHandlerAsync<TReturn>(object handler, object parameter)
        {          
            return _interceptor.InvokeHandlerAsync(
                handler, 
                parameter, 
                scope =>
                {
                    var method = scope.HandlerType.GetRuntimeMethod("Handle", new[] { scope.RequestType });
                    return (Task<TReturn>)method.Invoke(handler, new[] { parameter });
                });
        }
    }
}