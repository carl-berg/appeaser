using FluentValidation;
using FluentValidation.Results;
using Lamar;
using System;
using Xunit;

namespace Appeaser.Tests.IntegrationTests
{
    public class FluentValidationTest : TestBase
    {
        private Container _container;

        public FluentValidationTest()
        {

            _container = new Container(configure =>
            {
                configure.For<IValidatorFactory>().Use<ValidatorFactory>();
                configure.For<IMediatorHandlerFactory>().Use<LamarMediatorHandlerFactory>();
                configure.For<IMediator>().Use(x => new Mediator(
                        x.GetInstance<IMediatorHandlerFactory>(), 
                        new MediatorSettings { WrapExceptions = false }
                            .AddRequestInterceptor<ValidatingRequestInterceptor>()));
                configure.Scan(s =>
                {
                    s.AssemblyContainingType<FluentValidationTest>();
                    s.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                    s.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
                });
            });
        }

        [Fact]
        public void TestValidationRequestInterceptionCanFailValidation()
        {
            var mediator = _container.GetInstance<IMediator>();
            var exception = Assert.Throws<ValidationException>(() => mediator.Send(new TestFeature.Command(null)));
            Assert.False(exception.ValidationResult.IsValid);
        }

        [Fact]
        public void TestValidationRequestInterceptionCanPassValidation()
        {
            var mediator = _container.GetInstance<IMediator>();
            var response = mediator.Send(new TestFeature.Command("name"));
            Assert.Equal(UnitType.Default, response);
        }

        public class TestFeature
        {
            public class Command : ICommand<UnitType>
            {
                public Command(string name) => Name = name;
                public string Name { get; set; }
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).NotNull();
                }
            }

            public class Handler : ICommandHandler<Command, UnitType>
            {
                public UnitType Handle(Command request) => UnitType.Default;
            }
        }

        public class ValidationException : Exception
        {
            public ValidationException(ValidationResult validationResult)
            {
                ValidationResult = validationResult;
            }

            public ValidationResult ValidationResult { get; }
        }

        public class ValidatingRequestInterceptor : IRequestInterceptor
        {
            private readonly IValidatorFactory _factory;

            public ValidatingRequestInterceptor(IValidatorFactory factory)
            {
                _factory = factory;
            }

            public void Intercept(IRequestInterceptionContext context)
            {
                if (_factory.GetValidator(context.RequestType) is IValidator validator)
                {
                    var result = validator.Validate(context.Request);
                    if (!result.IsValid)
                    {
                        throw new ValidationException(result);
                    }
                }
            }
        }

        public class ValidatorFactory : ValidatorFactoryBase
        {
            private readonly IServiceContext _context;

            public ValidatorFactory(IServiceContext context)
            {
                _context = context;
            }

            public override IValidator CreateInstance(Type validatorType) => _context.TryGetInstance(validatorType) as IValidator;
        }

        public class LamarMediatorHandlerFactory : IMediatorHandlerFactory
        {
            private readonly IServiceContext _serviceContext;

            public LamarMediatorHandlerFactory(IServiceContext serviceContext)
            {
                _serviceContext = serviceContext;
            }

            public object GetHandler(Type handlerType)
            {
                return _serviceContext.TryGetInstance(handlerType);
            }
        }
    }
}
