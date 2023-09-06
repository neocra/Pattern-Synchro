using System;
using System.Text.Json.Serialization;

namespace Pattern.Synchro.Client
{
    // [JsonConverter(typeof(SynchroConverter))]
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTime LastUpdated { get; set; }

        bool IsDeleted { get; set; }
    }
}