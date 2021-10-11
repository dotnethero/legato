using Legato.Data.Models;

namespace Legato.Settings.Models
{
    public class GeneralSettings : IEntity
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
