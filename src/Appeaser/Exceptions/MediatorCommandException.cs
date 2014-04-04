using System;

namespace Appeaser.Exceptions
{
    public class MediatorCommandException : MediatorException
    {
        public MediatorCommandException(string message, params object[] parameters) : base(message, parameters) { }
        public MediatorCommandException(Exception ex, Type type) : base(ex, "An exception occured while handling a command of type {0}: {1}", type, UnwrapException(ex).Message) { }
    }
}