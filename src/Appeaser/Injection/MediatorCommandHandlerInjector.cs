using System;

namespace Appeaser.Injection
{
    public class MediatorCommandHandlerInjector : IMediatorCommandHandlerInjector
    {
        private readonly IMediator _mediator;

        public MediatorCommandHandlerInjector(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Type InjectionType { get { return typeof(IMediator); } }
        public void Inject(IMediatorInjectionContext context)
        {
            context.Inject(InjectionType, _mediator);
        }
    }
}