using System;

namespace Legato.Data.Models
{
    public interface IHasCreatedAt
    {
        public DateTime CreatedAt { get; set; }
    }
}