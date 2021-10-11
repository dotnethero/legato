using System.Threading.Tasks;

namespace Legato.Settings.Services
{
    public interface ISettingsProvider
    {
        Task<TSettings> Get<TSettings>() where TSettings : IGeneralSettings, new();
        Task Set<TSettings>(TSettings settings) where TSettings : IGeneralSettings;
    }
}
