using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Exceptions;

namespace Appeaser
{
    public class Mediator : IMediator, ISimpleMediator
    {
        protected readonly IMediatorHandlerFactory Resolver;
        protected readonly IMediatorSettings Settings;

        public Mediator(IMediatorHandlerFactory handlerFactory) : this(handlerFactory, new MediatorSettings()) { }

        public Mediator(IMediatorResolver resolver) : this(resolver, new MediatorSettings()) { }

        public Mediator(IMediatorHandlerFactory handlerFactory, IMediatorSettings settings)
        {
            Resolver = handlerFactory;
            Settings = settings ?? new MediatorSettings();
        }

        public Mediator(IMediatorResolver resolver, IMediatorSettings settings)
        {
            Resolver = resolver;
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

        public virtual async Task<TResponse> Request<TResponse>(IAsyncRequest<TResponse> request)
        {
            try
            {
                var handler = GetHandler<TResponse>(typeof(IAsyncRequestHandler<,>), request);
                if (handler == null)
                {
                    throw new MediatorRequestException("No request handler of type {0} could be found", request.GetType());
                }

                return await InvokeHandlerAsync<TResponse>(handler, request);
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
            return Resolver.GetHandler(requestingHandlerType);
        }

        protected virtual TReturn InvokeHandler<TReturn>(object handler, object parameter)
        {
            var parameterType = parameter.GetType();
            var handlerType = handler.GetType();
            var method = handlerType.GetRuntimeMethod("Handle", new[] { parameterType });
            var requestContext = new RequestInterceptionContext(parameterType, handlerType, handler, parameter);
            try
            {
                InvokeRequestInterceptors(requestContext);
                var response = (TReturn)method.Invoke(handler, new[] { parameter });
                InvokeResponseInterceptors(new ResponseInterceptionContext(requestContext, typeof(TReturn), response));
                return response;
            }
            catch (TargetInvocationException ex)
            {
                InvokeResponseInterceptors(new ResponseInterceptionContext(requestContext, typeof(TReturn), ex));
                throw ex.InnerException;
            }
        }

        protected virtual async Task<TReturn> InvokeHandlerAsync<TReturn>(object handler, object parameter)
        {
            var parameterType = parameter.GetType();
            var handlerType = handler.GetType();
            var method = handlerType.GetRuntimeMethod("Handle", new[] { parameterType });
            var requestContext = new RequestInterceptionContext(parameterType, handlerType, handler, parameter);
            try
            {
                InvokeRequestInterceptors(requestContext);
                var response = await(Task<TReturn>)method.Invoke(handler, new[] { parameter });
                InvokeResponseInterceptors(new ResponseInterceptionContext(requestContext, typeof(TReturn), response));
                return response;
            }
            catch (TargetInvocationException ex)
            {
                InvokeResponseInterceptors(new ResponseInterceptionContext(requestContext, typeof(TReturn), ex));
                throw ex.InnerException;
            }
        }

        private object GetInterceptor(Type interceptorType)
        {
            if (Resolver is IMediatorResolver resolver)
            {
                return resolver.GetInterceptor(interceptorType);
            }
            else
            {
                return Resolver.GetHandler(interceptorType);
            }
        }

        private void InvokeRequestInterceptors(RequestInterceptionContext context)
        {
            var interceptors = (Settings.RequestInterceptors ?? new Type[0]).Select(GetInterceptor).OfType<IRequestInterceptor>();
            foreach (var interceptor in interceptors)
            {
                interceptor.Intercept(context);
            }
        }

        private void InvokeResponseInterceptors(ResponseInterceptionContext context)
        {
            var interceptors = (Settings.ResponseInterceptors ?? new Type[0]).Select(GetInterceptor).OfType<IResponseInterceptor>();
            foreach (var interceptor in interceptors)
            {
                interceptor.Intercept(context);
            }
        }
    }
}