using System;
using System.Linq;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using SparkAlljoyn.Discovery;
using OnkyoAdapter.Onkyo;
using OnkyoAdapter.Onkyo.Discovery;


namespace OnkyoAdapter
{
    internal class OnkyoAdapter : BridgeAdapter
    {
        private OnkyoDiscovery _discovery;

        public OnkyoAdapter() : base("Onkyo")
        {
            
        }

        override public uint Initialize()
        {
            OnkyoDiscovery.DeviceDiscovered += OnkyoDiscovery_DeviceDiscovered;
            OnkyoDiscovery.DeviceRemoved += OnkyoDiscovery_DeviceRemoved;
            _discovery = new OnkyoDiscovery();

            return ERROR_SUCCESS;
        }       

        private void OnkyoDiscovery_DeviceDiscovered(object sender, AdapterDiscoveryEventArgs e)
        {
            var conn = e.Device as OnkyoConnection;
            //conn.Connect();

            Task.Factory.StartNew(async () =>
            {
                var device = new OnkyoDevice(this, conn, "TXXXX", "Onkyo", "TXXX2", "1.0.0.0", e.DeviceId, "Onkyo Receiver");
                devices.Add(device);
                conn.ConnectionLost += (object s, EventArgs args) => {
                    this.NotifyDeviceRemoval(device);
                    devices.Remove(device);
                };

                await device.AquireCurrentState();
                this.NotifyDeviceArrival(device);               
            });
        }

        private void OnkyoDiscovery_DeviceRemoved(object sender, AdapterDiscoveryEventArgs e)
        {
            var matchingDevices = devices.Where(d => d.SerialNumber == e.DeviceId).ToList();
            foreach(var device in matchingDevices)
            {
                this.NotifyDeviceRemoval(device);
                devices.Remove(device);
            }
        }

        override public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        override public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            ValuePtr = ((BridgeAdapterProperty<OnkyoDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

            return ERROR_SUCCESS;
        }

        override public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterProperty<OnkyoDevice>)Property).Device.SendPropertyValue(Property, Value);

            return ERROR_SUCCESS;
        }

        override public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterMethod<OnkyoDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}
