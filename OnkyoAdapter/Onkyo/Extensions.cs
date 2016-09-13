using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace OnkyoAdapter.Onkyo
{
    internal static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotEmpty(this string value)
        {
            return !value.IsEmpty();
        }

        public static string FormatWith(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }
    }

    internal static class EnumExtensions
    {
        public static string ToDescription(this Enum value)
        {
            var loFieldInfo = value.GetType().GetField(value.ToString());
            if (loFieldInfo == null)
                return "Unk. Enum:{0}, Type:{1}".FormatWith(value, value.GetType().Name);
            DescriptionAttribute[] loAttrib = (DescriptionAttribute[])
                loFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return loAttrib.Length > 0 ? loAttrib[0].Description : value.ToString();

        }

        public static T ToEnum<T>(this int value, T peDefaultValue) where T : struct
        {
            try
            {
                return value.ToEnum<T>();
            }
            catch
            {
                return peDefaultValue;
            }
        }

        public static T ToEnum<T>(this int value) where T : struct
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        public static T ToEnum<T>(this string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T FromDescription<T>(this string value) where T : struct
        {
            foreach (T leEnumValue in Enum.GetValues(typeof(T)))
            {
                if (value == (leEnumValue as Enum).ToDescription())
                    return leEnumValue;
            }
            return default(T);
        }
    }

    internal static class ISCPExtensions
    {
        public static byte[] ToISCPCommandMessage(this string value)
        {
            return value.ToISCPCommandMessage(true);
        }

        public static byte[] ToISCPCommandMessage(this string value, bool pbAddMessageChar)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("value is null or empty.", "value");

            List<byte> loISCPMessage = new List<byte>();
            byte[] loCommandBytes = pbAddMessageChar ? Encoding.ASCII.GetBytes("!1" + value) : Encoding.ASCII.GetBytes(value);

            loISCPMessage.AddRange(ASCIIEncoding.ASCII.GetBytes("ISCP"));
            loISCPMessage.AddRange(BitConverter.GetBytes(0x00000010).Reverse());
            loISCPMessage.AddRange(BitConverter.GetBytes(loCommandBytes.Length + 1).Reverse());
            loISCPMessage.Add(Properties.Settings.ISCP_Version);
            loISCPMessage.AddRange(new byte[] { 0x00, 0x00, 0x00 });
            loISCPMessage.AddRange(loCommandBytes);

            if (value.StartsWith("NKY"))
            {
                loISCPMessage.Add(ISCPDefinitions.EndCharacter["EOF"]);
                loISCPMessage.Add(ISCPDefinitions.EndCharacter["CR"]);
                loISCPMessage.Add(ISCPDefinitions.EndCharacter["LF"]);
            }
            else
            {
                foreach (var lsKey in Properties.Settings.ISCP_EndCharacter)
                {
                    if (ISCPDefinitions.EndCharacter.ContainsKey(lsKey))
                        loISCPMessage.Add(ISCPDefinitions.EndCharacter[lsKey]);
                }
            }

            return loISCPMessage.ToArray();
        }

        public static List<string> ToISCPStatusMessage(this byte[] value, out byte[] poNotProcessingBytes)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentException("value is null or empty.", "value");

            if (value.Length <= Properties.Settings.ISCP_HeaderSize)
                throw new ArgumentException("value is not an ISCP-Message.", "value");

            const int lnDataSizePostion = 8;
            const int lnDataSizeBytes = 4;
            List<string> loReturnList = new List<string>();
            string lsMessage;
            int lnStartSearchIndex = 0;
            int lnISCPIndex;
            poNotProcessingBytes = new byte[0];

            while ((lnISCPIndex = NextHeaderIndex(value, lnStartSearchIndex)) > -1)
            {
                if (value.Length > (lnISCPIndex + lnDataSizePostion + 4))
                {
                    int lnDataSize = BitConverter.ToInt32(Enumerable.Take(value.Skip(lnISCPIndex + lnDataSizePostion), lnDataSizeBytes).Reverse().ToArray(), 0);
                    if (value.Length >= (lnISCPIndex + Properties.Settings.ISCP_HeaderSize + lnDataSize))
                    {
                        lsMessage = ConvertMessage(value, lnISCPIndex + Properties.Settings.ISCP_HeaderSize, lnDataSize);
                        loReturnList.Add(lsMessage);
                        lnStartSearchIndex = lnISCPIndex + Properties.Settings.ISCP_HeaderSize + lnDataSize;
                    }
                    else
                        break;
                }
                else
                    break;
            }

            if (value.Length > lnStartSearchIndex && !value.Skip(lnStartSearchIndex).All(item => item == 0x00))
            {
                poNotProcessingBytes = value.Skip(lnStartSearchIndex).ToArray();
            }

            return loReturnList;
        }

        public static int ConvertHexValueToInt(this string value)
        {
            return Convert.ToInt32(value.Trim(), 16);
        }

        public static long ConvertHexValueToLong(this string value)
        {
            return Convert.ToInt64(value.Trim(), 16);
        }

        public static byte ConvertHexValueToByte(this string value)
        {
            return Convert.ToByte(value.Trim(), 16);
        }

        public static byte[] ConvertHexValueToByteArray(this string value)
        {
            List<byte> loByteList = new List<byte>();
            var loMatch = System.Text.RegularExpressions.Regex.Match(value, @"(\w\w)");
            while (loMatch.Success)
            {
                loByteList.Add(loMatch.Groups[1].Value.ConvertHexValueToByte());
                loMatch = loMatch.NextMatch();
            }

            return loByteList.ToArray();
        }

        public static string ConverIntValueToHexString(this int value)
        {
            return "{0:x2}".FormatWith(value).ToUpper();
        }

        public static int? ConvertDbValueToInt(this string value)
        {
            int lnMulti = 1;
            if (value.Length == 2)
            {
                if (value == "00")
                    return 0;
                char lsFirstToken = value.First();
                if (lsFirstToken == '-')
                    lnMulti = -1;
                return value[1].ToString().ConvertHexValueToInt() * lnMulti;
            }
            return null;
        }

        public static string ConvertIntToDbValue(this int value)
        {
            if (value == 0)
                return "00";
            if (value < 0)
                return "-{0}".FormatWith((value * -1).ConverIntValueToHexString()[1]);
            if (value > 0)
                return "+{0}".FormatWith(value.ConverIntValueToHexString()[1]);
            return "00";
        }

        public static string ConvertDbIntValueToDisplay(this int? value)
        {
            if (value.HasValue)
            {
                if (value.Value == 0)
                    return "0 dB";
                if (value.Value > 0)
                    return "+{0} dB".FormatWith(value);
                if (value.Value < 0)
                    return "-{0} dB".FormatWith(value * -1);
            }
            return "--";
        }

        private static int NextHeaderIndex(byte[] poBytes, int pnStart)
        {
            byte[] loISCPBytes = ASCIIEncoding.ASCII.GetBytes("ISCP");

            if (poBytes.Length > (pnStart + loISCPBytes.Length))
            {
                for (int i = pnStart; i < poBytes.Length; i++)
                {
                    if (!IsMatch(poBytes, i, loISCPBytes))
                        continue;
                    return i; //ISCP Header Found
                }
            }
            return -1;
        }

        private static bool IsMatch(byte[] poSourceArray, int pnPosition, byte[] poSearchArray)
        {
            if (poSearchArray.Length > (poSourceArray.Length - pnPosition))
                return false;

            for (int i = 0; i < poSearchArray.Length; i++)
                if (poSourceArray[pnPosition + i] != poSearchArray[i])
                    return false;

            return true;
        }

        private static string ConvertMessage(byte[] poSourceArray, int pnStartIndex, int pnCount)
        {
            int lnCount = pnCount;
            for (int i = pnStartIndex + pnCount - 1; i >= 0; i--)
            {
                if (ISCPDefinitions.EndCharacter.Values.Contains(poSourceArray[i]))
                    lnCount--;
                else
                    break;
            }
            string lsMessage = UnicodeEncoding.UTF8.GetString(poSourceArray, pnStartIndex, lnCount);

            //'/0' entfernen
            return lsMessage.Replace(char.ConvertFromUtf32(0), "");
        }
    }
}
