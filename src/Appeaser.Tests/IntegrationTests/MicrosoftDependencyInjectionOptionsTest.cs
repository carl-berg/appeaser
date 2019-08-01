using System.Reflection;
using System.Threading.Tasks;
using Appeaser.Interception;
using Appeaser.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Appeaser.Tests.IntegrationTests
{
    public class MicrosoftDependencyInjectionOptionsTest
    {
        [Fact]
        public void CanResolveMediatorSettings()
        {
            var services = new ServiceCollection()
                .AddAppeaser()
                .BuildServiceProvider();

            var options = services.GetService<IOptions<MediatorSettings>>();

            options.ShouldNotBeNull();
            options.Value.ShouldNotBeNull();
        }

        [Fact]
        public void CanCanfigureMediatorSettingsBeforeAppeaserIsRegistered()
        {
            var services = new ServiceCollection()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                .AddAppeaser()
                .BuildServiceProvider();

            var options = services.GetService<IOptions<MediatorSettings>>();

            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
        }

        [Fact]
        public void CanCanfigureMediatorSettingsAfterAppeaserIsRegistered()
        {
            var services = new ServiceCollection()
                .AddAppeaser()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                .BuildServiceProvider();

            var options = services.GetService<IOptions<MediatorSettings>>();

            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
        }


        [Fact]
        public void CanCanfigureMediatorSettingsBeforeAndAfterAppeaserIsRegistered()
        {
            var services = new ServiceCollection()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                .AddAppeaser()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorB>())
                .BuildServiceProvider();

            var options = services.GetService<IOptions<MediatorSettings>>();

            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorB));
        }

        [Fact]
        public void CanCanfigureMediatorSettingsBeforeInAndAfterAppeaserIsRegistered()
        {
            var services = new ServiceCollection()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                .AddAppeaser(c => c.AddRequestInterceptor<InterceptorB>())
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorC>())
                .BuildServiceProvider();

            var options = services.GetService<IOptions<MediatorSettings>>();

            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorA));
            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorB));
            options.Value.RequestInterceptors.ShouldContain(typeof(InterceptorC));
        }

        [Fact]
        public void AllConfiguredMediatorSettingsAreResolvedInMediator()
        {
            var services = new ServiceCollection()
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorA>())
                .AddAppeaser(c => c.AddRequestInterceptor<InterceptorB>())
                .Configure<MediatorSettings>(o => o.AddRequestInterceptor<InterceptorC>())
                .BuildServiceProvider();

            var mediator = (Mediator)services.GetService<IMediator>();
            var settings = typeof(Mediator)
                .GetField("Settings", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(mediator) as IMediatorSettings;

            settings.ShouldNotBeNull("Oops, settings were not properly resolved");
            settings.RequestInterceptors.ShouldContain(typeof(InterceptorA));
            settings.RequestInterceptors.ShouldContain(typeof(InterceptorB));
            settings.RequestInterceptors.ShouldContain(typeof(InterceptorC));
        }

        public class InterceptorA : IRequestInterceptor
        {
            public void Intercept(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }

            public Task InterceptAsync(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }
        }

        public class InterceptorB : IRequestInterceptor
        {
            public void Intercept(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }

            public Task InterceptAsync(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }
        }

        public class InterceptorC : IRequestInterceptor
        {
            public void Intercept(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }

            public Task InterceptAsync(IRequestInterceptionContext context)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
