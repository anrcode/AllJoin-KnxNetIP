using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;


namespace KnxNetIPAdapter
{
    internal class KnxDevice : BridgeAdapterDevice<KnxAdapter>
    {
        internal KnxDevice(KnxAdapter adapter, string Name, string VendorName, string Model, string Version, string SerialNumber, string Description) 
            : base(adapter, Name, VendorName, Model, Version, SerialNumber, Description)
        {

        }

        virtual internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _enableSignals = true;

            return true;
        }

        virtual public void CallMethod(IAdapterMethod methodn)
        {

        }

        virtual public void SendPropertyValue(IAdapterProperty Property, IAdapterValue Value)
        {

        }  
    }
}
