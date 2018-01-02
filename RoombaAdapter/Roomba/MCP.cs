using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;


namespace RoombaAdapter.Roomba
{
    internal class JmcpEventArgs : EventArgs
    {
        public MCP Mcp { get; private set; }
        public string Data { get; set; }

        internal JmcpEventArgs(MCP mcp)
        {
            this.Mcp = mcp;
            this.Data = Encoding.UTF8.GetString(mcp.Payload);
        }
    }

    internal delegate void JmcpEventHandler(object sender, JmcpEventArgs e);

    internal class TransitionEventArgs : EventArgs
    {
        public MCP Mcp { get; private set; }
        public byte Actual { get; set; }
        public byte Desired { get; set; }
        public bool AutoClose { get; set; }
        public short DriveTime { get; set; }
        public byte GkMain { get; set; }
        public byte GkSub { get; set; }
        public short Exst { get; set; }

        internal TransitionEventArgs(MCP mcp)
        {
            this.Mcp = mcp;
            //this.Data = Encoding.UTF8.GetString(mcp.Payload);
        }
    }

    internal delegate void TransitionEventHandler(object sender, TransitionEventArgs e);

    internal class MCP
    {
        public static int ADDRESS_SIZE = 12;
        public static int LENGTH_SIZE = 4;
        public static int FRAME_SIZE = 18;

        public int Tag { get; set; }
        public uint Token { get; set; }
        public McpCommand Command { get; set; }
        public byte[] Payload { get; set; }


        private int GetChecksum()
        {
            int checksum = (18 / 2) + (this.Payload != null ? this.Payload.Length : 0);
            checksum += this.Tag;
            checksum += (int)(this.Token & 0xff);
            checksum += (int)(this.Token >> 8 & 0xff);
            checksum += (int)(this.Token >> 16 & 0xff);
            checksum += (int)(this.Token >> 24 & 0xff);
            checksum += (int)this.Command;
            if(this.Payload != null)
            {
                foreach(byte b in this.Payload)
                {
                    checksum += b;
                }
            }

            return checksum & 0xff;
        }

        public byte[] ToByteArray()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            int length = (18 / 2) +  (this.Payload != null ? this.Payload.Length : 0);
            bw.Write((byte)((length >> 8) & 0xff));
            bw.Write((byte)(length & 0xff));
            bw.Write((byte)this.Tag);
            bw.Write(this.Token);
            bw.Write((byte)this.Command);
            if (this.Payload != null)
            {
                bw.Write(this.Payload);
            }
            bw.Write((byte)this.GetChecksum());

            return ms.ToArray();
        }

        public static MCP FromByteArray(byte[] data)
        {
            var ms = new MemoryStream(data);
            var bw = new BinaryReader(ms);

            int length = bw.ReadUInt16();

            var mcp = new MCP();
            mcp.Tag = bw.ReadByte();
            mcp.Token = bw.ReadUInt32();
            byte commandByte = bw.ReadByte();
            mcp.Command = (McpCommand)(commandByte & 0x7F);
            byte[] buffer = new byte[2048];
            int read = bw.Read(buffer, 0, buffer.Length);
            mcp.Payload = new byte[read];
            Array.Copy(buffer, mcp.Payload, read);

            return mcp;
        }

        public static MCP CreateLoginCmd(string user, string password)
        {
            return MCP.Create(McpCommand.Login, user, password);
        }

        public static MCP CreateChangePassCmd(string newpassword)
        {
            return MCP.Create(McpCommand.ChangePasswd, newpassword);
        }

        public static MCP CreateSetGatewayNameCmd(string name)
        {
            return MCP.Create(McpCommand.SetName, name);
        }

        public static MCP CreateGetWifiStateCmd()
        {
            return MCP.Create(McpCommand.GetWifiState);
        }

        public static MCP CreateScanWifiCmd()
        {
            return MCP.Create(McpCommand.ScanWifi);
        }

        public static MCP CreateGetUsersCmd()
        {
            return MCP.Create(McpCommand.Jmcp, "{\"cmd\":\"GET_USERS\"}");
        }

        public static MCP CreateAddUserCmd(string username, string password)
        {
            return MCP.Create(McpCommand.AddUser, username, password);
        }

        public static MCP CreateSetUserRightsCmd(int userId, IList<byte> groupIds)
        {
            return MCP.Create(McpCommand.SetUserRights, userId, groupIds.ToArray());
        }

        public static MCP CreateRemoveUserCmd(int userId)
        {
            return MCP.Create(McpCommand.RemoveUser, userId);
        }

        public static MCP CreateGetGroupsCmd()
        {
            return MCP.Create(McpCommand.Jmcp, "{\"cmd\":\"GET_GROUPS\"}");
        }

        public static MCP CreateAddGroupCmd()
        {
            return MCP.Create(McpCommand.AddGroup);
        }

        public static MCP CreateRemoveGroupCmd(int groupId)
        {
            return MCP.Create(McpCommand.RemoveGroup, groupId);
        }

        public static MCP CreateSetGroupNameCmd(int groupId, string name)
        {
            return MCP.Create(McpCommand.SetGroupName, groupId, name);
        }

        public static MCP CreateSetGroupTypeCmd(int groupId, McpGroupType type)
        {
            return MCP.Create(McpCommand.SetValue, groupId, (int)type);
        }

        public static MCP CreateSetRequestableCmd(int groupId, int portId)
        {
            return MCP.Create(McpCommand.SetValue, groupId + 16, portId);
        }

        public static MCP CreateSetGroupedPortsCmd(int groupId, IList<byte> portIds)
        {
            return MCP.Create(McpCommand.SetGroupedPorts, groupId, portIds.ToArray());
        }

        //public static MCP CreateSetValueCmd(int address, int value)
        //{
        //    return MCP.Create(McpCommand.SetValue, address, value);
        //}

        public static MCP CreateGetTransitionCmd(int newId)
        {
            return MCP.Create(McpCommand.HmGetTransition, newId);
        }

        public static MCP CreateGetPortsCmd()
        {
            return MCP.Create(McpCommand.GetPorts);
        }

        public static MCP CreateRemovePortCmd(int portId)
        {
            return MCP.Create(McpCommand.RemovePort, portId);
        }

        public static MCP CreateSetPortTypeCmd(int portId, McpPortType type)
        {
            return MCP.Create(McpCommand.SetType, portId, (int)type);
        }

        public static MCP CreateSetPortStateCmd(int portId, McpPortState state)
        {
            return MCP.Create(McpCommand.SetState, portId, (int)state);
        }

        public static MCP CreateJmcpGetValuesCmd()
        {
            return MCP.Create(McpCommand.Jmcp, "{\"cmd\":\"GET_VALUES\"}");
        }


        private static MCP Create(McpCommand cmd)
        {
            return new MCP()
            {
                Command = cmd
            };
        }

        private static MCP Create(McpCommand cmd, int val)
        {
            return new MCP()
            {
                Command = cmd,
                Payload = new byte[] { Convert.ToByte(val) }
            };
        }

        private static MCP Create(McpCommand cmd, int val, int val2)
        {
            return new MCP()
            {
                Command = cmd,
                Payload = new byte[] { Convert.ToByte(val), Convert.ToByte(val2) }
            };
        }

        private static MCP Create(McpCommand cmd, string val)
        {
            return new MCP()
            {
                Command = cmd,
                Payload = Encoding.UTF8.GetBytes(val)
            };
        }

        private static MCP Create(McpCommand cmd, string val, string val2)
        {
            var valBytes = Encoding.UTF8.GetBytes(val);
            var val2Bytes = Encoding.UTF8.GetBytes(val2);

            return new MCP()
            {
                Command = cmd,
                Payload = new byte[] { (byte)valBytes.Length }.Concat(valBytes).Concat(val2Bytes).ToArray()
            };
        }

        private static MCP Create(McpCommand cmd, int val, string val2)
        {
            return MCP.Create(cmd, val, Encoding.UTF8.GetBytes(val2));
        }

        private static MCP Create(McpCommand cmd, int val, byte[] val2)
        {
            return new MCP()
            {
                Command = cmd,
                Payload = new byte[] { Convert.ToByte(val) }.Concat(val2).ToArray()
            };
        }
    }
}
