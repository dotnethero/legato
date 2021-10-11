using Legato.Data.Services;
using Legato.Settings.Contracts;
using Legato.Settings.Models;

namespace Legato.Settings.Extensions
{
    public static class EntitySettingsExtensions
    {
        public static TSettings GetSettings<TSettings>(this IHasSettings<TSettings> entity) 
            where TSettings : IEntitySettings, new() =>
            entity.Settings is not null &&
            entity.Settings.Data is not null
                ? entity.Settings.Data
                : new TSettings();

        public static void SetSettings<TSettings>(this ISimpleStateContext context, IHasSettings<TSettings> entity, TSettings settings)
            where TSettings : IEntitySettings
        {
            if (entity.Settings is null)
            {
                entity.Settings = new EntitySettings<TSettings> { Data = settings };
                context.Add(entity.Settings);
            }
            else
            {
                entity.Settings.Data = settings;
                context.Modified(entity.Settings);
            }
        }
    }
}
