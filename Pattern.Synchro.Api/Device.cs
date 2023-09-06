using System;

namespace Pattern.Synchro.Api
{
    public class Device
    {
        public Guid Id { get; set; }
        
        public DateTime LastSynchro { get; set; }
        public DateTime LastLocalSynchro { get; set; }
        
        public int Version { get; set; }
    }
}