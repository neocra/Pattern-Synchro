using System;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Sample.Api
{
    public class Car : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime LastUpdated { get; set; }
        
        public string UserId { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}