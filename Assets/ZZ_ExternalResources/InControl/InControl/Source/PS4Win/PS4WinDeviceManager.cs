#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DS4Windows;

namespace InControl
{
    public class PS4WinDeviceManager : InputDeviceManager
    {
        float deviceRefreshTimer = 0.0f;
        const float deviceRefreshInterval = 1.0f;

        List<UnityInputDeviceProfile> deviceProfiles = new List<UnityInputDeviceProfile>();
        string joystickHash = "";

        public PS4WinDeviceManager()
        {
            DS4Devices.findControllers();
            AutoDiscoverDeviceProfiles();
            RefreshDevices();
        }


        public override void Update(ulong updateTick, float deltaTime)
        {
            deviceRefreshTimer += deltaTime;
            if (string.IsNullOrEmpty(joystickHash) || deviceRefreshTimer >= deviceRefreshInterval)
            {
                deviceRefreshTimer = 0.0f;

                if (joystickHash != JoystickHash)
                {
                    Logger.LogInfo("Change in Unity attached joysticks detected; refreshing device list.");
                    RefreshDevices();
                }
            }
        }

        void RefreshDevices()
        {
            DetectAttachedJoystickDevices();
            DetectDetachedJoystickDevices();
            joystickHash = JoystickHash;
        }

        void AttachDevice(UnityInputDevice device)
        {
            devices.Add(device);
            InputManager.AttachDevice(device);
        }

        void DetectAttachedJoystickDevices()
        {
            try
            {
                var joystickNames = Input.GetJoystickNames();
                for (int i = 0; i < joystickNames.Length; i++)
                {
                    DetectAttachedJoystickDevice(i + 1, joystickNames[i]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
        }

        void DetectAttachedJoystickDevice(int unityJoystickId, string unityJoystickName)
        {
            if (unityJoystickName != "Wireless Controller") //This is the joystick name for ps4 controller on windows
            {
                return;
            }       

            var matchedDeviceProfile = deviceProfiles.Find(config => config.HasJoystickName(unityJoystickName));

            if (matchedDeviceProfile == null)
            {
                matchedDeviceProfile = deviceProfiles.Find(config => config.HasLastResortRegex(unityJoystickName));
            }

            UnityInputDeviceProfile deviceProfile = null;

            if (matchedDeviceProfile == null)
            {
                return;
            }
            else
            {
                deviceProfile = matchedDeviceProfile;
            }

            int deviceCount = devices.Count;
            for (int i = 0; i < deviceCount; i++)
            {
                var device = devices[i];
                var unityDevice = device as UnityInputDevice;
                if (unityDevice != null && unityDevice.IsConfiguredWith(deviceProfile, unityJoystickId))
                {
                    Logger.LogInfo("Device \"" + unityJoystickName + "\" is already configured with " + deviceProfile.Name);
                    return;
                }
            }

            var joystickDevice = new PS4WinDevice(deviceProfile, unityJoystickId);
            joystickDevice.Device = DS4Devices.getNextDS4Controller(); //TODO: wrong, nothing ensures that order is the same so we could end rumbling the other player controller
            AttachDevice(joystickDevice);

            if (matchedDeviceProfile == null)
            {
                Logger.LogWarning("Device " + unityJoystickId + " with name \"" + unityJoystickName + "\" does not match any known profiles.");
            }
            else
            {
                Logger.LogInfo("Device " + unityJoystickId + " matched profile " + deviceProfile.GetType().Name + " (" + deviceProfile.Name + ")");
            }
        }

        void DetectDetachedJoystickDevices()
        {
            var joystickNames = Input.GetJoystickNames();

            for (int i = devices.Count - 1; i >= 0; i--)
            {
                var inputDevice = devices[i] as PS4WinDevice;

                if (inputDevice.Profile.IsNotJoystick)
                {
                    continue;
                }

                if (joystickNames.Length < inputDevice.JoystickId ||
                    !inputDevice.Profile.HasJoystickOrRegexName(joystickNames[inputDevice.JoystickId - 1]))
                {
                    devices.Remove(inputDevice);
                    InputManager.DetachDevice(inputDevice);

                    Logger.LogInfo("Detached device: " + inputDevice.Profile.Name);
                }
            }
        }


        void AutoDiscoverDeviceProfiles()
        {
            var deviceProfile = (PlayStation4WinProfile)Activator.CreateInstance(typeof(PlayStation4WinProfile));
            if (deviceProfile.IsSupportedOnThisPlatform)
            {
                // Logger.LogInfo( "Found profile: " + deviceProfile.GetType().Name + " (" + deviceProfile.Name + ")" );
                deviceProfiles.Add(deviceProfile);
            }
        }

        static string JoystickHash
        {
            get
            {
                var joystickNames = Input.GetJoystickNames();
                return joystickNames.Length + ": " + String.Join(", ", joystickNames);
            }
        }


        public static bool CheckPlatformSupport(ICollection<string> errors)
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                return false;
            }

            /*try
            {
                GamePad.GetState(PlayerIndex.One);
            }
            catch (DllNotFoundException e)
            {
                if (errors != null)
                {
                    errors.Add(e.Message + ".dll could not be found or is missing a dependency.");
                }
                return false;
            }*/

            return true;
        }


        public static void Enable()
        {
            var errors = new List<string>();
            if (PS4WinDeviceManager.CheckPlatformSupport(errors))
            {
                InputManager.HideDevicesWithProfile(typeof(PlayStation4WinProfile));
                InputManager.AddDeviceManager<PS4WinDeviceManager>();
            }
            else
            {
                foreach (var error in errors)
                {
                    Logger.LogError(error);
                }
            }
        }
    }
}
#endif
