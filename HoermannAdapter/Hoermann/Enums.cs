using System;


namespace HoermannAdapter.Hoermann
{
    public enum McpCommand : int
    {
        Ping = 0,
        Error = 1,
        GetMac = 2,
        SetValue = 3,
        GetValue = 4,
        Debug = 5,
        Jmcp = 6,
        Login = 16,
        Logout = 17,
        GetUserIds = 32,
        GetUserName = 33,
        AddUser = 34,
        ChangePasswd = 35,
        RemoveUser = 36,
        SetUserRights = 37,
        GetName = 38,
        SetName = 39,
        GetUserRights = 40,
        AddPort = 41,
        AddGroup = 42,
        RemoveGroup = 43,
        SetGroupName = 44,
        GetGroupName = 45,
        SetGroupedPorts = 46,
        GetGroupedPorts = 47,
        GetPorts = 48,
        GetTypes = 49,
        GetState = 50,
        SetState = 51,
        GetPortName = 52,
        SetPortName = 53,
        SetType = 54,
        GetGroupIds = 64,
        InheritPort = 65,
        RemovePort = 66,
        SetSsl = 80,
        ScanWifi = 81,
        WifiFound = 82,
        GetWifiState = 83,
        HmGetTransition = 112
    }

    public enum McpGroupType : int
    {
        None = 0,
        SectionalDoor = 1,
        HorizontalSectionalDoor = 2,
        SwingGateSingle = 3,
        SwingGateDouble = 4,
        SlidingGate = 5,
        Door = 6,
        Light = 7,
        Other = 8,
        Jack = 9
    }

    public enum McpPortType : int
    {
        None = 0,
        Impuls = 1,
        AutoClose = 2,
        OnOff = 3,
        Up = 4,
        Down = 5,
        Half = 6,
        Walk = 7,
        Light = 8,
        On = 9,
        Off = 10
    }

    public enum McpPortState : int
    {
    }

    public enum McpWifiState : int
    {
        Connected = 0,
        NotConnected = 1,
        Busy = 64,
        ApNotFound = 128,
        SecurityMismatch = 129,
        AuthFailure = 130,
        ConnectionFailure = 131
    }
}
