namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPoint8BitNoSignNonScaledValue1UCount : DataPoint
    {
        public override string[] Ids
        {
            get { return new[] { "5.010", "20.102" }; }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 2)
            {
                return null;
            }

            return (int) data[1];
        }

        public override byte[] ToASDU(object val)
        {
            var dataPoint = new byte[] { 0x00, 0x00 };

            int input = 0;
            if (val is int)
                input = ((int) val);
            else if (val is float)
                input = (int) ((float) val);
            else if (val is long)
                input = (int) ((long) val);
            else if (val is double)
                input = (int) ((double) val);
            else if (val is decimal)
                input = (int) ((decimal) val);
            else
            {
                //Logger.Error("5.010", "input value received is not a valid type");
                return dataPoint;
            }

            if (input < 0 || input > 255)
            {
                //Logger.Error("5.010", "input value received is not in a valid range");
                return dataPoint;
            }
            
            dataPoint[1] = (byte) input;

            return dataPoint;
        }
    }
}