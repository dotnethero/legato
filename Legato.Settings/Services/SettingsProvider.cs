using Legato.Common.Extensions;
using Legato.Data.Services;
using Legato.Settings.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Legato.Settings.Services
{
    class SettingsProvider : ISettingsProvider
    {
        IQueryProvider<GeneralSettings> provider;
        IStateContext context;
        IMemoryCache cache;

        public SettingsProvider(IQueryProvider<GeneralSettings> provider, IStateContext context, IMemoryCache cache)
        {
            this.provider = provider;
            this.context = context;
            this.cache = cache;
        }

        public async Task<TSettings> Get<TSettings>() where TSettings : IGeneralSettings, new()
        {
            var model = await GetSettingsModel<TSettings>();
            return model != null
                ? JsonExtensions.Deserialize<TSettings>(model.Data)
                : new TSettings();
        }

        public async Task Set<TSettings>(TSettings settings) where TSettings : IGeneralSettings
        {
            var json = JsonExtensions.Serialize(settings);
            var model = await GetSettingsModel<TSettings>();
            if (model == null)
            {
                model = new GeneralSettings
                {
                    Type = typeof(TSettings).FullName,
                    Data = json
                };
                context.Add(model);
            }
            else
            {
                context.Attach(model);
                model.Data = json;
            }

            await context.SaveChanges();
            cache.Set(model.Type, model);
        }

        Task<GeneralSettings> GetSettings(string type) => 
            cache.GetOrCreateAsync(type, _ => provider.Query().SingleOrDefaultAsync(x => x.Type == type));

        async Task<GeneralSettings> GetSettingsModel<TSettings>() where TSettings : IGeneralSettings
        {
            var type = typeof(TSettings).FullName;
            return await GetSettings(type);
        }
    }
}
