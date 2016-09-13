using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class InputSelector : CommandBase
    {
        public static InputSelector StateCommand()
        {
            string lsCommandMessage = "SLIQSTN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "SLZQSTN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "SL3QSTN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "SL4QSTN";
                    break;
            }
            return new InputSelector()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static InputSelector Chose(EInputSelector peInputSelector)
        {
            string lsCommandMessage = "SLI{0}";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "SLZ{0}";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "SL3{0}";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "SL4{0}";
                    break;
            }
            return new InputSelector()
            {
                CommandMessage = lsCommandMessage.FormatWith(((int)peInputSelector).ConverIntValueToHexString())
            };
        }

        #region Constructor / Destructor

        internal InputSelector()
        { }

        #endregion

        public EInputSelector CurrentInputSelector { get; private set; }

        public override bool Match(string psStatusMessage)
        {
            string lsMatchToken = "SLI";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsMatchToken = "SLZ";
                    break;
                case EZone.Zone3:
                    lsMatchToken = "SL3";
                    break;
                case EZone.Zone4:
                    lsMatchToken = "SL4";
                    break;
            }
            var loMatch = Regex.Match(psStatusMessage, @"!1{0}(\w\w)".FormatWith(lsMatchToken));
            if (loMatch.Success)
            {
                this.CurrentInputSelector = loMatch.Groups[1].Value.ConvertHexValueToInt().ToEnum<EInputSelector>();
                return true;
            }
            return false;   
        }
    }
}
