using Autofac;
using Legato.CQRS.Services;

namespace Legato.CQRS
{
    public class CQRSModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventDispatcher>().AsImplementedInterfaces();
            builder.RegisterType<CommandDispatcher>().AsImplementedInterfaces();
        }
    }
}
