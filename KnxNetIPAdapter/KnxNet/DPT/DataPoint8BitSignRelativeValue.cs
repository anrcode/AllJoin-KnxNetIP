namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPoint8BitSignRelativeValue : DataPoint
    {
        public override string[] Ids
        {
            get { return new[] { "6.001", "6.010" }; }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 2)
            {
                return null;
            }

            return (int) ((sbyte) data[1]);
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
                //Logger.Error("6.xxx", "input value received is not a valid type");
                return dataPoint;
            }
            
            if (input < -128 || input > 127)
            {
                //Logger.Error("6.xxx", "input value received is not in a valid range");
                return dataPoint;
            }

            dataPoint[1] = (byte) ((sbyte) ((int) input));

            return dataPoint;
        }
    }
}