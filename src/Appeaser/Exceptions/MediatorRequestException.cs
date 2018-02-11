using System;

namespace Appeaser.Exceptions
{
    public class MediatorRequestException : MediatorException
    {
        public MediatorRequestException(string message, params object[] parameters) : base(message, parameters) { }
        public MediatorRequestException(Exception ex, Type type) : base(ex, $"An exception occured while handling a request of type {type}: {UnwrapException(ex).Message}") { }
    }
}