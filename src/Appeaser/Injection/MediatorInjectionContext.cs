using System;
using System.Linq;
using System.Reflection;

namespace Appeaser.Injection
{
    public class MediatorInjectionContext : IMediatorInjectionContext
    {
        private readonly object _injectionEntity;

        public MediatorInjectionContext(object injectionEntity)
        {
            _injectionEntity = injectionEntity;
        }

        public Type HandlerType { get; set; }
        public Type CommandType { get; set; }
        public Type QueryType { get; set; }

        public virtual void Inject(Type injectionType, object item)
        {
            var properties = _injectionEntity.GetType()
                                             .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                             .Where(x => x.PropertyType == injectionType)
                                             .Where(x => x.GetValue(_injectionEntity) == null);
            foreach (var property in properties)
            {
                property.SetValue(_injectionEntity, item);
            }
        }
    }
}