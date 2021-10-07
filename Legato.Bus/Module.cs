using Autofac;
using Legato.Bus.Azure.Dispatchers;

namespace Legato.Bus.Azure
{
    public class BusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BusEventDispatcher>().AsImplementedInterfaces();
            builder.RegisterType<BusCommandDispatcher>().AsImplementedInterfaces();
            builder.RegisterType<BusMessageProcessorHub>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<BusMessageProvider>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<BusManager>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
