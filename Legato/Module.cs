using Autofac;
using Legato.Bus.Azure;
using Legato.Common;
using Legato.CQRS;

namespace Legato
{
    public class CompositionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<CQRSModule>();
            builder.RegisterModule<BusModule>();
        }
    }
}
