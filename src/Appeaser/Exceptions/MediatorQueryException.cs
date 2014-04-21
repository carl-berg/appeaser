using System;

namespace Appeaser.Exceptions
{
    public class MediatorQueryException : MediatorException
    {
        public MediatorQueryException(string message, params object[] parameters) : base(message, parameters) { }
        public MediatorQueryException(Exception ex, Type type) : base(ex, "An exception occured while handling a query of type {0}: {1}", type, UnwrapException(ex).Message) { }
    }
}