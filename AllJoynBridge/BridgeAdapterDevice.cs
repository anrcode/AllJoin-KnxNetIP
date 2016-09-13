using System;
using System.Collections.Generic;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterDevice<A> : IAdapterDevice where A:BridgeAdapter
    {
        // Device Arrival and Device Removal Signal Indices
        private const int CHANGE_OF_VALUE_SIGNAL_INDEX = 0;
        private const int CHANGE_OF_VALUE_SIGNAL_PARAM_INDEX = 0;

        protected A _adapter = null;
        protected bool _enableSignals = false;

        // Object Name
        public string Name { get; }

        public string Vendor { get; }

        public string Model { get; }

        public string Version { get; }

        public string FirmwareVersion { get; }

        public string SerialNumber { get; }

        public string Description { get; }

        // Device properties
        public IList<IAdapterProperty> Properties { get; }

        // Device methods
        public IList<IAdapterMethod> Methods { get; }

        // Device signals
        public IList<IAdapterSignal> Signals { get; }

        // Control Panel Handler
        public IControlPanelHandler ControlPanelHandler
        {
            get
            {
                return null;
            }
        }

        public IAdapterIcon Icon
        {
            get
            {
                return null;
            }
        }

        public BridgeAdapterDevice(
            A adapter,
            string Name,
            string VendorName,
            string Model,
            string Version,
            string SerialNumber,
            string Description)
        {
            _adapter = adapter;

            this.Name = Name;
            this.Vendor = VendorName;
            this.Model = Model;
            this.Version = Version;
            this.FirmwareVersion = Version;
            this.SerialNumber = SerialNumber;
            this.Description = Description;

            this.Properties = new List<IAdapterProperty>();
            this.Methods = new List<IAdapterMethod>();
            this.Signals = new List<IAdapterSignal>();

            this.CreateProperties();
            this.CreateMethods();
        }

        virtual protected void CreateProperties()
        {

        }

        public IAdapterValue GetPropertyValue(IAdapterProperty Property, string AttributeName)
        {
            // find corresponding attribute
            foreach (var attribute in Property.Attributes)
            {
                if (attribute.Value.Name == AttributeName)
                {
                    return attribute.Value;
                }
            }

            return null;
        } 
        
        protected void UpdatePropertyValue(IAdapterProperty property, IAdapterAttribute attribute, object newValue)
        {
            //if (newValue.Equals(attribute.Value.Data)) return;

            attribute.Value.Data = newValue;
            if(((BridgeAdapterAttribute)attribute).COVBehavior == SignalBehavior.Always)
            {
                this.NotifyChangeOfValueSignal(property, attribute);
            }          
        }    

        virtual protected void CreateMethods()
        {

        }      

        protected void AddChangeOfValueSignal(IAdapterProperty Property)
        {
            var covSignal = new BridgeAdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            var propertyHandle = new BridgeAdapterValue(Constants.COV__PROPERTY_HANDLE, null);
            var attrHandle = new BridgeAdapterValue(Constants.COV__ATTRIBUTE_HANDLE, null);
            covSignal.Params.Add(propertyHandle);
            covSignal.Params.Add(attrHandle);
            this.Signals.Add(covSignal);
        }

        public void NotifyChangeOfValueSignal(IAdapterProperty Property, IAdapterAttribute Attribute)
        {
            if (!_enableSignals) return;

            IAdapterSignal covSignal = this.Signals[CHANGE_OF_VALUE_SIGNAL_INDEX];
            covSignal.Params[0].Data = Property;
            covSignal.Params[1].Data = Attribute.Value;
            _adapter.NotifySignalListener(covSignal);
        }
    }
}
