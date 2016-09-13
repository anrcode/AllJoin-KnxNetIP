using BridgeRT;
using SparkAlljoyn;
using IrTransAdapter.IrTrans;
using IrTransAdapter.IrTrans.Discovery;


namespace IrTransAdapter
{
    internal class IrTransAdapter : BridgeAdapter
    {
        private IrTransDiscovery _discovery;

        public IrTransAdapter() : base("IrTrans")
        {
                      
        }

        override public uint Initialize()
        {
            IrTransDiscovery.DeviceDiscovered += IrTransDiscovery_DeviceDiscovered;
            _discovery = new IrTransDiscovery();

            return ERROR_SUCCESS;
        }

        private void IrTransDiscovery_DeviceDiscovered(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var conn = e.Device as IrTransConnection;
            conn.Connect();

            var device = new IrTransDevice(this, conn, "TXXXX", "IrTrans", "IrTrans", "1.0.0.0", e.DeviceId, "IrTrans");
            devices.Add(device);
            this.NotifyDeviceArrival(device);
        }

        override public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        override public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            ValuePtr = ((BridgeAdapterProperty<IrTransDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

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

            ((BridgeAdapterMethod<IrTransDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}
