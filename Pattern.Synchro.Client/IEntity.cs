using System;

namespace Pattern.Synchro.Client
{
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTime LastUpdated { get; set; }

        bool IsDeleted { get; set; }
    }
}