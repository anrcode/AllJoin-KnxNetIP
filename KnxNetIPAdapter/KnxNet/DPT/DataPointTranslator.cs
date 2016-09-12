using System;
using System.Collections.Generic;


namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPointTranslator
    {
        public static readonly DataPointTranslator Instance = new DataPointTranslator();
        private readonly IDictionary<string, DataPoint> _dataPoints = new Dictionary<string, DataPoint>();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DataPointTranslator()
        {
        }

        private DataPointTranslator()
        {
            Type[] types = new Type[] {  typeof(DataPointBoolean),
                                        typeof(DataPoint3BitControl),
                                        typeof(DataPoint8BitNoSignScaledScaling),
                                        typeof(DataPoint8BitNoSignNonScaledValue1UCount),
                                        typeof(DataPoint8BitNoSignScaledAngle),
                                        typeof(DataPoint8BitNoSignScaledPercentU8),
                                        typeof(DataPoint8BitSignRelativeValue),
                                        typeof(DataPoint2ByteFloatTemperature),
                                        typeof(DataPointTime),
                                        typeof(DataPointDate) };

            foreach (Type t in types)
            {
                DataPoint dp = (DataPoint)Activator.CreateInstance(t);

                foreach (string id in dp.Ids)
                {
                    _dataPoints.Add(id, dp);
                }
            }
        }

        public object FromASDU(string type, byte[] data)
        {
            try
            {
                DataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.FromASDU(data);
            }
            catch
            {
            }

            return null;
        }

        public byte[] ToASDU(string type, object value)
        {
            try
            {
                DataPoint dpt;
                if (_dataPoints.TryGetValue(type, out dpt))
                    return dpt.ToASDU(value);
            }
            catch
            {
            }

            return null;
        }
    }
}
