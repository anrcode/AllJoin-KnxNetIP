using System;
using System.Linq;
using BridgeRT;
using SparkAlljoyn;
using HoermannAdapter.Hoermann;
using HoermannAdapter.Hoermann.Discovery;


namespace HoermannAdapter
{
    internal class HoermannAdapter : BridgeAdapter
    {
        private HoermannDiscovery _discovery;

        public HoermannAdapter() : base("Hoermann")
        {
            
        }

        override public uint Initialize()
        {
            HoermannDiscovery.DeviceDiscovered += HoermannDiscovery_DeviceDiscovered;
            HoermannDiscovery.DeviceRemoved += HoermannDiscovery_DeviceRemoved;
            _discovery = new HoermannDiscovery();

            return ERROR_SUCCESS;
        }

        private void HoermannDiscovery_DeviceDiscovered(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var conn = e.Device as HoermannConnection;
            conn.Connect();

            var device = new HoermannDevice(this, conn, "Hoermann", "Hoermann", "TXXX2", "1.0.0.0", e.DeviceId, "Hoermann");
            devices.Add(device);
            this.NotifyDeviceArrival(device);

            // TEMP
            var msg = MCP.CreateLoginCmd("admin", "0000");
            conn.SendMessage(msg);
        }

        private void HoermannDiscovery_DeviceRemoved(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var matchingDevices = devices.Where(d => d.SerialNumber == e.DeviceId).ToList();
            foreach (var device in matchingDevices)
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

            ValuePtr = ((BridgeAdapterProperty<HoermannDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

            return ERROR_SUCCESS;
        }

        override public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        override public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterMethod<HoermannDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}
