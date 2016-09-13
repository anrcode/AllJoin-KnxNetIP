using System;
using System.Collections.Generic;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterSignal : IAdapterSignal
    {
        // public properties
        public string Name { get; }

        public IList<IAdapterValue> Params { get; }

        public BridgeAdapterSignal(string ObjectName)
        {
            this.Name = ObjectName;
            this.Params = new List<IAdapterValue>();
        }
    }
}
