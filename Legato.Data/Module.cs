using Autofac;
using Legato.Data.Services;

namespace Legato.Data
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(QueryProvider<>)).AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(QueryExecutor<>)).AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<StateContext>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
