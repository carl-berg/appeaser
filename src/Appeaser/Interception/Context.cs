using System.Collections.Generic;

namespace Appeaser.Interception
{
    public interface IContext
    {
        object Get(string key);
        T Get<T>(string key);
        void Set<T>(string key, T value);
    }

    internal class Context : IContext
    {
        private Dictionary<string, object> _items;

        public Context()
        {
            _items = new Dictionary<string, object>();
        }

        public object Get(string key) => _items.TryGetValue(key, out object value) ? value : null;
        public T Get<T>(string key) => _items.TryGetValue(key, out object value) && value is T typedValue ? typedValue : default(T);
        public void Set<T>(string key, T value) => _items[key] = value;
    }
}
