using System;

namespace Appeaser.Injection
{
    public interface IMediatorInjectionContext
    {
        Type HandlerType { get; }
        Type CommandType { get; }
        Type QueryType { get; }
        void Inject(Type injectionType, object item);
    }
}