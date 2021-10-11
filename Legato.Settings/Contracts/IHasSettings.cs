using Legato.Settings.Models;

namespace Legato.Settings.Contracts
{
    public interface IHasSettings<TSettings> where TSettings : IEntitySettings
    {
        public EntitySettings<TSettings> Settings { get; set; }
    }
}
