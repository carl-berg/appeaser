using System;
using System.Reflection;

namespace Appeaser.Exceptions
{
    public abstract class MediatorException : Exception
    {
        protected MediatorException(string messageFormat, params object[] parameters) : base(string.Format(messageFormat, parameters)) { }
        protected MediatorException(string message, Exception ex) : base(message, UnwrapException(ex)) { }
        protected MediatorException(Exception ex, string messageFormat, params object[] parameters) : base(string.Format(messageFormat, parameters), UnwrapException(ex)) { }

        public static Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException && exception.InnerException != null)
            {
                return UnwrapException(exception.InnerException);
            }

            return exception;
        }
    }
}