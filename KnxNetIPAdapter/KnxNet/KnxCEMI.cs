using System;


namespace KnxNetIPAdapter.KnxNet
{
    // CEMI
    // +--------+--------+--------+--------+----------------+----------------+--------+----------------+
    // |  Msg   |Add.Info| Ctrl 1 | Ctrl 2 | Source Address | Dest. Address  |  Data  |      APDU      |
    // | Code   | Length |        |        |                |                | Length |                |
    // +--------+--------+--------+--------+----------------+----------------+--------+----------------+
    //   1 byte   1 byte   1 byte   1 byte      2 bytes          2 bytes       1 byte      2 bytes
    //
    //  Message Code    = 0x11 - a L_Data.req primitive
    //      COMMON EMI MESSAGE CODES FOR DATA LINK LAYER PRIMITIVES
    //          FROM NETWORK LAYER TO DATA LINK LAYER
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          | Data Link Layer Primitive | Message Code | Data Link Layer Service | Service Description | Common EMI Frame |
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          |        L_Raw.req          |    0x10      |                         |                     |                  |
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          |                           |              |                         | Primitive used for  | Sample Common    |
    //          |        L_Data.req         |    0x11      |      Data Service       | transmitting a data | EMI frame        |
    //          |                           |              |                         | frame               |                  |
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          |        L_Poll_Data.req    |    0x13      |    Poll Data Service    |                     |                  |
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          |        L_Raw.req          |    0x10      |                         |                     |                  |
    //          +---------------------------+--------------+-------------------------+---------------------+------------------+
    //          FROM DATA LINK LAYER TO NETWORK LAYER
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          | Data Link Layer Primitive | Message Code | Data Link Layer Service | Service Description |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |        L_Poll_Data.con    |    0x25      |    Poll Data Service    |                     |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |                           |              |                         | Primitive used for  |
    //          |        L_Data.ind         |    0x29      |      Data Service       | receiving a data    |
    //          |                           |              |                         | frame               |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |        L_Busmon.ind       |    0x2B      |   Bus Monitor Service   |                     |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |        L_Raw.ind          |    0x2D      |                         |                     |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |                           |              |                         | Primitive used for  |
    //          |                           |              |                         | local confirmation  |
    //          |        L_Data.con         |    0x2E      |      Data Service       | that a frame was    |
    //          |                           |              |                         | sent (does not mean |
    //          |                           |              |                         | successful receive) |
    //          +---------------------------+--------------+-------------------------+---------------------+
    //          |        L_Raw.con          |    0x2F      |                         |                     |
    //          +---------------------------+--------------+-------------------------+---------------------+

    //  Add.Info Length = 0x00 - no additional info
    //  Control Field 1 = see the bit structure above
    //  Control Field 2 = see the bit structure above
    //  Source Address  = 0x0000 - filled in by router/gateway with its source address which is
    //                    part of the KNX subnet
    //  Dest. Address   = KNX group or individual address (2 byte)
    //  Data Length     = Number of bytes of data in the APDU excluding the TPCI/APCI bits
    //  APDU            = Application Protocol Data Unit - the actual payload including transport
    //                    protocol control information (TPCI), application protocol control
    //                    information (APCI) and data passed as an argument from higher layers of
    //                    the KNX communication stack
    //

    internal class KnxCEMI
    {
        public byte message_code;
        public byte aditional_info_length;
        public byte[] aditional_info;
        public byte control_field_1;
        public byte control_field_2;
        public string source_address;
        public string destination_address;
        public byte[] apdu;
        private bool _isstatus = false;

        public bool IsEvent
        {
            get { return (message_code == 0x29) && (apdu[0] >> 4 == 8); }
        }

        public bool IsStatus
        {
            get { return (message_code == 0x29) && (apdu[0] >> 4 == 4); }
        }
       
        public static KnxCEMI CreateActionCEMI(byte messageCode, string destinationAddress, byte[] asdu)
        {
            KnxCEMI cemi = new KnxCEMI()
            {
                message_code = messageCode != 0x00 ? messageCode : (byte)0x11,
                aditional_info_length = 0,
                control_field_1 = 0xAC,
                control_field_2 = KnxHelper.IsAddressIndividual(destinationAddress) ? (byte)0x50 : (byte)0xF0,
                destination_address = destinationAddress,
                apdu = new byte[asdu.Length]
            };

            cemi.apdu[0] = (byte)(0x80 | (asdu[0] & 0x3f));
            for (var i = 1; i < asdu.Length; i++)
            {
                cemi.apdu[i] = asdu[i];
            }

            return cemi;
        }

        public static KnxCEMI CreateStatusCEMI(byte messageCode, string destinationAddress)
        {
            KnxCEMI cemi = new KnxCEMI()
            {
                message_code = messageCode != 0x00 ? messageCode : (byte)0x11,
                aditional_info_length = 0,
                control_field_1 = 0xAC,
                control_field_2 = KnxHelper.IsAddressIndividual(destinationAddress) ? (byte)0x50 : (byte)0xF0,
                destination_address = destinationAddress,
                apdu = new byte[] { 0x00 },
                _isstatus = true
            };

            return cemi;
        }

        public byte[] ToBytes()
        {
            var cemiBytes = new byte[apdu.Length + 10];
            cemiBytes[0] = message_code;

            cemiBytes[1] = aditional_info_length;

            cemiBytes[2] = control_field_1;
            cemiBytes[3] = control_field_2;

            cemiBytes[4] = 0x00;
            cemiBytes[5] = 0x00;

            var dst_address = KnxHelper.GetAddress(destination_address);
            cemiBytes[6] = dst_address[0];
            cemiBytes[7] = dst_address[1];

            cemiBytes[8] = (byte)apdu.Length;

            cemiBytes[9] = 0x00;
            if (!_isstatus)
            {
                cemiBytes[10] = 0x80;
            }

            cemiBytes[10] |= (byte)(apdu[0] & 0x3f);
            for (var i = 1; i < apdu.Length; i++)
            {
                cemiBytes[10 + i] = apdu[i];
            }

            return cemiBytes;
        }

        public static KnxCEMI FromBytes(bool threeLevelGroupAssigning, byte[] cemiBytes)
        {
            var cemi = new KnxCEMI();
            cemi.message_code = cemiBytes[0];
            cemi.aditional_info_length = cemiBytes[1];

            if (cemi.aditional_info_length > 0)
            {
                cemi.aditional_info = new byte[cemi.aditional_info_length];
                Array.Copy(cemiBytes, 2, cemi.aditional_info, 0, cemi.aditional_info_length);
            }

            cemi.control_field_1 = cemiBytes[2 + cemi.aditional_info_length];
            cemi.control_field_2 = cemiBytes[3 + cemi.aditional_info_length];
            cemi.source_address = KnxHelper.GetIndividualAddress(new[] { cemiBytes[4 + cemi.aditional_info_length], cemiBytes[5 + cemi.aditional_info_length] });

            cemi.destination_address =
                KnxHelper.GetKnxDestinationAddressType(cemi.control_field_2).Equals(KnxHelper.KnxDestinationAddressType.INDIVIDUAL)
                    ? KnxHelper.GetIndividualAddress(new[] { cemiBytes[6 + cemi.aditional_info_length], cemiBytes[7 + cemi.aditional_info_length] })
                    : KnxHelper.GetGroupAddress(new[] { cemiBytes[6 + cemi.aditional_info_length], cemiBytes[7 + cemi.aditional_info_length] }, threeLevelGroupAssigning);

            var data_length = Math.Min(cemiBytes[8 + cemi.aditional_info_length], cemiBytes.Length - 10); // AR
            if (data_length > 0)
            {
                cemi.apdu = new byte[data_length];
                Array.Copy(cemiBytes, 10 + cemi.aditional_info_length, cemi.apdu, 0, data_length);
            }

            return cemi;
        }
    }
}
