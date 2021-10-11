using Autofac;
using Legato.Bus.Azure;
using Legato.Common;
using Legato.CQRS;
using Legato.Data;
using Legato.Transactions;

namespace Legato
{
    public class CompositionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<CQRSModule>();
            builder.RegisterModule<DataModule>();
            builder.RegisterModule<BusModule>();
            builder.RegisterModule<TransactionModule>(); // decorators for dispatchers
        }
    }
}
