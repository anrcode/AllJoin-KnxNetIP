using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class NLT : CommandBase
    {
        #region Constructor / Destructor

        internal NLT()
        { }

        #endregion

        public ENetworkServiceType NetworkSource { get; private set; }
        public ENetworkListUIType UiType { get; private set; }
        public ENetworkListLayerInfo LayerInfo { get; private set; }
        public int CurrentCursorPosition { get; private set; }
        public int NumberOfListItems { get; private set; }
        public int NumberOfLayer { get; private set; }
        public string Reserved { get; set; }
        public ENetworkListLeftIcon IconLeft { get; private set; }
        public ENetworkListRightIcon IconRight { get; private set; }
        public ENetworkListStatusInfo StatusInfo { get; private set; }
        public string CurrentTitle { get; private set; }

        public override bool Match(string psStatusMessage)
        {
            var loMatch = Regex.Match(psStatusMessage, @"!1NLT(\w{2})(\w{1})(\w{1})(\w{4})(\w{4})(\w{2})(\w{2})(\w{2})(\w{2})(\w{2})(.*)");
            if (loMatch.Success)
            {
                this.NetworkSource = loMatch.Groups[1].Value.ConvertHexValueToInt().ToEnum<ENetworkServiceType>(ENetworkServiceType.None);
                this.UiType = loMatch.Groups[2].Value.ConvertHexValueToInt().ToEnum<ENetworkListUIType>(ENetworkListUIType.None);
                this.LayerInfo = loMatch.Groups[3].Value.ConvertHexValueToInt().ToEnum<ENetworkListLayerInfo>(ENetworkListLayerInfo.None);
                this.CurrentCursorPosition = loMatch.Groups[4].Value.ConvertHexValueToInt();
                this.NumberOfListItems = loMatch.Groups[5].Value.ConvertHexValueToInt();
                this.NumberOfLayer = loMatch.Groups[6].Value.ConvertHexValueToInt();
                this.Reserved = loMatch.Groups[7].Value;
                this.IconLeft = loMatch.Groups[8].Value.ConvertHexValueToInt().ToEnum<ENetworkListLeftIcon>(ENetworkListLeftIcon.None);
                this.IconRight = loMatch.Groups[9].Value.ConvertHexValueToInt().ToEnum<ENetworkListRightIcon>(ENetworkListRightIcon.None);
                this.StatusInfo = loMatch.Groups[10].Value.ConvertHexValueToInt().ToEnum<ENetworkListStatusInfo>(ENetworkListStatusInfo.None);
                this.CurrentTitle = loMatch.Groups[11].Value;
                return true;
            }
            return false;
        }

        public void Clear()
        {
            this.NetworkSource = ENetworkServiceType.None;
            this.UiType = ENetworkListUIType.None;
            this.LayerInfo = ENetworkListLayerInfo.None;
            this.CurrentCursorPosition =  this.NumberOfListItems = this.NumberOfLayer = 0;
            this.Reserved = string.Empty;
            this.IconLeft = ENetworkListLeftIcon.None;
            this.IconRight = ENetworkListRightIcon.None;
            this.StatusInfo = ENetworkListStatusInfo.None;
            this.CurrentTitle = string.Empty;
        }
    }
}
