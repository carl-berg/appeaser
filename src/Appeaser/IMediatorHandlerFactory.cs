using System;
using System.Collections.Generic;
using Appeaser.Injection;

namespace Appeaser
{
    public interface IMediatorHandlerFactory
    {
        object GetHandler(Type handlerType);
        IEnumerable<TMediatorInjector> GetInjectors<TMediatorInjector>() where TMediatorInjector : IMediatorInjector;
    }
}