// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pvr_UnitySDKAPI
{
    public enum PBS_SystemInfoEnum
    {
        ELECTRIC_QUANTITY,
        PUI_VERSION,
        EQUIPMENT_MODEL,
        EQUIPMENT_SN,
        CUSTOMER_SN,
        INTERNAL_STORAGE_SPACE_OF_THE_DEVICE,
        DEVICE_BLUETOOTH_STATUS,
        BLUETOOTH_NAME_CONNECTED,
        BLUETOOTH_MAC_ADDRESS,
        DEVICE_WIFI_STATUS,
        WIFI_NAME_CONNECTED,
        WLAN_MAC_ADDRESS,
        DEVICE_IP
    }
    public enum PBS_DeviceControlEnum
    {
        DEVICE_CONTROL_REBOOT,
        DEVICE_CONTROL_SHUTDOWN
    }
    public enum PBS_PackageControlEnum
    {
        PACKAGE_SILENCE_INSTALL,
        PACKAGE_SILENCE_UNINSTALL
    }
    public enum PBS_SwitchEnum
    {
        S_ON,
        S_OFF
    }
    public enum PBS_HomeEventEnum
    {
        SINGLE_CLICK,
        DOUBLE_CLICK,
        LONG_PRESS
    }
    public enum PBS_HomeFunctionEnum
    {
        VALUE_HOME_GO_TO_SETTING,
        VALUE_HOME_BACK,
        VALUE_HOME_RECENTER,
        VALUE_HOME_OPEN_APP,
        VALUE_HOME_DISABLE,
        VALUE_HOME_GO_TO_HOME,
        VALUE_HOME_SEND_BROADCAST,
        VALUE_HOME_CLEAN_MEMORY
    }
    public enum PBS_ScreenOffDelayTimeEnum
    {
        THREE = 3,
        TEN = 10,
        THIRTY = 30,
        SIXTY = 60,
        THREE_HUNDRED = 300,
        SIX_HUNDRED = 600,
        NEVER = -1
    }
    public enum PBS_SleepDelayTimeEnum
    {
        FIFTEEN = 15,
        THIRTY = 30,
        SIXTY = 60,
        THREE_HUNDRED = 300,
        SIX_HUNDRED = 600,
        ONE_THOUSAND_AND_EIGHT_HUNDRED = 1800,
        NEVER = -1
    }
    public enum PBS_SystemFunctionSwitchEnum
    {
        SFS_USB,
        SFS_AUTOSLEEP,
        SFS_SCREENON_CHARGING,
        SFS_OTG_CHARGING,
        SFS_RETURN_MENU_IN_2DMODE,
        SFS_COMBINATION_KEY,
        SFS_CALIBRATION_WITH_POWER_ON,
        SFS_SYSTEM_UPDATE,
        SFS_CAST_SERVICE,
        SFS_EYE_PROTECTION,
        SFS_SECURITY_ZONE_PERMANENTLY,
        SFS_GLOBAL_CALIBRATION,
        SFS_Auto_Calibration,
        SFS_USB_BOOT
    }
    public enum PBS_USBConfigModeEnum
    {
        MTP,
        CHARGE
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ToBService
    {
        public static Action<bool> BoolCallback;
        public static Action<int> IntCallback;
        public static Action<long> LongCallback;
#if ANDROID_DEVICE
        private static AndroidJavaClass unityPlayer;
        private static AndroidJavaObject currentActivity;
        private static AndroidJavaObject tobHelper;
        private static AndroidJavaClass tobHelperClass;
#endif
        #region Public Function
        public static void UPvr_InitToBService()
        {
#if ANDROID_DEVICE
            tobHelperClass = new AndroidJavaClass("com.pvr.tobservice.ToBServiceHelper");
            tobHelper = tobHelperClass.CallStatic<AndroidJavaObject>("getInstance");
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
        }
        public static void UPvr_SetUnityObjectName(string obj)
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "setUnityObjectName", obj);
#endif
        }
        public static void UPvr_BindToBService()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "bindTobService", currentActivity);
#endif
        }
        public static void UPvr_UnBindToBService()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "unBindTobService", currentActivity);
#endif
        }
        private static AndroidJavaObject GetEnumType(Enum enumType)
        {
            AndroidJavaClass enumjs = new AndroidJavaClass("com.pvr.tobservice.enums" + enumType.GetType().ToString().Replace("Pvr_UnitySDKAPI", ""));
            AndroidJavaObject enumjo = enumjs.GetStatic<AndroidJavaObject>(enumType.ToString());
            return enumjo;
        }

        public static string UPvr_StateGetDeviceInfo(PBS_SystemInfoEnum type)
        {
            string result = "";
#if ANDROID_DEVICE
            System.UPvr_CallMethod<string>(ref result, tobHelper, "pbsStateGetDeviceInfo", GetEnumType(type), 0);
#endif
            return result;
        }

        public static void UPvr_ControlSetDeviceAction(PBS_DeviceControlEnum deviceControl, Action<int> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) IntCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsControlSetDeviceAction", GetEnumType(deviceControl), null);
#endif
        }

        public static void UPvr_ControlAPPManger(PBS_PackageControlEnum packageControl, string path, Action<int> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) IntCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsControlAPPManger", GetEnumType(packageControl), path, 0, null);
#endif
        }

        public static void UPvr_ControlSetAutoConnectWIFI(string ssid, string pwd, Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsControlSetAutoConnectWIFI", ssid, pwd, 0, null);
#endif
        }

        public static void UPvr_ControlClearAutoConnectWIFI(Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsControlClearAutoConnectWIFI", null);
#endif
        }

        public static void UPvr_PropertySetHomeKey(PBS_HomeEventEnum eventEnum, PBS_HomeFunctionEnum function, Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsPropertySetHomeKey", GetEnumType(eventEnum), GetEnumType(function), null);
#endif
        }

        public static void UPvr_PropertySetHomeKeyAll(PBS_HomeEventEnum eventEnum, PBS_HomeFunctionEnum function, int timesetup, string pkg, string className, Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsPropertySetHomeKeyAll", GetEnumType(eventEnum), GetEnumType(function), timesetup, pkg, className, null);
#endif
        }

        public static void UPvr_PropertyDisablePowerKey(bool isSingleTap, bool enable, Action<int> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) IntCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsPropertyDisablePowerKey", isSingleTap, enable, null);
#endif
        }

        public static void UPvr_PropertySetScreenOffDelay(PBS_ScreenOffDelayTimeEnum timeEnum, Action<int> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) IntCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsPropertySetScreenOffDelay", GetEnumType(timeEnum), null);
#endif
        }

        public static void UPvr_PropertySetSleepDelay(PBS_SleepDelayTimeEnum timeEnum)
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsPropertySetSleepDelay", GetEnumType(timeEnum));
#endif
        }

        public static void UPvr_SwitchSystemFunction(PBS_SystemFunctionSwitchEnum systemFunction, PBS_SwitchEnum switchEnum)
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsSwitchSystemFunction", GetEnumType(systemFunction), GetEnumType(switchEnum), 0);
#endif
        }

        public static void UPvr_SwitchSetUsbConfigurationOption(PBS_USBConfigModeEnum uSBConfigModeEnum)
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsSwitchSetUsbConfigurationOption", GetEnumType(uSBConfigModeEnum), 0);
#endif
        }

        public static void UPvr_ScreenOn()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsScreenOn");
#endif
        }

        public static void UPvr_ScreenOff()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsScreenOff");
#endif
        }

        public static void UPvr_AcquireWakeLock()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsAcquireWakeLock");
#endif
        }

        public static void UPvr_ReleaseWakeLock()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsReleaseWakeLock");
#endif
        }

        public static void UPvr_EnableEnterKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsEnableEnterKey");
#endif
        }

        public static void UPvr_DisableEnterKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsDisableEnterKey");
#endif
        }

        public static void UPvr_EnableVolumeKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsEnableVolumeKey");
#endif
        }

        public static void UPvr_DisableVolumeKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsDisableVolumeKey");
#endif
        }

        public static void UPvr_EnableBackKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsEnableBackKey");
#endif
        }

        public static void UPvr_DisableBackKey()
        {
#if ANDROID_DEVICE
            System.UPvr_CallMethod(tobHelper, "pbsDisableBackKey");
#endif
        }

        public static void UPvr_WriteConfigFileToDataLocal(string path, string content, Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsWriteConfigFileToDataLocal", path, content, null);
#endif
        }

        public static void UPvr_ResetAllKeyToDefault(Action<bool> callback)
        {
#if ANDROID_DEVICE
            if (callback != null) BoolCallback = callback;
            System.UPvr_CallMethod(tobHelper, "pbsResetAllKeyToDefault", null);
#endif
        }
        #endregion
    }
}