using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BoschCFW
{
    class Program
    {
        public static UsbEndpointWriter Writer;
        public static UsbEndpointReader Reader;
        static UsbDevice Bike = null;

        static void Main()
        {


            UsbRegDeviceList Devices = null;

            while (Bike == null)
            {
                while (Devices == null || Devices?.Count == 0)
                {
                    Devices = UsbDevice.AllDevices;
                    Thread.Sleep(1000);
                }

                foreach (var Device in Devices.ToList())
                {
                    bool Output;
                    try { 
                       Output = UsbDevice.OpenUsbDevice(ref Device.DeviceInterfaceGuids[0], out Bike);
                    }
                    catch { }
                    if (Bike != null) { break; }
                }
                Thread.Sleep(100);
            }

            



            //Determined from USBView https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/usbview?redirectedfrom=MSDN
            //idVendor: 0x108C = Robert Bosch GmbH
            //idProduct: 0x0182
            //iSerialNumber: 0x03
            //English(United States)  "953021702D3"

            //var Serial = new UsbDeviceFinder("953021702D3");
            //var PIDVID = new UsbDeviceFinder(4236, 386);
            //UsbDevice Bike = UsbDevice.OpenUsbDevice(PIDVID);

            Bike.Open();
            

            Writer = Bike.OpenEndpointWriter(WriteEndpointID.Ep01);
            Reader = Bike.OpenEndpointReader(ReadEndpointID.Ep01);
            Reader.DataReceivedEnabled = true;


            //Writer.Abort();
            //Writer.Reset();
            //Writer.Flush();
            //Reader.Abort();
            //Reader.ReadFlush();
            //Reader.Flush();
            //Reader.Reset();
            

            string Response = null;

            while (Response == null)
            {

                for (char c = char.MinValue; c <= char.MaxValue; c++)
                {
                    Response = Write(c.ToString());                    
                }
                Thread.Sleep(1000);
            }
            Debug.WriteLine(Response);

        }


        static string Write(string Command)
        {
            Command.Replace("\r", "");
            byte[] BytesToWrite = ASCIIEncoding.ASCII.GetBytes(Command + "\r");

            var Error = Writer.Write(BytesToWrite, 3000, out _);
            Debug.WriteLineIf(Error != 0, Error.ToString());

            int ByteCount;
            byte[] Buffer = new byte[64];

            Error = Reader.Read(Buffer, 5, out ByteCount);
            Debug.WriteLineIf(Error != 0, Error.ToString());

            Buffer = Buffer.Take(ByteCount).ToArray();

            var Output = Encoding.ASCII.GetString(Buffer);

            Output = Output.Replace("\0", "");
            Output = Output.Replace("\r", "");

            return Output;

        }
    }
}
