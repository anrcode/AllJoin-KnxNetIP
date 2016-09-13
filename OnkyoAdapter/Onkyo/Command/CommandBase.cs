using System.Collections.Generic;


namespace OnkyoAdapter.Onkyo.Command
{
    internal abstract class CommandBase
    {
        private static List<CommandBase> moCommandList = null;
        public static List<CommandBase> CommandList
        {
            get
            {
                if (moCommandList == null)
                {
                    moCommandList = new List<CommandBase>();
                    moCommandList.Add(new Power());
                    moCommandList.Add(new MasterVolume());
                    moCommandList.Add(new InputSelector());
                    moCommandList.Add(new ListeningMode());
                    moCommandList.Add(new AudioMuting());
                    moCommandList.Add(new OSD());
                    moCommandList.Add(new Preset());
                    moCommandList.Add(new Tuning());
                    moCommandList.Add(new NetTune());
                    moCommandList.Add(new NetListInfo());
                    moCommandList.Add(new NetAlbumName());
                    moCommandList.Add(new NetArtistName());
                    moCommandList.Add(new NetTimeInfo());
                    moCommandList.Add(new NetTitleName());
                    moCommandList.Add(new NetTrackInfo());
                    moCommandList.Add(new NetPlayStatus());
                    moCommandList.Add(new NetJacketArt());
                    moCommandList.Add(new NLT());
                    moCommandList.Add(new NetKeyboard());
                    moCommandList.Add(new Tone());
                    moCommandList.Add(new CenterLevel());
                    moCommandList.Add(new SubwooferLevel());
                    moCommandList.Add(new Dimmer());
                }
                return moCommandList;
            }
        }

        public string CommandMessage { get; protected set; }

        public abstract bool Match(string psStatusMessage);
    }
}
