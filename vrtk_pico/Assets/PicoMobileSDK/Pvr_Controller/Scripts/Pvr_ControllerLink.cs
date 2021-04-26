// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using Pvr_UnitySDKAPI;
using UnityEngine;

public class Pvr_ControllerLink
{

#if ANDROID_DEVICE
    public AndroidJavaClass javaHummingbirdClass;
    public AndroidJavaClass javaPico2ReceiverClass;
    public AndroidJavaClass javaserviceClass;
    public AndroidJavaClass javavractivityclass;
    public AndroidJavaClass javaCVClass;
    public AndroidJavaObject activity;
#endif
    public string gameobjname = "";
    public bool picoDevice = false;
    public string hummingBirdMac;
    public int hummingBirdRSSI;
    public bool goblinserviceStarted = false;
    public bool neoserviceStarted = false;
    public bool controller0Connected = false;
    public bool controller1Connected = false;
    public int mainHandID = 0;
    public Pvr_Controller.UserHandNess handness = Pvr_Controller.UserHandNess.Right;
    public int controllerType = 0;
    public ControllerHand Controller0;
    public ControllerHand Controller1;
    public int platFormType = -1; //0 phone，1 Pico Neo DK，2 Pico Goblin 3 Pico Neo
    public int trackingmode = -1; //0:null,1:3dof,2:cv 3:cv+hb 4:cv2 5:cv2+hb
    public int systemProp = -1;   //0：goblin1 1：goblin1 2:neo 3:goblin2 4:neo2
    public int enablehand6dofbyhead = -1;
    public bool switchHomeKey = true;
    private int iPhoneHMDModeEnabled = 0;

    public Pvr_ControllerLink(string name)
    {
        gameobjname = name;
        hummingBirdMac = "";
        hummingBirdRSSI = 0;
        Debug.Log("PvrLog Controller GameObject:" +gameobjname);
        StartHummingBirdService();
        Controller0 = new ControllerHand();
        Controller0.Position = new Vector3(0, Pvr_UnitySDKSensor.Instance.HeadPose.Position.y, 0)  + new Vector3(-0.1f, -0.3f, 0.3f);
        Controller1 = new ControllerHand();
        Controller1.Position = new Vector3(0, Pvr_UnitySDKSensor.Instance.HeadPose.Position.y, 0) + new Vector3(0.1f, -0.3f, 0.3f);
    }

    private void StartHummingBirdService()
    {
#if ANDROID_DEVICE
        try
        {
            UnityEngine.AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            javaHummingbirdClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientActivity");
            javaCVClass = new AndroidJavaClass("com.picovr.picovrlib.cvcontrollerclient.ControllerClient");
            javavractivityclass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaserviceClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.UnityClient");
            Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaHummingbirdClass.GetRawClass());
            int enumindex = (int)GlobalIntConfigs.PLATFORM_TYPE;
            Render.UPvr_GetIntConfig(enumindex, ref platFormType);
            Debug.Log("PvrLog platform" + platFormType);
            enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
            Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
            Debug.Log("PvrLog trackingmode" + trackingmode);
            systemProp = GetSysproc();
            Debug.Log("PvrLog systemProp" + systemProp);
            enumindex = (int) GlobalIntConfigs.ENBLE_HAND6DOF_BY_HEAD;
            Render.UPvr_GetIntConfig(enumindex, ref enablehand6dofbyhead);
            Debug.Log("PvrLog enablehand6dofbyhead" + enablehand6dofbyhead);
            if (trackingmode == 0 || trackingmode == 1 || (trackingmode == 3 || trackingmode == 5 || trackingmode == 6) && (systemProp == 1 || systemProp == 3))
            {
                picoDevice = platFormType != 0;
                javaPico2ReceiverClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientReceiver");
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver", activity, gameobjname);
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setPlatformType", platFormType);
            }
            else
            {
                picoDevice = true;
                SetGameObjectToJar(gameobjname);
            }
            Render.UPvr_GetIntConfig((int)GlobalIntConfigs.iPhoneHMDModeEnabled, ref iPhoneHMDModeEnabled);
            if (iPhoneHMDModeEnabled == 1)
            {
                BindService();
            }
            else
            {
                if (IsServiceExisted())
                {
                    BindService();
                }
            }
        }
        catch (AndroidJavaException e)
        {
            PLOG.E("ConnectToAndriod--catch" + e.Message);
        }
#endif
    }

    public bool IsServiceExisted()
    {
        bool service = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref service, javaserviceClass, "isServiceExisted", activity,trackingmode);
#endif
        Debug.Log("PvrLog ServiceExisted ?" + service);
        return service;
    }

    public void SetGameObjectToJar(string name)
    {
        Debug.Log("PvrLog SetGameObjectToJar " + name);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setGameObjectCallback", name);
#endif
    }

    public void BindService()
    {
        Debug.Log("PvrLog Bind Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "bindService", activity,trackingmode);
#endif
    }

    public void UnBindService()
    {
        Debug.Log("PvrLog UnBind Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "unbindService", activity,trackingmode);
#endif
    }

    public void StopLark2Receiver()
    {
        Debug.Log("PvrLog StopLark2Receiver");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver",activity);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopOnBootReceiver",activity);
#endif
    }

    public void StartLark2Receiver()
    {
        Debug.Log("PvrLog StartLark2Receiver");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
#endif
    }

    public void StopLark2Service()
    {
        Debug.Log("PvrLog StopLark2Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver", activity); 
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "unbindHbService", activity);
#endif
    }

    public void StartLark2Service()
    {
        Debug.Log("PvrLog StartLark2Service");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "bindHbService", activity);
#endif
    }

    public int getHandness()
    {
        int handness = -1;
#if ANDROID_DEVICE
        if (iPhoneHMDModeEnabled == 0)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref handness, javavractivityclass, "getPvrHandness", activity);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref handness, javaHummingbirdClass, "getHbHandednessInSP");
        }
#endif
        PLOG.I("PvrLog GetHandness =" + handness);
        return handness;
    }

    public void setHandness(int hand)
    {
        PLOG.I("PvrLog SetHandness =" + hand);
#if ANDROID_DEVICE
        if (iPhoneHMDModeEnabled == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setHbHandednessInSP", hand);
        }
#endif
    }

    public void StartScan()
    {
        PLOG.I("PvrLog ScanHBController");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", true);
#endif
    }

    public void StopScan()
    {
        PLOG.I("PvrLog StopScanHBController");
        if (iPhoneHMDModeEnabled == 0)
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", false);
#endif
        }
    }

    public int GetSysproc()
    {
        int prop = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref prop, javaserviceClass, "getSysproc");
#endif
        PLOG.I("PvrLog GetSysproc" + prop);
        return prop;
    }

    public void ResetController(int num)
    {
        Debug.Log("PvrLog ResetController" + num);
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetControllerSensorState",num);
        }
        if(goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "resetHbSensorState");
		}
#endif
    }

    public void ConnectBLE()
    {
        Debug.Log("PvrLog ConnectHBController" + hummingBirdMac);
        if (hummingBirdMac != "")
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "connectHbController", hummingBirdMac);
#endif
        }
    }

    public void DisConnectBLE()
    {
        Debug.Log("PvrLog DisConnectHBController");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "disconnectHbController");
#endif
    }

    public bool StartUpgrade()
    {
        Debug.Log("PvrLog StartUpgradeHBController");
        bool start = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref start, javaHummingbirdClass, "startUpgrade");
#endif
        return start;
    }

    public void setBinPath(string path, bool isasset)
    {
        Debug.Log("PvrLog setBinPath" + path + isasset);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setBinPath",path,isasset);
#endif
    }

    public string GetBLEImageType()
    {
        string type = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getBLEImageType");
        }
#endif
        Debug.Log("PvrLog GetBLEImageType" + type);
        return type;
    }

    public long GetBLEVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getBLEVersion");
        }
#endif
        Debug.Log("PvrLog GetBLEVersion" + version.ToString());
        return version;
    }

    public string GetFileImageType()
    {
        string type = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getFileImageType");
        }
#endif
        Debug.Log("PvrLog GetFileImageType" + type);
        return type;
    }

    public long GetFileVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getFileVersion");
        }
#endif
        Debug.Log("PvrLog GetFileVersion" + version.ToString());
        return version;
    }

    public int GetControllerConnectionState(int num)
    {
        int state = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaCVClass, "getControllerConnectionState",num);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaHummingbirdClass, "getHbConnectionState");
        }
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GetControllerState:" + num + "state:" + state);
        }
        return state;
    }

    public void RebackToLauncher()
    {
        Debug.Log("PvrLog RebackToLauncher");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startLauncher");
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "startLauncher");
        }
#endif
    }

    public void TurnUpVolume()
    {
        Debug.Log("PvrLog TurnUpVolume");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnUpVolume", activity);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnUpVolume", activity);
        }
#endif
    }

    public void TurnDownVolume()
    {
        Debug.Log("PvrLog TurnDownVolume");
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnDownVolume", activity);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnDownVolume", activity);
        }
#endif
    }

    private float[] hbPoseData = new float[4];
    public float[] GetHBControllerPoseData()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref hbPoseData, javaHummingbirdClass, "getHBSensorPose");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog HBControllerData" + hbPoseData[0] + "," + hbPoseData[1] + "," + hbPoseData[2] + "," + hbPoseData[3]);
        }
        return hbPoseData;
    }

    private  float[] sensorData = new float[28];
    public float[] GetControllerSensorData()
    {
#if ANDROID_DEVICE
        if (enablehand6dofbyhead == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref sensorData, javaCVClass, "getControllerDataInfoBySharmem",Pvr_UnitySDKManager.SDK.headData);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref sensorData, javaCVClass, "getControllerDataInfoBySharmem");
        }
#endif

        return sensorData;
    }

    private int[] keyData = new int[134];
    public int[] GetControllerKeyData()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref keyData, javaCVClass, "getDoubleControllerKeyEventUnityExtBySharmem");
#endif
        return keyData;
    }

    private int[] neo3TouchValue = new int[15];
    public int[] GetNeo3TouchData(int hand)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref neo3TouchValue, javaCVClass, "getControllerTouchEvent", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog Neo3Touch hand:" + hand + "-" + neo3TouchValue[0] + neo3TouchValue[1] + neo3TouchValue[2] + neo3TouchValue[3] + neo3TouchValue[4] 
                                                        + "," + neo3TouchValue[5] + neo3TouchValue[6] + neo3TouchValue[7] + neo3TouchValue[8] + neo3TouchValue[9] 
                                                        + "," + neo3TouchValue[10] + neo3TouchValue[11] + neo3TouchValue[12] + neo3TouchValue[13] + neo3TouchValue[14]);
        }
        return neo3TouchValue;
    }

    public int GetNeo3GripValue(int hand)
    {
        int value = 0;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, javaCVClass, "getControllerGripValue", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog Neo3GripValue:" + value);
        }
        return value;
    }

    private float[] fixedState = new float[7] {0, 0, 0, 1, 0, 0, 0};
    public float[] GetControllerFixedSensorState(int hand)
    {
        if (trackingmode == 2 || trackingmode == 3)
        {
            return fixedState;
        }

#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref fixedState, javaCVClass, "getControllerFixedSensorState", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GetControllerFixedSensorState " + hand + "Rotation:" + fixedState[0] + "," + fixedState[1] + "," + fixedState[2] + "," + fixedState[3] + "Position:" +
                   fixedState[4] + "," + fixedState[5] + "," + fixedState[6]);
        }
        return fixedState;
    }

    private float[] neoposeData = new float[7] { 0, 0, 0, 1, 0, 0, 0 };
    public float[] GetCvControllerPoseData(int hand)
    {
#if ANDROID_DEVICE
        if (enablehand6dofbyhead == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref neoposeData, javaCVClass, "getControllerSensorState", hand,Pvr_UnitySDKManager.SDK.headData);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref neoposeData, javaCVClass, "getControllerSensorState", hand);
        }

#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog CVControllerData :" + neoposeData[0] + "," + neoposeData[1] + "," + neoposeData[2] + "," + neoposeData[3] + "," +
                   neoposeData[4] + "," + neoposeData[5] + "," + neoposeData[6]);
        }
        return neoposeData;
    }

    private int[] goblinKeyArray = new int[47];
    //touch.x,touch.y,home,app,touch click,volume up,volume down,trigger,power
    public int[] GetHBControllerKeyData()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref goblinKeyArray, javaHummingbirdClass, "getHBKeyEventUnityExt");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog HBControllerKey" + goblinKeyArray[0] + goblinKeyArray[1] + goblinKeyArray[2] + goblinKeyArray[3] + goblinKeyArray[4] + "," + goblinKeyArray[5] + goblinKeyArray[6] + goblinKeyArray[7] + goblinKeyArray[8] + goblinKeyArray[9] + ","
                   + goblinKeyArray[10] + goblinKeyArray[11] + goblinKeyArray[12] + goblinKeyArray[13] + goblinKeyArray[14] + "," + goblinKeyArray[15] + goblinKeyArray[16] + goblinKeyArray[17] + goblinKeyArray[18] + goblinKeyArray[19] + ","
                   + goblinKeyArray[20] + goblinKeyArray[21] + goblinKeyArray[22] + goblinKeyArray[23] + goblinKeyArray[24] + "," + goblinKeyArray[25] + goblinKeyArray[26] + goblinKeyArray[27] + goblinKeyArray[28] + goblinKeyArray[29] + ","
                   + goblinKeyArray[30] + goblinKeyArray[31] + goblinKeyArray[32] + goblinKeyArray[33] + goblinKeyArray[34] + "," + goblinKeyArray[35] + goblinKeyArray[36] + goblinKeyArray[37] + goblinKeyArray[38] + goblinKeyArray[39] + ","
                   + goblinKeyArray[40] + goblinKeyArray[41] + goblinKeyArray[42] + goblinKeyArray[43] + goblinKeyArray[44] + "," + goblinKeyArray[45] + goblinKeyArray[46]);
        }
        return goblinKeyArray;
    }

    public int GetHBKeyValue()
    {
        int key = -1;
#if ANDROID_DEVICE
     Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref key,javaHummingbirdClass, "getTriggerKeyEvent");
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog GoblinControllerTriggerKey:" + key);
        }
        return key;
    }

    private int[] neoKeyArray = new int[67];
    //touch.x,touch.y,home,app,touch click,volume up,volume down,trigger,power,X（A），Y（B），Left，Right
    public int[] GetCvControllerKeyData(int hand)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref neoKeyArray, javaCVClass, "getControllerKeyEventUnityExt", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog CVControllerKey hand:" + hand + "-" + neoKeyArray[0] + neoKeyArray[1] + neoKeyArray[2] + neoKeyArray[3] + neoKeyArray[4] + "," + neoKeyArray[5] + neoKeyArray[6] + neoKeyArray[7] + neoKeyArray[8] + neoKeyArray[9] + ","
                   + neoKeyArray[10] + neoKeyArray[11] + neoKeyArray[12] + neoKeyArray[13] + neoKeyArray[14] + "," + neoKeyArray[15] + neoKeyArray[16] + neoKeyArray[17] + neoKeyArray[18] + neoKeyArray[19] + ","
                   + neoKeyArray[20] + neoKeyArray[21] + neoKeyArray[22] + neoKeyArray[23] + neoKeyArray[24] + "," + neoKeyArray[25] + neoKeyArray[26] + neoKeyArray[27] + neoKeyArray[28] + neoKeyArray[29] + ","
                   + neoKeyArray[30] + neoKeyArray[31] + neoKeyArray[32] + neoKeyArray[33] + neoKeyArray[34] + "," + neoKeyArray[35] + neoKeyArray[36] + neoKeyArray[37] + neoKeyArray[38] + neoKeyArray[39] + ","
                   + neoKeyArray[40] + neoKeyArray[41] + neoKeyArray[42] + neoKeyArray[43] + neoKeyArray[44] + "," + neoKeyArray[45] + neoKeyArray[46] + neoKeyArray[47] + neoKeyArray[48] + neoKeyArray[49] + ","
                   + neoKeyArray[50] + neoKeyArray[51] + neoKeyArray[52] + neoKeyArray[53] + neoKeyArray[54] + "," + neoKeyArray[55] + neoKeyArray[56] + neoKeyArray[57] + neoKeyArray[58] + neoKeyArray[59] + ","
                   + neoKeyArray[60] + neoKeyArray[61] + neoKeyArray[62] + neoKeyArray[63] + neoKeyArray[64] + "," + neoKeyArray[65] + neoKeyArray[66]);
        }
        return neoKeyArray;
    }

    private int[] neotriggerV = new int[9];
    public int GetCVTriggerValue(int hand)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref neotriggerV, javaCVClass, "getControllerKeyEvent", hand);
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog CVTriggerValue " + neotriggerV[7]);
        }
        return neotriggerV[7];
    }

    public void AutoConnectHbController(int scanTimeMs)
    {
        PLOG.I("PvrLog AutoConnectHbController" + scanTimeMs);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "autoConnectHbController",scanTimeMs,gameobjname);
#endif
    }

    public void StartControllerThread(int headSensorState, int handSensorState)
    {
        if (BoundarySystem.UPvr_IsBoundaryEnable())
        {
            headSensorState = 1;
            handSensorState = 1;
        }
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerThread",headSensorState,handSensorState);
#endif
        Debug.Log("PvrLog StartControllerThread" + headSensorState + handSensorState);
    }
    public void StopControllerThread(int headSensorState, int handSensorState)
    {
        if (BoundarySystem.UPvr_IsBoundaryEnable())
        {
            headSensorState = 1;
            handSensorState = 1;
        }
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "stopControllerThread",headSensorState,handSensorState);
#endif
        Debug.Log("PvrLog StopControllerThread" + headSensorState + handSensorState);
    }

    public void SetUnityVersionToJar(string version)
    {
        if (trackingmode == 4 || trackingmode == 5 || trackingmode == 6)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setUnityVersion",version);
#endif
        }
        Debug.Log("PvrLog SetUnityVersionToJar" + version);
    }

    private float[] velocity = new float[3];
    public Vector3 GetVelocity(int num)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref velocity, javaCVClass, "getControllerLinearVelocity", num);
        }
#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog Velocity" + velocity[0] + "," + velocity[1] + "," + velocity[2]);
        }
        return new Vector3(velocity[0], velocity[1], -velocity[2]);
    }

    private float[] angularVelocity = new float[3];
    public Vector3 GetAngularVelocity(int num)
    {
#if ANDROID_DEVICE

        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angularVelocity, javaCVClass, "getControllerAngularVelocity", num);
        }
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angularVelocity, javaHummingbirdClass, "getHbAngularVelocity");
        }
#endif

        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog Gyro:" + angularVelocity[0] + "," + angularVelocity[1] + "," + angularVelocity[2]);
        }
        return new Vector3(0, 0, 0);
    }

    private float[] acceData = new float[3];
    public Vector3 GetAcceleration(int num)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref acceData, javaCVClass, "getControllerAcceleration", num);
        }
        if(goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref acceData, javaHummingbirdClass, "getHbAcceleration");
        }

#endif
        if (PLOG.logLevel > 2)
        {
            PLOG.D("PvrLog Acce:" + acceData[0] + acceData[1] + acceData[2]);
        }
        return new Vector3(0, 0, 0);
    }

    public string GetConnectedDeviceMac()
    {
        string mac = "";
#if ANDROID_DEVICE
        if (goblinserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref mac, javaHummingbirdClass, "getConnectedDeviceMac");
        }
#endif
        PLOG.I("PvrLog ConnectedDeviceMac:" + mac);
        return mac;
    }
  
    public void VibrateNeo2Controller(float strength, int time, int hand)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "vibrateCV2ControllerStrength",strength,time, hand);
        }
#endif
        PLOG.I("PvrLog VibrateNeo2Controller:" + strength + time + hand);
    }

    public int GetMainControllerIndex()
    {
        int index = 0;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index, javaCVClass, "getMainControllerIndex");
        }
#endif
        PLOG.I("PvrLog GetMainControllerIndex:" + index);
        return index;
    }

    public void SetMainController(int index)
    {
        PLOG.I("PvrLog SetMainController:" + index);
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setMainController",index); 
        }
#endif
    }
    public void ResetHeadSensorForController()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetHeadSensorForController");
        }
#endif
        PLOG.I("PvrLog ResetHeadSensorForController:");
    }

    public void GetDeviceVersion(int deviceType)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getDeviceVersion",deviceType); 
        }
#endif
        PLOG.I("PvrLog GetDeviceVersion:" + deviceType);
    }
 
    public void GetControllerSnCode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerSnCode",controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog GetControllerSnCode:" + controllerSerialNum);
    }
 
    public void SetControllerUnbind(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerUnbind",controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog SetControllerUnbind:" + controllerSerialNum);
    }

    public void SetStationRestart()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setStationRestart"); 
        }
#endif
        PLOG.I("PvrLog SetStationRestart");
    }

    public void StartStationOtaUpdate()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startStationOtaUpdate"); 
        }
#endif
        PLOG.I("PvrLog StartStationOtaUpdate");
    }
  
    public void StartControllerOtaUpdate(int mode, int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerOtaUpdate",mode,controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog StartControllerOtaUpdate" + mode + controllerSerialNum);
    }
    
    public void EnterPairMode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "enterPairMode",controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog EnterPairMode" + controllerSerialNum);
    }
   
    public void SetControllerShutdown(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerShutdown",controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog SetControllerShutdown" + controllerSerialNum);
    }
    
    public int GetStationPairState()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationPairState"); 
        }
#endif
        PLOG.I("PvrLog StationPairState" + index);
        return index;
    }
   
    public int GetStationOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationOtaUpdateProgress"); 
        }
#endif
        PLOG.I("PvrLog StationOtaUpdateProgress" + index);
        return index;
    }
    
    public int GetControllerOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerOtaUpdateProgress"); 
        }
#endif
        PLOG.I("PvrLog ControllerOtaUpdateProgress" + index);
        return index;
    }

    public void GetControllerVersionAndSN(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerVersionAndSN",controllerSerialNum); 
        }
#endif
        PLOG.I("PvrLog GetControllerVersionAndSN" + controllerSerialNum);
    }
    
    public void GetControllerUniqueID()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerUniqueID"); 
        }
#endif
        PLOG.I("PvrLog GetControllerUniqueID");
    }
    
    public void InterruptStationPairMode()
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "interruptStationPairMode"); 
        }
#endif
        PLOG.I("PvrLog InterruptStationPairMode");
    }

    public int GetControllerAbility(int controllerSerialNum)
    {
        int index = -1;
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerAbility",controllerSerialNum);
        }
#endif
        PLOG.I("PvrLog ControllerAbility:" + index);
        return index;
    }

    public void SwitchHomeKey(bool state)
    {
        PLOG.I("PvrLog SwitchHomeKey:" + state);
        switchHomeKey = state;
    }

    public void SetBootReconnect()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setBootReconnect");
#endif
        PLOG.I("PvrLog SetBootReconnect");
    }

    //Acquisition of equipment temperature
    public int GetTemperature()
    {
        int value = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value,javaHummingbirdClass, "getTemperature");
#endif
        PLOG.I("PvrLog Temperature:" + value);
        return value;
    }

    //Get the device type
    public int GetDeviceType()
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaHummingbirdClass, "getDeviceType");
#endif
        PLOG.I("PvrLog DeviceType:" + type);
        return type;
    }

    public int GetControllerType()
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaHummingbirdClass, "getControllerType");
#endif
        return type;
    }

    public string GetHummingBird2SN()
    {
        string type = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type,javaHummingbirdClass, "getHummingBird2SN");
#endif
        PLOG.I("PvrLog HummingBird2SN:" + type);
        return type;
    }

    public string GetControllerVersion()
    {
        string type = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type,javaHummingbirdClass, "getControllerVersion");
#endif
        PLOG.I("PvrLog ControllerVersion:" + type);
        return type;
    }

    public bool IsEnbleTrigger()
    {
        bool state = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref state,javaHummingbirdClass, "isEnbleTrigger");
#endif
        PLOG.I("PvrLog IsEnbleTrigger:" + state);
        return state;
    }

    //deviceType: 0：scan both controller；1：scan left controller；2：scan right controller
    public void StartCV2PairingMode(int devicetype)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startCV2PairingMode",devicetype); 
        }
#endif
        PLOG.I("PvrLog StartCV2PairingMode:" + devicetype);
    }

    public void StopCV2PairingMode(int devicetype)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "stopCV2PairingMode",devicetype); 
        }
#endif
        PLOG.I("PvrLog StopCV2PairingMode:" + devicetype);
    }

    public int GetControllerBindingState(int id)
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaCVClass, "getControllerBindingState",id);
#endif
        PLOG.I("PvrLog getControllerBindingState:" + type);
        return type;
    }
    public void setIsEnbleHomeKey(bool state)
    {
#if ANDROID_DEVICE
        if (neoserviceStarted)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setIsEnbleHomeKey",state); 
        }
#endif
        PLOG.I("PvrLog setIsEnbleHomeKey:" + state);
    }

    public int getControllerSensorStatus(int id)
    {
        int type = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref type,javaCVClass, "getControllerSensorStatus",id);
#endif
        PLOG.I("PvrLog getControllerSensorStatus:" + type);
        return type;
    }

}
