using System;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using DlmsAdapter.Dlms;


namespace DlmsAdapter
{
    class DlmsDevice : BridgeAdapterDevice<DlmsAdapter>
    {
        private DlmsSerial _dlms;

        internal DlmsDevice(DlmsAdapter adapter, DlmsSerial conn, string Name, string VendorName, string Model, string Version, string SerialNumber, string Description) 
            : base(adapter, Name, VendorName, Model, Version, SerialNumber, Description)
        {
            _dlms = conn;
            _dlms.DataReceived += Dlms_DataReceived;

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(30000);

                while (true)
                {
                    try
                    {
                        _dlms.ReadData();
                    }
                    finally
                    {
                        await Task.Delay(30000);
                    }
                }
            });
        }  

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<DlmsDevice>(this, "PowerMeter", "com.allseen.SmartHome.PowerMeter");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("EnergyTotal", 0.0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("EnergyHi", 0.0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("EnergyLo", 0.0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        virtual internal async Task<bool> AquireCurrentState()
        {
            _dlms.ReadData();
            await Task.Delay(3000);
            _enableSignals = true;

            return true;
        }

        virtual public void CallMethod(IAdapterMethod methodn)
        {

        }

        virtual public void SendPropertyValue(IAdapterProperty Property, IAdapterValue Value)
        {

        }

        private void Dlms_DataReceived(object sender, Dlms.DlmsEventArgs e)
        {
            var statusProp = this.Properties[0];

            double currentReading = Double.Parse(e.GetValue("1.8.0"));
            if (!currentReading.Equals(statusProp.Attributes[0].Value.Data))
            {
                statusProp.Attributes[0].Value.Data = currentReading;
                this.NotifyChangeOfValueSignal(statusProp, statusProp.Attributes[0]);
            }

            currentReading = Double.Parse(e.GetValue("1.8.1"));
            if (!currentReading.Equals(statusProp.Attributes[1].Value.Data))
            {
                statusProp.Attributes[1].Value.Data = currentReading;
                this.NotifyChangeOfValueSignal(statusProp, statusProp.Attributes[1]);
            }

            currentReading = Double.Parse(e.GetValue("1.8.2"));
            if (!currentReading.Equals(statusProp.Attributes[2].Value.Data))
            {
                statusProp.Attributes[2].Value.Data = currentReading;
                this.NotifyChangeOfValueSignal(statusProp, statusProp.Attributes[2]);
            }
        }
    }
}
