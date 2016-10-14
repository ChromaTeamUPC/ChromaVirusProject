#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;

namespace DS4Windows
{
    public struct DS4Color
    {
        public byte red;
        public byte green;
        public byte blue;

        public DS4Color(byte r, byte g, byte b)
        {
            red = r;
            green = g;
            blue = b;
        }

        public override bool Equals(object obj)
        {
            if (obj is DS4Color)
            {
                DS4Color dsc = ((DS4Color)obj);
                return (this.red == dsc.red && this.green == dsc.green && this.blue == dsc.blue);
            }
            else
                return false;
        }
    }

    public enum ConnectionType : byte { BT, USB }; // Prioritize Bluetooth when both are connected.

    public struct DS4HapticState
    {
        public DS4Color LightBarColor;
        public byte RumbleMotorStrengthLeftHeavySlow, RumbleMotorStrengthRightLightFast;
    }

    public class DS4Device
    {
        private const int BT_OUTPUT_REPORT_LENGTH = 78;
        private HidDevice hDevice;
        private string Mac;
        private ConnectionType conType;
        private byte[] outputReportBuffer, outputReport;
        public DS4HapticState hapticState;

        public event EventHandler<EventArgs> Removal = null;

        public HidDevice HidDevice { get { return hDevice; } }
        public string MacAddress { get { return Mac; } }

        public bool IsDisconnecting { get; private set; }

        public int IdleTimeout { get; set; } // behavior only active when > 0
 

        public static ConnectionType HidConnectionType(HidDevice hidDevice)
        {
            return hidDevice.Capabilities.InputReportByteLength == 64 ? ConnectionType.USB : ConnectionType.BT;
        }

        public DS4Device(HidDevice hidDevice)
        {
            hDevice = hidDevice;
            conType = HidConnectionType(hDevice);
            Mac = hDevice.readSerial();
            if (conType == ConnectionType.USB)
            {
                outputReport = new byte[hDevice.Capabilities.OutputReportByteLength];
                outputReportBuffer = new byte[hDevice.Capabilities.OutputReportByteLength];
            }
            else
            {
                outputReport = new byte[BT_OUTPUT_REPORT_LENGTH];
                outputReportBuffer = new byte[BT_OUTPUT_REPORT_LENGTH];
            }
        }

        public void Update()
        {
            SetOutput();
        }

        private bool writeOutput()
        {
            if (conType == ConnectionType.BT)
            {
                return hDevice.WriteOutputReportViaControl(outputReport);
            }
            else
            {
                return hDevice.WriteOutputReportViaInterrupt(outputReport, 8);
            }
        }
      
        public void FlushHID()
        {
            hDevice.flush_Queue();
        }


        private void SetOutput()
        {
            if (conType == ConnectionType.BT)
            {
                outputReportBuffer[0] = 0x11;
                outputReportBuffer[1] = 0x80;
                outputReportBuffer[3] = 0xff;
                outputReportBuffer[6] = hapticState.RumbleMotorStrengthRightLightFast; //fast motor
                outputReportBuffer[7] = hapticState.RumbleMotorStrengthLeftHeavySlow; //slow motor
                outputReportBuffer[8] = hapticState.LightBarColor.red; //red
                outputReportBuffer[9] = hapticState.LightBarColor.green; //green
                outputReportBuffer[10] = hapticState.LightBarColor.blue; //blue
                outputReportBuffer[11] = 0; //flash on duration
                outputReportBuffer[12] = 0; //flash off duration
            }
            else
            {
                outputReportBuffer[0] = 0x05;
                outputReportBuffer[1] = 0xff;
                outputReportBuffer[4] = hapticState.RumbleMotorStrengthRightLightFast; //fast motor
                outputReportBuffer[5] = hapticState.RumbleMotorStrengthLeftHeavySlow; //slow  motor
                outputReportBuffer[6] = hapticState.LightBarColor.red; //red
                outputReportBuffer[7] = hapticState.LightBarColor.green; //green
                outputReportBuffer[8] = hapticState.LightBarColor.blue; //blue
                outputReportBuffer[9] = 0; //flash on duration
                outputReportBuffer[10] = 0; //flash off duration
            }

            bool output = false;
            for (int i = 0; !output && i < outputReport.Length; i++)
                output = outputReport[i] != outputReportBuffer[i];

            if (true)
            {
                outputReportBuffer.CopyTo(outputReport, 0);
                writeOutput();
            }          
        }

        override
        public String ToString()
        {
            return Mac;
        }
    }
}
#endif