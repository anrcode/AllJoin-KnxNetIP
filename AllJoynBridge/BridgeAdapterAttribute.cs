using System;
using System.Collections.Generic;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterAttribute : IAdapterAttribute
    {
        // public properties
        public IAdapterValue Value { get; }

        public E_ACCESS_TYPE Access { get; set; }
        public IDictionary<string, string> Annotations { get; }
        public SignalBehavior COVBehavior { get; set; }

        public BridgeAdapterAttribute(string ObjectName, object DefaultData, E_ACCESS_TYPE access = E_ACCESS_TYPE.ACCESS_READ)
        {
            this.Value = new BridgeAdapterValue(ObjectName, DefaultData);
            this.Annotations = new Dictionary<string, string>();
            this.Access = access;
            this.COVBehavior = SignalBehavior.Never;
        }
    }
}
