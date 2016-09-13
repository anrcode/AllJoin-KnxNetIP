using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;


namespace DlmsAdapter.Dlms
{
    internal class DlmsSerial
    {
        private SerialDevice _serial;
        private DataReader _serialReader;
        private DataWriter _serialWriter;

        public event EventHandler<DlmsEventArgs> DataReceived;

        public DlmsSerial()
        {
        }

        public async Task<bool> Connect(string portName)
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector(); //gets all serial controllers in system and returns string
                var currentDevices = await DeviceInformation.FindAllAsync(aqs); //enumerates devices
                _serial = await SerialDevice.FromIdAsync(currentDevices[0].Id); //creates example of my controller

                _serial.BaudRate = 2400;
                _serial.DataBits = 7;
                _serial.WriteTimeout = TimeSpan.FromMilliseconds(500);
                _serial.ReadTimeout = TimeSpan.FromMilliseconds(2500);
                _serial.StopBits = SerialStopBitCount.One;
                _serial.Parity = SerialParity.Even;

                _serialReader = new DataReader(_serial.InputStream);
                _serialReader.InputStreamOptions = InputStreamOptions.Partial;

                _serialWriter = new DataWriter(_serial.OutputStream);
                _serialWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async void ReadData()
        {
            _serialWriter.WriteBytes(Encoding.ASCII.GetBytes("/?!\r\n"));
            var t = await _serialWriter.StoreAsync();

            byte[] buffer = new byte[1024];
            var bytesRead = await _serialReader.LoadAsync((uint)buffer.Length);
            var inputString = _serialReader.ReadString(bytesRead);

            this.DataReceived?.Invoke(this, new DlmsEventArgs(inputString));
        }
    }
}
