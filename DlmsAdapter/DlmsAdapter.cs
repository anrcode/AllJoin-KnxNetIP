using System;
using BridgeRT;
using SparkAlljoyn;
using System.Threading.Tasks;

namespace DlmsAdapter
{
    internal class DlmsAdapter : BridgeAdapter
    {
        public DlmsAdapter() : base("Dlms")
        {
            
        }

        override public uint Initialize()
        {
            Task.Factory.StartNew(async () =>
            {
                var conn = new Dlms.DlmsSerial();
                var status = await conn.Connect(null);
                var device = new DlmsDevice(this, conn, "Test", "Test", "Test", "Test", "test", "test");
                devices.Add(device);
                await device.AquireCurrentState();
                this.NotifyDeviceArrival(device);
            });

            return ERROR_SUCCESS;
        }      

        override public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            ValuePtr = ((BridgeAdapterProperty<DlmsDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

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

            ((BridgeAdapterMethod<DlmsDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}
