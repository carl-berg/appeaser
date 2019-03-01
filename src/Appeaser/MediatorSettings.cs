using System;
using System.Collections.Generic;

namespace Appeaser
{
    public class MediatorSettings : IMediatorSettings
    {
        private List<Type> _requestInterceptors;
        private List<Type> _responseInterceptors;

        public MediatorSettings()
        {
            WrapExceptions = true;
            _requestInterceptors = new List<Type>();
            _responseInterceptors = new List<Type>();
        }

        public bool WrapExceptions { get; set; }

        public IEnumerable<Type> RequestInterceptors => _requestInterceptors;

        public IEnumerable<Type> ResponseInterceptors => _responseInterceptors;

        public MediatorSettings AddRequestInterceptor<TInterceptor>() where TInterceptor : IRequestInterceptor
        {
            _requestInterceptors.Add(typeof(TInterceptor));
            return this;
        }

        public MediatorSettings AddResponseInterceptor<TInterceptor>() where TInterceptor : IResponseInterceptor
        {
            _responseInterceptors.Add(typeof(TInterceptor));
            return this;
        }

        public MediatorSettings AddInterceptor<TInterceptor>() where TInterceptor : IRequestInterceptor, IResponseInterceptor
        {
            _requestInterceptors.Add(typeof(TInterceptor));
            _responseInterceptors.Add(typeof(TInterceptor));
            return this;
        }
    }
}
