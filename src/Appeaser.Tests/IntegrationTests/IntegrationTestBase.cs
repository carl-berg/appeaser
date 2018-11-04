using System;
using System.Collections.Generic;
using System.Text;

namespace Appeaser.Tests.IntegrationTests
{
    public abstract class IntegrationTestBase : TestBase
    {
        public class DisposableFeature
        {
            public class Request : IRequest<TestDisposable> { }

            public class Handler : IRequestHandler<Request, TestDisposable>
            {
                private readonly TestDisposable _disposable;
                public Handler(TestDisposable disposable) => _disposable = disposable;
                public TestDisposable Handle(Request command) => _disposable;
            }
        }
    }
}
