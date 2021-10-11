using Autofac;
using Legato.Bus.Azure;
using Legato.Common;
using Legato.CQRS;
using Legato.Data;
using Legato.Settings;
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
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<BusModule>();
            builder.RegisterModule<TransactionsModule>(); // decorators for dispatchers
        }
    }
}
