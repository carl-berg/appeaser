using System;
using System.Collections.Generic;
using System.Linq;

namespace Appeaser.Tests
{
    public class TestMediatorSettings : IMediatorSettings
    {
        public TestMediatorSettings()
        {
            WrapExceptions = new MediatorSettings().WrapExceptions;
        }

        public bool WrapExceptions { get; set; }

        public IEnumerable<Type> RequestInterceptors => Enumerable.Empty<Type>();

        public IEnumerable<Type> ResponseInterceptors => Enumerable.Empty<Type>();
    }
}
