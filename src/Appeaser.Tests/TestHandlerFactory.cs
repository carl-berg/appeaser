using System;
using System.Collections.Generic;
using System.Linq;

namespace Appeaser.Tests
{
    public class TestHandlerFactory : IMediatorHandlerFactory
    {
        private IList<Type> _handlers = new List<Type>();

        public TestHandlerFactory() { }

        public object GetHandler(Type handlerType)
        {
            var handler = _handlers.Single(x => x == handlerType  || x.GetInterfaces().Contains(handlerType));
            return Activator.CreateInstance(handler);
        }

        public TestHandlerFactory AddHandler<T>()
        {
            _handlers.Add(typeof(T));
            return this;
        }
    }
}
