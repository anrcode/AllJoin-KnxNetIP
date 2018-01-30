using System;


namespace RoombaAdapter.Roomba
{
    internal delegate void RobotStateEventHandler(object sender, RobotState e);
    internal delegate void PoseChangedEventHandler(object sender, PoseState e);
    internal delegate void CleanMissionStateEventHandler(object sender, CleanMissionState e);

    internal class RobotState : EventArgs
    {
        public string Country { get; internal set; }

        public bool MapUploadAllowed { get; internal set; }

        public uint BatPct { get; internal set; }

        public BinState Bin { get; internal set; }

        public CapState Cap { get; internal set; }

        public bool VacHigh { get; internal set; }

        public bool BinPause { get; internal set; }

        public bool CarpetBoost { get; internal set; }

        public bool OpenOnly { get; internal set; }

        public bool TwoPass { get; internal set; }

        public bool SchedHold { get; internal set; }

        public RobotState()
        {
            this.Bin = new BinState();
            this.Cap = new CapState();
        }
    }

    public sealed class BinState
    {
        public bool Present { get; internal set; }

        public bool IsFull { get; internal set; }
    } 

    public sealed class CapState
    {
        public uint Pose { get; internal set; }

        public uint Ota { get; internal set; }

        public uint MultiPass { get; internal set; }

        public uint CarpetBoost { get; internal set; }

        public uint PP { get; internal set; }

        public uint BinFullDetect { get; internal set; }

        public uint LangOta { get; internal set; }

        public uint Maps { get; internal set; }

        public uint Edge { get; internal set; }

        public uint Eco { get; internal set; }
    }

    internal class CleanMissionState : EventArgs
    {
        public string Cycle { get; internal set; }

        public string Phase { get; internal set; }

        public uint MissionNo { get; internal set; }

        public TimeSpan MissionTime { get; internal set; }

        public uint Sqft { get; internal set; }
    }

    internal class PoseState : EventArgs
    {
        public int Theta { get; internal set; }

        public int X { get; internal set; }

        public int Y { get; internal set; }
    }
}
