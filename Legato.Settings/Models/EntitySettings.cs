using Legato.Data.Models;

namespace Legato.Settings.Models
{
    public class EntitySettings<TSettings> : IEntity where TSettings : IEntitySettings
    {
        public TSettings Data { get; set; }
    }
}
