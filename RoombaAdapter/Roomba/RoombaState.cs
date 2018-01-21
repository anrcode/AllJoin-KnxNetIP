using System;


namespace RoombaAdapter.Roomba
{
    internal delegate void RoombaStateEventHandler(object sender, RoombaState e);

    internal class RoombaState : EventArgs
    {
        public string Country { get; internal set; }

        public bool MapUploadAllowed { get; internal set; }

        public uint BatPct { get; internal set; }

        public RoombaBinState Bin { get; internal set; }

        public CleanMissionState CleanMission { get; internal set; }

        public CapState Cap { get; internal set; }

        public bool VacHigh { get; internal set; }

        public bool BinPause { get; internal set; }

        public bool CarpetBoost { get; internal set; }

        public bool OpenOnly { get; internal set; }

        public bool TwoPass { get; internal set; }

        public bool SchedHold { get; internal set; }

        public PoseState Pose { get; internal set; }

        public RoombaState()
        {
            this.Bin = new RoombaBinState();
            this.CleanMission = new CleanMissionState();
            this.Cap = new CapState();
            this.Pose = new PoseState();
        }
    }

    public sealed class RoombaBinState
    {
        public bool Present { get; internal set; }

        public bool IsFull { get; internal set; }
    }

    public sealed class CleanMissionState
    {
        public string Cycle { get; internal set; }

        public string Phase { get; internal set; }

        public uint Sqft { get; internal set; }
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

    public sealed class PoseState
    {
        public int Theta { get; internal set; }

        public int X { get; internal set; }

        public int Y { get; internal set; }
    }
}
