using System;
using System.Threading.Tasks;
using Appeaser.Interception;
using FluentValidation;
using FluentValidation.Results;
using Lamar;
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
                configure.For<IMediator>().Use(CreateMediator);
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
        public async Task TestValidationRequestInterceptionCanFailValidation()
        {
            var mediator = _container.GetInstance<IMediator>();
            var exception = await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(new TestFeature.Command(null)));
            Assert.False(exception.ValidationResult.IsValid);
        }

        [Fact]
        public async Task TestValidationRequestInterceptionCanPassValidation()
        {
            var mediator = _container.GetInstance<IMediator>();
            var response = await mediator.Send(new TestFeature.Command("name"));
            Assert.Equal(UnitType.Default, response);
        }

        private Mediator CreateMediator(IServiceContext context)
        {
            var handlerFactory = context.GetInstance<IMediatorHandlerFactory>();
            var settings = new MediatorSettings { WrapExceptions = false }
                .AddRequestInterceptor<ValidatingRequestInterceptor>();
            return new Mediator(handlerFactory, settings);
        }

        public class TestFeature
        {
            public class Command : IAsyncCommand<UnitType>
            {
                public Command(string name) => Name = name;
                public string Name { get; set; }
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).MustAsync(async (val, ct) =>
                    {
                        await Task.Delay(100);
                        return val != null;
                    });
                }
            }

            public class Handler : IAsyncCommandHandler<Command, UnitType>
            {
                public Task<UnitType> Handle(Command request) => Task.FromResult(UnitType.Default);
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

            public async Task InterceptAsync(IRequestInterceptionContext context)
            {
                if (_factory.GetValidator(context.RequestType) is IValidator validator)
                {
                    var result = await validator.ValidateAsync(context.Request);
                    if (!result.IsValid)
                    {
                        throw new ValidationException(result);
                    }
                }
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