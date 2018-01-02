﻿using System;
using System.Linq;
using BridgeRT;
using SparkAlljoyn;
using RoombaAdapter.Roomba;
using RoombaAdapter.Roomba.Discovery;


namespace RoombaAdapter
{
    internal class RoombaAdapter : BridgeAdapter
    {
        private RoombaDiscovery _discovery;

        public RoombaAdapter() : base("Roomba")
        {
            
        }

        override public uint Initialize()
        {
            RoombaDiscovery.DeviceDiscovered += RoombaDiscovery_DeviceDiscovered;
            RoombaDiscovery.DeviceRemoved += RoombaDiscovery_DeviceRemoved;
            _discovery = new RoombaDiscovery();

            return ERROR_SUCCESS;
        }

        private void RoombaDiscovery_DeviceDiscovered(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var conn = e.Device as RoombaConnection;
            conn.Connect();

            var device = new RoombaDevice(this, conn, "Roomba", "Roomba", "Roomba", "1.0.0.0", e.DeviceId, "Roomba");
            devices.Add(device);
            this.NotifyDeviceArrival(device);

            // TEMP
            var msg = MCP.CreateLoginCmd("admin", "0000");
            conn.SendMessage(msg);
        }

        private void RoombaDiscovery_DeviceRemoved(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var matchingDevices = devices.Where(d => d.SerialNumber == e.DeviceId).ToList();
            foreach (var device in matchingDevices)
            {
                this.NotifyDeviceRemoval(device);
                devices.Remove(device);
            }
        }

        override public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        override public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            ValuePtr = ((BridgeAdapterProperty<RoombaDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

            return ERROR_SUCCESS;
        }

        override public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        override public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterMethod<RoombaDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}