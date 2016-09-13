using System;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using IrTransAdapter.IrTrans;


namespace IrTransAdapter
{
    class IrTransDevice : BridgeAdapterDevice<IrTransAdapter>
    {
        private IrTransConnection _conn;

        internal IrTransDevice(IrTransAdapter adapter, IrTransConnection conn, string Name, string VendorName, string Model, string Version, string SerialNumber, string Description) 
            : base(adapter, Name, VendorName, Model, Version, SerialNumber, Description)
        {
            _conn = conn;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();
        }

        override protected void CreateMethods()
        {
            this.Methods.Clear();

            var inputMethod = new BridgeAdapterMethod<IrTransDevice>(this, "LearnCmd", "test", 0);
            inputMethod.InputParams.Add(new BridgeAdapterValue("name", "") { Data = "" });
            this.Methods.Add(inputMethod);

            var sendMethod = new BridgeAdapterMethod<IrTransDevice>(this, "SendCmd", "test", 0);
            sendMethod.InputParams.Add(new BridgeAdapterValue("name", "") { Data = "" });
            this.Methods.Add(sendMethod);
        }

        virtual internal async Task<bool> AquireCurrentState()
        {
            return true;
        }

        virtual public void CallMethod(IAdapterMethod method)
        {
            if (method.Name == "LearnCmd")
            {
                Task.Factory.StartNew(async () =>
                {
                    var commandhex = await _conn.LearnCommand();
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["commandhex:" + Convert.ToString(method.InputParams[0].Data)] = commandhex;
                });
            }
            else if (method.Name == "SendCmd")
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                var commandhex = (string)localSettings.Values["commandhex:" + Convert.ToString(method.InputParams[0].Data)];
                _conn.SendCommandHex(commandhex);
            }
        }

        virtual public void SendPropertyValue(IAdapterProperty Property, IAdapterValue Value)
        {

        }
    }
}
