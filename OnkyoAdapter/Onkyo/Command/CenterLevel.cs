using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class CenterLevel : CommandBase
    {
        public static readonly CenterLevel State = new CenterLevel()
        {
            CommandMessage = "CTLQSTN"
        };

        public static readonly CenterLevel Up = new CenterLevel()
        {
            CommandMessage = "CTLUP"
        };

        public static readonly CenterLevel Down = new CenterLevel()
        {
            CommandMessage = "CTLDOWN"
        };

        #region Constructor / Destructor

        internal CenterLevel()
        { }

        #endregion

        public int? Level { get; private set; }

        public string Display { get; private set; }

        public bool CanLevelDown()
        {
            return this.Level.GetValueOrDefault() > -12;
        }

        public bool CanLevelUp()
        {
            return this.Level.GetValueOrDefault() < 12;
        }

        public override bool Match(string psStatusMessage)
        {
            var loMatch = Regex.Match(psStatusMessage, @"!1CTL(.\w)");
            if (loMatch.Success)
            {
                this.Level = loMatch.Groups[1].Value.ConvertDbValueToInt();
                this.Display = this.Level.ConvertDbIntValueToDisplay();
                return true;
            }
            return false;
        }

    }
}
