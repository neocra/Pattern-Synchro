using System;

namespace Pattern.Synchro.Client
{
    public class SynchroDevice
    {
        public DateTime BeginServerDateTime { get; set; }
        public DateTime LastLocalSyncDateTime { get; set; }
        public int Version { get; set; }
    }
}