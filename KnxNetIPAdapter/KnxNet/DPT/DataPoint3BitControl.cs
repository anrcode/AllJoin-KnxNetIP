namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPoint3BitControl : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[] {"3.008", "3.007"};
            }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 1)
            {
                return null;
            }

            int input = data[0] & 0x0F;

            bool direction = (input >> 3) == 1;
            int step = input & 0x07;

            if (step != 0)
            {
                if (direction)
                {
                    step = step*-1;
                    step = step + 8;
                }
                else
                {
                    step = step*-1;
                    step = step + 8;
                    step = step*-1;
                }
            }

            return step;
        }

        public override byte[] ToASDU(object val)
        {
            var dataPoint = new byte[1];
            dataPoint[0] = 0x00;

            var input = 0;
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

            if (input > 7 || input < -7)
            {
                //Logger.Error("3.xxx", "input value received is not in a valid range");
                return dataPoint;
            }

            var direction = 8; // binary 1000


            if (input <= 0)
            {
                direction = 0;
                input = input*-1;
                input = input - 8;
                input = input*-1;
            }
            else
            {
                input = input*-1;
                input = input + 8;
            }

            int step = (input & 7);

            dataPoint[0] = (byte) (step | direction);

            return dataPoint;
        }
    }
}
