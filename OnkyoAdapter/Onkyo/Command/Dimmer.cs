using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class Dimmer : CommandBase
    {
        public static readonly Dimmer State = new Dimmer()
        {
            CommandMessage = "DIMQSTN"
        };

        public static readonly Dimmer Change = new Dimmer()
        {
            CommandMessage = "DIMDIM"
        };

        #region Constructor / Destructor

        internal Dimmer()
        { }

        #endregion

        public EDimmerMode Mode { get; private set; }

        public override bool Match(string psStatusMessage)
        {
            var loMatch = Regex.Match(psStatusMessage, @"!1DIM(\w\w)");
            if (loMatch.Success)
            {
                this.Mode = loMatch.Groups[1].Value.ConvertHexValueToInt().ToEnum<EDimmerMode>(EDimmerMode.None);
                return true;
            }
            return false;
        }

    }
}
