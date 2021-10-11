using System;

namespace Legato.Data.Models
{
    public interface IHasUpdatedAt
    {
        public DateTime UpdatedAt { get; set; }
    }
}