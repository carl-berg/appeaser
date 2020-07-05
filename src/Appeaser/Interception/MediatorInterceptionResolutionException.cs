using System;

namespace Appeaser.Interception
{
    public class MediatorInterceptionResolutionException : Exception
    {
        public MediatorInterceptionResolutionException(Type type)
            : base($"Mediator Interceptor of type {type} could not be resolved. Make sure mediator handler factory or mediator resolver can resolve this type")
        {
        }
    }
}
