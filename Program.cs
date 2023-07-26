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
        public static DateTime LastDataEventDate = DateTime.Now;
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder;

        #endregion

        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                var BoschDevice = UsbDevice.AllDevices.Where(i => i.Name.Contains("Bosch")).First();
                MyUsbFinder = new UsbDeviceFinder(BoschDevice.Vid, BoschDevice.Pid);

                 BoschDevice.Open(out MyUsbDevice);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");


                // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                // it exposes an IUsbDevice interface. If not (WinUSB) the 
                // 'wholeUsbDevice' variable will be null indicating this is 
                // an interface of a device; it does not require or support 
                // configuration and interface selection.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                // open read endpoint 1.
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // open write endpoint 1.
                UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                // Remove the exepath/startup filename text from the begining of the CommandLine.
                string cmdLine = Regex.Replace(
                    Environment.CommandLine, "^\".+?\"^.*? |^.*? ", "", RegexOptions.Singleline);

                if (!String.IsNullOrEmpty(cmdLine))
                {
                    reader.DataReceived += (OnRxEndPointData);
                    reader.DataReceivedEnabled = true;





                    ////Brute force loop
                    //int maxLength = 5; // Maximum length of the string to brute force
                    //char[] charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
                    //// Add any other characters you want to include in the charset

                    //// Loop through all possible string combinations
                    //for (int length = 1; length <= maxLength; length++)
                    //{
                    //    char[] currentString = new char[length];

                    //    // Initialize the currentString with the first character in the charset
                    //    for (int i = 0; i < length; i++)
                    //    {
                    //        currentString[i] = charset[0];
                    //    }

                    //    while (true)
                    //    {
                    //        // Process the currentString (e.g., print, compare, etc.)
                    //        string resultString = new string(currentString);
                    //        Console.WriteLine(resultString);



                    //        int bytesWritten;
                    //        ec = writer.Write(Encoding.Default.GetBytes(resultString), 2000, out bytesWritten);
                    //        if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);

                    //        LastDataEventDate = DateTime.Now;
                    //        while ((DateTime.Now - LastDataEventDate).TotalMilliseconds < 100)
                    //        {
                    //        }

                    //        // Increment the currentString to the next combination
                    //        int index = length - 1;
                    //        while (index >= 0 && currentString[index] == charset[charset.Length - 1])
                    //        {
                    //            currentString[index] = charset[0];
                    //            index--;
                    //        }

                    //        if (index < 0)
                    //        {
                    //            // The loop has reached the last combination for the current length
                    //            break;
                    //        }

                    //        // Move to the next combination
                    //        currentString[index] = charset[Array.IndexOf(charset, currentString[index]) + 1];
                    //    }
                    //}

                    while (true)
                    {
                        // Size of the packet in bytes (4KB)
                        int packetSize = 4096;

                        // Create a byte array to hold the packet data
                        byte[] packet = new byte[packetSize];

                        // Fill the packet array with sample data (you can replace this with your actual data)
                        for (int i = 0; i < packetSize; i++)
                        {
                            // Filling the packet with some arbitrary data (e.g., incrementing values)
                            packet[i] = (byte)(i % 256); // Modulo 256 to ensure values are within 0-255 range
                        }


                        int bytesWritten;
                        writer.Write(packet, 2000, out bytesWritten);



                        //ec = writer.Write(Encoding.Default.GetBytes(Console.ReadLine()), 2000, out bytesWritten);
                        if (ec != ErrorCode.None) throw new Exception(UsbDevice.LastErrorString);
                    }









                    // Always disable and unhook event when done.
                    reader.DataReceivedEnabled = false;
                    reader.DataReceived -= (OnRxEndPointData);

                    Console.WriteLine("\r\nDone!\r\n");
                }
                else
                    throw new Exception("Nothing to do.");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }
                        MyUsbDevice.Close();
                    }
                }
                MyUsbDevice = null;

                // Free usb resources
                UsbDevice.Exit();

                // Wait for user input..
                Console.ReadKey();
            }
        }

        private static void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {
            LastDataEventDate = DateTime.Now;
            Console.Write(Encoding.Default.GetString(e.Buffer, 0, e.Count));
        }
    }
}
