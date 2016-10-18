#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace DS4Windows
{
    public class HidDevice : IDisposable
    {
        public enum ReadStatus
        {
            Success = 0,
            WaitTimedOut = 1,
            WaitFail = 2,
            NoDataRead = 3,
            ReadError = 4,
            NotConnected = 5
        }

        private readonly string _description;
        private readonly string _devicePath;
        private readonly HidDeviceAttributes _deviceAttributes;

        private readonly HidDeviceCapabilities _deviceCapabilities;
        private bool _monitorDeviceEvents;
        private string serial = null;
        internal HidDevice(string devicePath, string description = null)
        {
            _devicePath = devicePath;
            _description = description;

            try
            {
                var hidHandle = OpenSafeHandle(_devicePath, false);

                _deviceAttributes = GetDeviceAttributes(hidHandle);
                _deviceCapabilities = GetDeviceCapabilities(hidHandle);

                hidHandle.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw new Exception(string.Format("Error querying HID device '{0}'.", devicePath), exception);
            }
        }

        public SafeFileHandle safeReadHandle { get; private set; }

        public FileStream fileStream { get; private set; }
        public bool IsOpen { get; private set; }
        public bool IsExclusive { get; private set; }
        public bool IsConnected { get { return HidDevices.IsConnected(_devicePath); } }
        public string Description { get { return _description; } }
        public HidDeviceCapabilities Capabilities { get { return _deviceCapabilities; } }
        public HidDeviceAttributes Attributes { get { return _deviceAttributes; } }
        public string DevicePath { get { return _devicePath; } }

        public override string ToString()
        {
            return string.Format("VendorID={0}, ProductID={1}, Version={2}, DevicePath={3}",
                                _deviceAttributes.VendorHexId,
                                _deviceAttributes.ProductHexId,
                                _deviceAttributes.Version,
                                _devicePath);
        }

        public void OpenDevice(bool isExclusive)
        {
            if (IsOpen) return;
            try
            {
                if (safeReadHandle == null || safeReadHandle.IsInvalid)
                    safeReadHandle = OpenSafeHandle(_devicePath, isExclusive);
            }
            catch (Exception exception)
            {
                IsOpen = false;
                throw new Exception("Error opening HID device.", exception);
            }

            IsOpen = !safeReadHandle.IsInvalid;
            IsExclusive = isExclusive;
        }

        public void CloseDevice()
        {
            if (!IsOpen) return;
            closeFileStreamIO();

            IsOpen = false;
        }

        public void Dispose()
        {
            CancelIO();
            CloseDevice();
        }

        public void CancelIO()
        {
            if (IsOpen)
                NativeMethods.CancelIoEx(safeReadHandle.DangerousGetHandle(), IntPtr.Zero);
        }

        public bool ReadInputReport(byte[] data)
        {
            if (safeReadHandle == null)
                safeReadHandle = OpenSafeHandle(_devicePath, true);
            return NativeMethods.HidD_GetInputReport(safeReadHandle, data, data.Length);
        }


        private static HidDeviceAttributes GetDeviceAttributes(SafeFileHandle hidHandle)
        {
            var deviceAttributes = default(NativeMethods.HIDD_ATTRIBUTES);
            deviceAttributes.Size = Marshal.SizeOf(deviceAttributes);
            NativeMethods.HidD_GetAttributes(hidHandle.DangerousGetHandle(), ref deviceAttributes);
            return new HidDeviceAttributes(deviceAttributes);
        }

        private static HidDeviceCapabilities GetDeviceCapabilities(SafeFileHandle hidHandle)
        {
            var capabilities = default(NativeMethods.HIDP_CAPS);
            var preparsedDataPointer = default(IntPtr);

            if (NativeMethods.HidD_GetPreparsedData(hidHandle.DangerousGetHandle(), ref preparsedDataPointer))
            {
                NativeMethods.HidP_GetCaps(preparsedDataPointer, ref capabilities);
                NativeMethods.HidD_FreePreparsedData(preparsedDataPointer);
            }
            return new HidDeviceCapabilities(capabilities);
        }

        private void closeFileStreamIO()
        {
            if (fileStream != null)
                fileStream.Close();
            fileStream = null;
            Console.WriteLine("Close fs");
            if (safeReadHandle != null && !safeReadHandle.IsInvalid)
            {
                safeReadHandle.Close();
                Console.WriteLine("Close sh");

            }
            safeReadHandle = null;
        }

        public void flush_Queue()
        {
            if (safeReadHandle != null)
            {
                NativeMethods.HidD_FlushQueue(safeReadHandle);
            }
        }

        private ReadStatus ReadWithFileStreamTask(byte[] inputBuffer)
        {
            try
            {
                if (fileStream.Read(inputBuffer, 0, inputBuffer.Length) > 0)
                {
                    return ReadStatus.Success;
                }
                else
                {
                    return ReadStatus.NoDataRead;
                }
            }
            catch (Exception)
            {
                return ReadStatus.ReadError;
            }
        }

        public ReadStatus ReadFile(byte[] inputBuffer)
        {
            if (safeReadHandle == null)
                safeReadHandle = OpenSafeHandle(_devicePath, true);
            try
            {
                uint bytesRead;
                if (NativeMethods.ReadFile(safeReadHandle.DangerousGetHandle(), inputBuffer, (uint)inputBuffer.Length, out bytesRead, IntPtr.Zero))
                {
                    return ReadStatus.Success;
                }
                else
                {
                    return ReadStatus.NoDataRead;
                }
            }
            catch (Exception)
            {
                return ReadStatus.ReadError;
            }
        }

        public bool WriteOutputReportViaControl(byte[] outputBuffer)
        {
            if (safeReadHandle == null)
            {
                safeReadHandle = OpenSafeHandle(_devicePath, true);
            }

            if (NativeMethods.HidD_SetOutputReport(safeReadHandle, outputBuffer, outputBuffer.Length))
                return true;
            else
                return false;
        }

        private bool WriteOutputReportViaInterruptTask(byte[] outputBuffer)
        {
            try
            {
                fileStream.Write(outputBuffer, 0, outputBuffer.Length);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool WriteOutputReportViaInterrupt(byte[] outputBuffer, int timeout)
        {
            uint bytesWritten;
            System.Threading.NativeOverlapped aux = new System.Threading.NativeOverlapped();
            try
            {
                if (safeReadHandle == null)
                {
                    safeReadHandle = OpenSafeHandle(_devicePath, false);
                }
                if (fileStream == null && !safeReadHandle.IsInvalid)
                {
                    IntPtr handle = safeReadHandle.DangerousGetHandle();
                    NativeMethods.WriteFile(handle, outputBuffer, (uint)outputBuffer.Length, out bytesWritten, ref aux);
                }
            }
            catch (Exception ex)
            {
                if(ex != null)
                    Debug.Log(ex.Message);
                return false;
            }

            return true;
        }

        private SafeFileHandle OpenSafeHandle(String devicePathName, Boolean isExclusive)
        {
            SafeFileHandle hidHandle;
            isExclusive = false;

            try
            {
                if (isExclusive)
                {
                    hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, 0, IntPtr.Zero, NativeMethods.OpenExisting, 0, 0);
                }
                else
                {
                    hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, NativeMethods.OpenExisting, 0, 0);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return hidHandle;
        }

        private IntPtr OpenHandle(String devicePathName, Boolean isExclusive)
        {
            IntPtr hidHandle;
            NativeMethods.SECURITY_ATTRIBUTES secAttribs = new NativeMethods.SECURITY_ATTRIBUTES();

            try
            {
                if (isExclusive)
                {
                    hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, ref secAttribs, NativeMethods.OpenExisting, 0, 0);
                }
                else
                {
                    hidHandle = NativeMethods.CreateFile(devicePathName, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE, NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE, ref secAttribs, NativeMethods.OpenExisting, 0, 0);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return hidHandle;
        }

        public bool readFeatureData(byte[] inputBuffer)
        {
            return NativeMethods.HidD_GetFeature(safeReadHandle.DangerousGetHandle(), inputBuffer, inputBuffer.Length);
        }

        public string readSerial()
        {
            if (serial != null)
                return serial;

            if (Capabilities.InputReportByteLength == 64)
            {
                byte[] buffer = new byte[16];
                buffer[0] = 18;
                readFeatureData(buffer);
                serial = String.Format("{0:X02}:{1:X02}:{2:X02}:{3:X02}:{4:X02}:{5:X02}", buffer[6], buffer[5], buffer[4], buffer[3], buffer[2], buffer[1]);
                return serial;
            }
            else
            {
                byte[] buffer = new byte[126];
                NativeMethods.HidD_GetSerialNumberString(safeReadHandle.DangerousGetHandle(), buffer, (ulong)buffer.Length);
                string MACAddr = System.Text.Encoding.Unicode.GetString(buffer).Replace("\0", string.Empty).ToUpper();
                //MACAddr = $"{MACAddr[0]}{MACAddr[1]}:{MACAddr[2]}{MACAddr[3]}:{MACAddr[4]}{MACAddr[5]}:{MACAddr[6]}{MACAddr[7]}:{MACAddr[8]}{MACAddr[9]}:{MACAddr[10]}{MACAddr[11]}";
                serial = MACAddr;
                return serial;
            }
        }
    }
}
#endif