using System.Collections.Generic;


namespace OnkyoAdapter.Onkyo
{
    internal static class ISCPDefinitions
    {
        internal static Dictionary<string, byte> EndCharacter = new Dictionary<string, byte>() 
        { 
            {"EOF", 0x1A}, //1
            {"CR" , 0x0D}, //2
            {"LF",  0x0A}, //3
            {"EM",  0x19}
        };

        public static List<string> EndCharacterKeys
        {
            get { return new List<string>(EndCharacter.Keys); }
        }

        public static string EmptyNetInfo
        {
            get { return "---"; }
        }
    }
}
