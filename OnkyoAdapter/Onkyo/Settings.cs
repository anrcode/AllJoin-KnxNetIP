namespace OnkyoAdapter.Onkyo.Properties
{
    public sealed partial class Settings
    {
        private static byte _iscpVersion = 1;

        public static byte ISCP_Version
        {
            get
            {
                return _iscpVersion;
            }
            set
            {
                _iscpVersion = value;
            }
        }

        private static int _iscpHeaderSize = 16;

        public static int ISCP_HeaderSize
        {
            get
            {
                return _iscpHeaderSize;
            }
            set
            {
                _iscpHeaderSize = value;
            }
        }

        //[global::System.Configuration.UserScopedSettingAttribute()]
        //[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        //[global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
        //    "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
        //    "tring>CR</string>\r\n  <string>LF</string>\r\n  <string>EOF</string>\r\n</ArrayOfStrin" +
        //    "g>")]
        private static string[] _iscpEndCharacter = new string[] { "CR", "LF", "EOF" };

        public static string[] ISCP_EndCharacter
        {
            get
            {
                return _iscpEndCharacter;
            }
            set
            {
                _iscpEndCharacter = value;
            }
        }
    }
}
