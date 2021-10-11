using Autofac;
using Legato.Settings.Services;

namespace Legato.Settings
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsProvider>().AsImplementedInterfaces();
        }
    }
}
