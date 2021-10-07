using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Legato.Common.Logging;
using Legato.Common.Services;

namespace Legato.Common
{
    public class CommonModule : Module
    {
        LoggingMiddleware middleware;

        public CommonModule()
        {
            middleware = new LoggingMiddleware();
        }

        protected override void AttachToComponentRegistration(
            IComponentRegistryBuilder registry,
            IComponentRegistration registration) =>
            registration.PipelineBuilding += (sender, pipeline) => pipeline.Use(middleware);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DateTimeProvider>().AsImplementedInterfaces();
        }
    }
}
