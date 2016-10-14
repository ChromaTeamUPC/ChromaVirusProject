#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InControl
{
    public class PS4WinDevice : UnityInputDevice
    {
        private DS4Windows.DS4Device device;

        public DS4Windows.DS4Device Device { set { device = value; } }

        public PS4WinDevice(UnityInputDeviceProfile profile, int joystickId)
			: base(profile, joystickId)
		{
        }

        public override void Update(ulong updateTick, float deltaTime)
        {
            base.Update(updateTick, deltaTime);

            device.Update();
        }

        public override void Vibrate(float leftMotor, float rightMotor) //strong, weak
        {
            if (device != null)
            {
                device.hapticState.RumbleMotorStrengthLeftHeavySlow = (byte)(leftMotor * 255);
                device.hapticState.RumbleMotorStrengthRightLightFast = (byte)(rightMotor * 255);
            }
        }

        public override void Iluminate(byte red, byte green, byte blue)
        {
            if (device != null)
            {
                device.hapticState.LightBarColor.red = red;
                device.hapticState.LightBarColor.green = green;
                device.hapticState.LightBarColor.blue = blue;
            }
        }
    }
}
#endif

