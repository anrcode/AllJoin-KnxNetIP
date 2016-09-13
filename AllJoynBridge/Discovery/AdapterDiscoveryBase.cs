using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SparkAlljoyn.Discovery
{
    public abstract class AdapterDiscoveryBase
    {
        protected static Dictionary<string, AdapterDiscoveryEventArgs> _deviceMap = new Dictionary<string, AdapterDiscoveryEventArgs>();

        public static event EventHandler<AdapterDiscoveryEventArgs> DeviceDiscovered;
        public static event EventHandler<AdapterDiscoveryEventArgs> DeviceRemoved;

        public AdapterDiscoveryBase()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        this.Discover();
                    }
                    catch
                    {
                        // nothing to do
                    }
                    finally
                    {
                        await Task.Delay(20000);
                    }                 
                }
            });

            Task.Run(async () => {
                while (true)
                {
                    this.RemoveInactiveDevices();
                    await Task.Delay(1000);
                }
            });
        }

        protected bool AlreadyDiscovered(string deviceId)
        {
            AdapterDiscoveryEventArgs device;
            if(!_deviceMap.TryGetValue(deviceId, out device))
            {
                return false;
            }

            device.LastSeen = DateTime.Now;
            return true;
        }

        protected void AddDevice(string deviceId, object device)
        {
            var evtArgs = new AdapterDiscoveryEventArgs()
            {
                 DeviceId = deviceId,
                 Device = device,
                 LastSeen = DateTime.Now
            };
            _deviceMap.Add(deviceId, evtArgs);

            DeviceDiscovered?.Invoke(this, evtArgs);
        }

        public object GetDevice(string deviceId)
        {
            AdapterDiscoveryEventArgs device;
            if (!_deviceMap.TryGetValue(deviceId, out device))
            {
                return null;
            }

            return device.Device;
        }

        protected abstract void Discover();

        protected void RemoveDevice(string deviceId)
        {
            var device = _deviceMap[deviceId];
            DeviceRemoved?.Invoke(this, new AdapterDiscoveryEventArgs()
            {
                DeviceId = deviceId,
                Device = device
            });
            _deviceMap.Remove(deviceId);
        }

        protected void RemoveInactiveDevices()
        {
            foreach (string deviceId in _deviceMap.Keys.ToList())
            {
                var device = _deviceMap[deviceId];
                if (device.LastSeen < DateTime.Now.AddMinutes(-1))
                {
                    this.RemoveDevice(deviceId);
                }
            }
        }
    }
}
