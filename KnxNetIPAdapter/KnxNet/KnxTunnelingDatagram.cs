using System;
using System.Diagnostics;


namespace KnxNetIPAdapter.KnxNet
{
    internal class KnxNetTunnelingDatagram
    {
        // HEADER
        public int header_length;
        public byte protocol_version;
        public ushort service_type;
        public int total_length;

        // CONNECTION
        public byte channel_id;
        public byte status;
        public byte sequence_number;

        public byte[] data;

        public static KnxNetTunnelingDatagram FromBytes(byte[] datagram)
        {
            if((datagram == null) || (datagram.Length < 8))
            {
                Debug.WriteLine("Received parial datagram " + BitConverter.ToString(datagram));
                return null;
            }

            if(datagram[4] + datagram[5] != datagram.Length)
            {
                Debug.WriteLine("Datagram length validaton failed " + BitConverter.ToString(datagram));
                return null;
            }

            var header = new KnxNetTunnelingDatagram()
            {
                header_length = datagram[0],
                protocol_version = datagram[1],
                service_type = (ushort)((datagram[2] << 8) + datagram[3]),
                total_length = datagram[4] + datagram[5],
                channel_id = datagram[6],
                status = datagram[7]
            };

            if (datagram.Length >= 10)
            {
                header.sequence_number = datagram[8];

                header.data = new byte[datagram.Length - 10];
                Array.Copy(datagram, 10, header.data, 0, header.data.Length);               
            }

            return header;
        }
    }
}
