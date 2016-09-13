using System;
using System.Collections.Generic;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterProperty<T> : IAdapterProperty
    {
        // public properties
        public string Name { get; }
        public string InterfaceHint { get; }
        public IList<IAdapterAttribute> Attributes { get; }

        public T Device { get; private set; }

        public BridgeAdapterProperty(T device, string ObjectName, string IfHint)
        {
            this.Device = device;

            this.Name = ObjectName;
            this.InterfaceHint = IfHint;
            this.Attributes = new List<IAdapterAttribute>();
        }
    }
}
