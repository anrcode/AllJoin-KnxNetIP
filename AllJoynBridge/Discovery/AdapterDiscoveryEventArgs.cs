using System;


namespace SparkAlljoyn.Discovery
{
    public class AdapterDiscoveryEventArgs : EventArgs
    {
        public string DeviceId { get; set; }

        public object Device { get; set; }

        public DateTime LastSeen { get; set;  }
    }
}
