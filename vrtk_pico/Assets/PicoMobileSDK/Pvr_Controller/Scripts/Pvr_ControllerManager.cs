// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using UnityEngine;
using System;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pvr_ControllerManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    private static Pvr_ControllerManager instance = null;

    public static Pvr_ControllerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindObjectOfType<Pvr_ControllerManager>();
            }
            if (instance == null)
            {
                var go = new GameObject("GameObject");
                instance = go.AddComponent<Pvr_ControllerManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return instance;
        }
    }
    #region Properties
    
    public static Pvr_ControllerLink controllerlink;
    private float cTime = 1.0f;
    private bool stopConnect; 
    public Text toast;
    private bool controllerServicestate;
    private float disConnectTime;
    public bool LengthAdaptiveRay;
    private float[] sensorData = new float[28];
    private int[] keyData = new int[134];
    private float[] g2SensorData = new float[4];
    private int[] g2KeyData = new int[47];
    private int keyOffset = 0;
    private int rotControllerMode = 1;
    #endregion

    //Service Start Success
    //The service startup is successful and the API interface can be used normally.
    public delegate void PvrServiceStartSuccess();
    public static event PvrServiceStartSuccess PvrServiceStartSuccessEvent;
    //Controller State event
    //1.Goblin controller，"int a"，0：Disconnect 1：Connect
    //2.Neo controller，"int a,int b"，a(0:controller0,1：controller1)，b(0:Disconnect，1：Connect)  
    public delegate void PvrControllerStateChanged(string data);
    public static event PvrControllerStateChanged PvrControllerStateChangedEvent;
    //Master control hand change
    public delegate void ChangeMainControllerCallBack(string index);
    public static event ChangeMainControllerCallBack ChangeMainControllerCallBackEvent;
    //HandNess Changed
    public delegate void ChangeHandNessCallBack(string index);
    public static event ChangeHandNessCallBack ChangeHandNessCallBackEvent;


    //The following is the separation of platform events, suggesting the use of the above events.
    //goblin service bind success
    public delegate void SetHbServiceBindState();
    public static event SetHbServiceBindState SetHbServiceBindStateEvent;
    //neo ControllerThread start-up success
    public delegate void ControllerThreadStartedCallback();
    public static event ControllerThreadStartedCallback ControllerThreadStartedCallbackEvent;
    //neo service Bind success
    public delegate void SetControllerServiceBindState();
    public static event SetControllerServiceBindState SetControllerServiceBindStateEvent;
    //goblin Controller connection status change
    public delegate void ControllerStatusChange(string isconnect);
    public static event ControllerStatusChange ControllerStatusChangeEvent;
    //neo Controller connection status change
    public delegate void SetControllerAbility(string data);
    public static event SetControllerAbility SetControllerAbilityEvent;
    //neo Controller connection status change
    public delegate void SetControllerStateChanged(string data);
    public static event SetControllerStateChanged SetControllerStateChangedEvent;
    //goblin Mac
    public delegate void SetHbControllerMac(string mac);
    public static event SetHbControllerMac SetHbControllerMacEvent;
    //Get the version
    public delegate void ControllerDeviceVersionCallback(string data);
    public static event ControllerDeviceVersionCallback ControllerDeviceVersionCallbackEvent;
    //Acquisition controller SN
    public delegate void ControllerSnCodeCallback(string data);
    public static event ControllerSnCodeCallback ControllerSnCodeCallbackEvent;
    //controller unbundling
    public delegate void ControllerUnbindCallback(string status);
    public static event ControllerUnbindCallback ControllerUnbindCallbackEvent;
    //Station working status.
    public delegate void ControllerStationStatusCallback(string status);
    public static event ControllerStationStatusCallback ControllerStationStatusCallbackEvent;
    //Station is busy.
    public delegate void ControllerStationBusyCallback(string status);
    public static event ControllerStationBusyCallback ControllerStationBusyCallbackEvent;
    //OTA upgrade error
    public delegate void ControllerOtaStartCodeCallback(string data);
    public static event ControllerOtaStartCodeCallback ControllerOtaStartCodeCallbackEvent;
    //controller version and SN 
    public delegate void ControllerDeviceVersionAndSNCallback(string data);
    public static event ControllerDeviceVersionAndSNCallback ControllerDeviceVersionAndSNCallbackEvent;
    //The controller's unique identification code
    public delegate void ControllerUniqueIDCallback(string data);
    public static event ControllerUniqueIDCallback ControllerUniqueIDCallbackEvent;
    //The combined to controller
    public delegate void ControllerCombinedKeyUnbindCallback(string data);
    public static event ControllerCombinedKeyUnbindCallback ControllerCombinedKeyUnbindCallbackEvent;
    /*************************************  Unity API ****************************************/

    #region Unity API
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (instance != this)
        {
            PLOG.E("instance object should be a singleton.");
            return;
        }
        if (controllerlink == null)
        {
            controllerlink = new Pvr_ControllerLink(this.gameObject.name);
        }
        else
        {
            controllerlink.SetGameObjectToJar(this.gameObject.name);
            BindService();
        }
    }
    // Use this for initialization
    void Start()
    {
#if ANDROID_DEVICE
        if (controllerlink.trackingmode < 2)
        {
            Invoke("CheckControllerService", 10.0f);
        }
#endif
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.RotControllerMode, ref rotControllerMode);
    }

    void Update()
    {
#if UNITY_ANDROID
        if (controllerlink.neoserviceStarted)
        {
            sensorData = controllerlink.GetControllerSensorData();
            keyData = controllerlink.GetControllerKeyData();

            if (controllerlink.controller0Connected)
            {
                if (Pvr_UnitySDKManager.SDK.ShowVideoSeethrough)
                {
                    //var fixedpose0 = controllerlink.GetControllerFixedSensorState(0);
                    // fixed pose
                    sensorData[2] = -sensorData[2];
                    sensorData[3] = -sensorData[3];
                    controllerlink.Controller0.Rotation.Set(sensorData[0], sensorData[1], sensorData[2], sensorData[3]);
                    if (rotControllerMode == 0)
                    {
                        controllerlink.Controller0.Rotation *= Quaternion.Euler(34.0f, 0, 0);
                    }
                    controllerlink.Controller0.Position.Set(sensorData[4] / 1000.0f, sensorData[5] / 1000.0f, -sensorData[6] / 1000.0f);
                }
                else
                {
                    controllerlink.Controller0.Rotation.Set(sensorData[7], sensorData[8], sensorData[9], sensorData[10]);
                    if (rotControllerMode == 0)
                    {
                        controllerlink.Controller0.Rotation *= Quaternion.Euler(34.0f, 0, 0);
                    }
                    controllerlink.Controller0.Position.Set(sensorData[11] / 1000.0f, sensorData[12] / 1000.0f, -sensorData[13] / 1000.0f);
                }

                if (!controllerlink.Controller0.isShowBoundary)
                {
                    if (controllerlink.getControllerSensorStatus(0) == 0)
                    {
                        Sensor.UPvr_SetReinPosition(sensorData[0], sensorData[1], sensorData[2], sensorData[3], sensorData[4], sensorData[5], sensorData[6], 0, false, 0);
                    }
                    else
                    {
                        controllerlink.Controller0.isShowBoundary = true;
                        Sensor.UPvr_SetReinPosition(sensorData[0], sensorData[1], sensorData[2], sensorData[3], sensorData[4], sensorData[5], sensorData[6], 0, true, Convert.ToInt32((Convert.ToString(keyData[35]) + Convert.ToString(keyData[15]) + "00"), 2));
                    }
                }
                else
                {

                    Sensor.UPvr_SetReinPosition(sensorData[0], sensorData[1], sensorData[2], sensorData[3], sensorData[4], sensorData[5], sensorData[6], 0, true, Convert.ToInt32((Convert.ToString(keyData[35]) + Convert.ToString(keyData[15]) + "00"), 2));

                }
                TransformData(controllerlink.Controller0, 0, keyData);
            }
            else
            {
                Sensor.UPvr_SetReinPosition(sensorData[0], sensorData[1], sensorData[2], sensorData[3], sensorData[4], sensorData[5], sensorData[6], 0, false, 0);
            }

            if (controllerlink.controller1Connected)
            {
                if (Pvr_UnitySDKManager.SDK.ShowVideoSeethrough)
                {
                    //var fixedpose1 = controllerlink.GetControllerFixedSensorState(1);
                    // fixed pose
                    sensorData[16] = -sensorData[16];
                    sensorData[17] = -sensorData[17];
                    controllerlink.Controller1.Rotation.Set(sensorData[14], sensorData[15], sensorData[16], sensorData[17]);
                    if (rotControllerMode == 0)
                    {
                        controllerlink.Controller1.Rotation *= Quaternion.Euler(34.0f, 0, 0);
                    }
                    controllerlink.Controller1.Position.Set(sensorData[18] / 1000.0f, sensorData[19] / 1000.0f, -sensorData[20] / 1000.0f);
                }
                else
                {
                    controllerlink.Controller1.Rotation.Set(sensorData[21], sensorData[22], sensorData[23], sensorData[24]);
                    if (rotControllerMode == 0)
                    {
                        controllerlink.Controller1.Rotation *= Quaternion.Euler(34.0f, 0, 0);
                    }
                    controllerlink.Controller1.Position.Set(sensorData[25] / 1000.0f, sensorData[26] / 1000.0f, -sensorData[27] / 1000.0f);
                }

                if (!controllerlink.Controller1.isShowBoundary)
                {
                    if (controllerlink.getControllerSensorStatus(1) == 0)
                    {
                        Sensor.UPvr_SetReinPosition(sensorData[14],sensorData[15], sensorData[16], sensorData[17], sensorData[18], sensorData[19], sensorData[20], 1, false, 0);
                    }
                    else
                    {
                        controllerlink.Controller1.isShowBoundary = true;
                        Sensor.UPvr_SetReinPosition(sensorData[14], sensorData[15], sensorData[16], sensorData[17], sensorData[18], sensorData[19], sensorData[20], 1, true, Convert.ToInt32((Convert.ToString(keyData[102]) + Convert.ToString(keyData[82]) + "00"), 2));
                    }
                }
                else
                {
                    Sensor.UPvr_SetReinPosition(sensorData[14], sensorData[15], sensorData[16], sensorData[17], sensorData[18], sensorData[19], sensorData[20], 1, true, Convert.ToInt32((Convert.ToString(keyData[102]) + Convert.ToString(keyData[82]) + "00"), 2));
                }
                TransformData(controllerlink.Controller1, 1, keyData);
            }
            else
            {
                Sensor.UPvr_SetReinPosition(sensorData[14], sensorData[15], sensorData[16], sensorData[17], sensorData[18], sensorData[19], sensorData[20], 1, false, 0);
            }
        }

        //Goblin controller
        if (controllerlink.goblinserviceStarted && controllerlink.controller0Connected)
        {
            g2SensorData = controllerlink.GetHBControllerPoseData();
            controllerlink.Controller0.Rotation.Set(g2SensorData[0], g2SensorData[1], g2SensorData[2], g2SensorData[3]);

            g2KeyData = controllerlink.GetHBControllerKeyData();
            TransformData(controllerlink.Controller0, 0, g2KeyData);
        }

        SetSystemKey();
#endif
    }

    private void OnApplicationPause(bool pause)
    {
        var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
        var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;

        if (pause)
        {
            if (controllerlink.neoserviceStarted)
            {
                controllerlink.SetGameObjectToJar("");
                controllerlink.StopControllerThread(headdof, handdof);
            }
            if (controllerlink.goblinserviceStarted)
            {
                controllerlink.StopLark2Receiver();
            }
        }
        else
        {
            controllerlink.Controller0 = new ControllerHand();
            controllerlink.Controller1 = new ControllerHand();
            if (controllerlink.neoserviceStarted)
            {
                controllerlink.SetGameObjectToJar(this.gameObject.name);
                controllerlink.SetUnityVersionToJar(Pvr_UnitySDKAPI.System.UnitySDKVersion);
                controllerlink.StartControllerThread(headdof, handdof);
            }
            if (controllerlink.goblinserviceStarted)
            {
                controllerlink.StartLark2Receiver();
                controllerlink.controller0Connected = GetControllerConnectionState(0) == 1;
                controllerlink.controllerType = controllerlink.GetControllerType();
                controllerlink.handness = (Pvr_Controller.UserHandNess)controllerlink.getHandness();

                if (PvrServiceStartSuccessEvent != null)
                    PvrServiceStartSuccessEvent();
            }
        }
    }

    private void OnDestroy()
    {
#if ANDROID_DEVICE
        controllerlink.UnBindService();
        Sensor.UPvr_SetReinPosition(0, 0, 0, 1, 0, 0, 0, 0, false, 0);
        Sensor.UPvr_SetReinPosition(0, 0, 0, 1, 0, 0, 0, 1, false, 0);
#endif
    }
    // Update is called once per frame

    void OnApplicationQuit()
    {
        var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
        var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;
      
        if (controllerlink.neoserviceStarted)
        {
            controllerlink.SetGameObjectToJar("");
            controllerlink.StopControllerThread(headdof, handdof);
        }
            
    }
    
#endregion

    /************************************ Public Interfaces  *********************************/
#region Public Interfaces

   
    public void StopLark2Service()
    {
        if (controllerlink != null)
        {
            controllerlink.StopLark2Service();
        }
    }
  
    public Vector3 GetAngularVelocity(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAngularVelocity(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }
   
    public Vector3 GetAcceleration(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAcceleration(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void BindService()
    {
        if (controllerlink != null)
        {
            controllerlink.BindService();
        }
    }
    public void StartScan()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.StartScan();
        }
#endif
    }
    public void StopScan()
    {
        if (controllerlink != null)
        {
            controllerlink.StopScan();
        }
    }
   
    public void ResetController(int num)
    {
        if (controllerlink != null)
        {
            controllerlink.ResetController(num);
        }
    }
    public static int GetControllerConnectionState(int num)
    {
        return controllerlink.GetControllerConnectionState(num);
    }
    public void ConnectBLE()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.ConnectBLE();
        }
#endif
    }
    public void DisConnectBLE()
    {
        if (controllerlink != null)
        {
            controllerlink.DisConnectBLE();
        }
    }

    public void SetBinPath(string path, bool isAsset)
    {
        if (controllerlink != null)
        {
            controllerlink.setBinPath(path, isAsset);
        }
    }
    public void StartUpgrade()
    {
        if (controllerlink != null)
        {
            controllerlink.StartUpgrade();
        }
    }
    public static string GetBLEImageType()
    {
        var type = controllerlink.GetBLEImageType();
        return type;
    }
    public static long GetBLEVersion()
    {
        var version = controllerlink.GetBLEVersion();
        return version;
    }
    public static string GetFileImageType()
    {
        var type = controllerlink.GetFileImageType();
        return type;
    }
    public static long GetFileVersion()
    {
        var version = controllerlink.GetFileVersion();
        return version;
    }
    public static void AutoConnectHbController(int scans)
    {
        if (controllerlink != null)
        {
            controllerlink.AutoConnectHbController(scans);
        }
    }
    public static string GetConnectedDeviceMac()
    {
        string mac = "";
        if (controllerlink != null)
        {
            mac = controllerlink.GetConnectedDeviceMac();
        }
        return mac;
    }
    //--------------
    public void setHbControllerMac(string mac)
    {
        PLOG.I("PvrLog HBMacRSSI" + mac);
        controllerlink.hummingBirdMac = mac.Substring(0, 17);
        controllerlink.hummingBirdRSSI = Convert.ToInt16(mac.Remove(0, 18));
        if (SetHbControllerMacEvent != null)
            SetHbControllerMacEvent(mac.Substring(0, 17));

    }
    public int GetControllerRSSI()
    {
        return controllerlink.hummingBirdRSSI;
    }

    public void setHbServiceBindState(string state)
    {
        PLOG.I("PvrLog HBBindCallBack" + state);
        controllerServicestate = true;
        //State: 0- unbound, 1- bound, 2- timed.
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.goblinserviceStarted = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.goblinserviceStarted = true;
            controllerlink.controller0Connected = GetControllerConnectionState(0) == 1;
            controllerlink.controllerType = controllerlink.GetControllerType();
            controllerlink.handness = (Pvr_Controller.UserHandNess)controllerlink.getHandness();
            if (SetHbServiceBindStateEvent != null)
            {
                SetHbServiceBindStateEvent();
            }
            if (PvrServiceStartSuccessEvent != null)
            {
                PvrServiceStartSuccessEvent();
            }
        }
    }
    public void setControllerServiceBindState(string state)
    {
        PLOG.I("PvrLog CVBindCallBack" + state);
        //state:0 unbind,1:bind
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.neoserviceStarted = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.SetUnityVersionToJar(Pvr_UnitySDKAPI.System.UnitySDKVersion);
            controllerlink.neoserviceStarted = true;
            var headdof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
            var handdof = Pvr_UnitySDKManager.SDK.ControllerOnlyrot ? 0 : 1;
            controllerlink.StartControllerThread(headdof, handdof);
            if (SetControllerServiceBindStateEvent != null)
                SetControllerServiceBindStateEvent();

        }

    }
    public void setHbControllerConnectState(string isconnect)
    {
        PLOG.I("PvrLog HBControllerConnect" + isconnect);
        controllerlink.controller0Connected = Convert.ToInt16(isconnect) == 1;
        if (!controllerlink.controller0Connected)
        {
            controllerlink.Controller0 = new ControllerHand();
        }
        else
        {
            ResetController(0);
            controllerlink.controllerType = controllerlink.GetControllerType();
            controllerlink.handness = (Pvr_Controller.UserHandNess)controllerlink.getHandness();
        }
        //State: 0- disconnect, 1- connected, 2- unknown.
        stopConnect = false;
        if (ControllerStatusChangeEvent != null)
            ControllerStatusChangeEvent(isconnect);
        if (PvrControllerStateChangedEvent != null)
            PvrControllerStateChangedEvent(isconnect);
    }
    
    public void setControllerStateChanged(string state)
    {
        PLOG.I("PvrLog CVControllerStateChanged" + state);
        
        int controller = Convert.ToInt16(state.Substring(0, 1));
        if (controller == 0)
        {
            controllerlink.controller0Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller0Connected)
            {
                controllerlink.Controller0 = new ControllerHand();
                controllerlink.Controller0.Position = new Vector3(0, Pvr_UnitySDKSensor.Instance.HeadPose.Position.y, 0) + new Vector3(-0.1f, -0.3f, 0.3f);
            }
                
        }
        else
        {
            controllerlink.controller1Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller1Connected)
            {
                controllerlink.Controller1 = new ControllerHand();
                controllerlink.Controller1.Position = new Vector3(0, Pvr_UnitySDKSensor.Instance.HeadPose.Position.y, 0) + new Vector3(0.1f, -0.3f, 0.3f);
            }   
        }
        if (Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1))))
        { 
            controllerlink.controllerType = controllerlink.GetControllerType();
            controllerlink.ResetController(controller);
        }
        controllerlink.handness = (Pvr_Controller.UserHandNess)controllerlink.getHandness();
        controllerlink.mainHandID = controllerlink.GetMainControllerIndex();
        if (SetControllerStateChangedEvent != null)
            SetControllerStateChangedEvent(state);
        if (PvrControllerStateChangedEvent != null)
            PvrControllerStateChangedEvent(state);

    }
 
    public void setControllerAbility(string data)
    {
        //data format is ID,ability,state.
        //ID 0/1 represents two handles.
        //ability 1/2 1:3dof controller 2. 6dof controller.
        //state 0/1 0: disconnect 1: connection.
        //this callback for setControllerStateChanged extended edition, on the basis of this callback to increase the ability of controller
        PLOG.I("PvrLog setControllerAbility" + data);
        if (SetControllerAbilityEvent != null)
            SetControllerAbilityEvent(data);
    }

    public void controllerThreadStartedCallback()
    {
        PLOG.I("PvrLog ThreadStartSuccess");
        GetCVControllerState();
        if (ControllerThreadStartedCallbackEvent != null)
            ControllerThreadStartedCallbackEvent();
        if (PvrServiceStartSuccessEvent != null)
            PvrServiceStartSuccessEvent();
    }


    public void controllerDeviceVersionCallback(string data)
    {
        PLOG.I("PvrLog VersionCallBack" + data);
        //data format device, deviceVersion
        //device: 0-station 1- controller 0 2- controller 1 deviceVersion: version number.
        if (ControllerDeviceVersionCallbackEvent != null)
            ControllerDeviceVersionCallbackEvent(data);
    }

    public void controllerSnCodeCallback(string data)
    {
        PLOG.I("PvrLog SNCodeCallBack" + data);
        //data formats: controllerSerialNum, controllerSn
        //controllerSerialNum: 0- controller 1 controllerSn: the unique identification of the controller of Sn.
        if (ControllerSnCodeCallbackEvent != null)
            ControllerSnCodeCallbackEvent(data);
    }
   
    public void controllerUnbindCallback(string status)
    {
        PLOG.I("PvrLog ControllerUnBindCallBack" + status);
        // status: 0- all unbind  1- unbind left 2-unbind right 
        if (ControllerUnbindCallbackEvent != null)
            ControllerUnbindCallbackEvent(status);
    }
    
    public void controllerStationStatusCallback(string status)
    {
        PLOG.I("PvrLog StationStatusCallBack" + status);
        //STATION_STATUS{NORMAL = 0, QUERYING = 1, PAIRING = 2, OTA = 3, RESTARTING = 4, CTRLR_UNBINDING = 5, CTRLR_SHUTTING_DOWN = 6};
        if (ControllerStationStatusCallbackEvent != null)
            ControllerStationStatusCallbackEvent(status);
    }
    
    public void controllerStationBusyCallback(string status)
    {
        PLOG.I("PvrLog StationBusyCallBack" + status);
        //STATION_STATUS{NORMAL = 0, QUERYING, PAIRING, OTA, RESTARTING, CTRLR_UNBINDING, CTRLR_SHUTTING_DOWN};
        if (ControllerStationBusyCallbackEvent != null)
            ControllerStationBusyCallbackEvent(status);
    }
   
    public void controllerOTAStartCodeCallback(string data)
    {
        PLOG.I("PvrLog OTAUpdateCallBack" + data);
        //data:deviceType,statusCode
        // deviceType:0-station 1-controller statusCode: 0- upgrade launch success 1- upgrade file not found 2- upgrade file failed to open.
        if (ControllerOtaStartCodeCallbackEvent != null)
            ControllerOtaStartCodeCallbackEvent(data);
    }
 
    public void controllerDeviceVersionAndSNCallback(string data)
    {
        PLOG.I("PvrLog DeviceVersionAndSNCallback" + data);
        //data controllerSerialNum,deviceVersion
        //controllerSerialNum : 0- controller 0 1- controller 1 deviceVersion: version and SN 
        if (ControllerDeviceVersionAndSNCallbackEvent != null)
            ControllerDeviceVersionAndSNCallbackEvent(data);
    }
    
    public void controllerUniqueIDCallback(string data)
    {
        PLOG.I("PvrLog controllerUniqueIDCallback" + data);
        //data controller0ID，controller1ID
        //controller0ID ：ID of controller 0;Controller1ID: ID of controller 1 (if the current controller is not connected, the ID will return to 0)
        if (ControllerUniqueIDCallbackEvent != null)
            ControllerUniqueIDCallbackEvent(data);
    }
    
    public void controllerCombinedKeyUnbindCallback(string controllerSerialNum)
    {
        //controllerSerialNum 0：controller0 1：controller1
        if (ControllerCombinedKeyUnbindCallbackEvent != null)
            ControllerCombinedKeyUnbindCallbackEvent(controllerSerialNum);
    }
    public void setupdateFailed()
    {
        
    }

    public void setupdateSuccess()
    {
        
    }

    public void setupdateProgress(string progress)
    {
        //The upgrade progress 0-100 
    }

    public void setHbAutoConnectState(string state)
    {
        PLOG.I("PvrLog HBAutoConnectState" + state);
        // UNKNOW = 1; the default value
        // NO_DEVICE = 0; No scan to HB controller.
        // ONLY_ONE = 1; Scan only one HB controller.
        // MORE_THAN_ONE = 2; Scan to multiple HB handles.
        // LAST_CONNECTED = 3; Scan the HB controller that was last connected.
        // FACTORY_DEFAULT = 4; Scan the HB controller of the factory binding(temporarily not enabled)
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            if (GetControllerConnectionState(0) == 0)
            {
                ShowToast(2);
            }
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(3);
        }
    }

    public void callbackControllerServiceState(string state)
    {
        PLOG.I("PvrLog HBServiceState" + state);
        //state = 0,Non-mobile platform, service is not started.
        //state = 1,The mobile platform, the service is not started, but the system will initiate the service.
        //state = 2,Mobile platform, service apk is not installed, need to install.
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            ShowToast(0);
        }
        if (Convert.ToInt16(state) == 1)
        {
            BindService();
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(1);
        }
    }
  
    public void changeMainControllerCallback(string index)
    {
        PLOG.I("PvrLog MainControllerCallBack" + index);

        controllerlink.mainHandID = Convert.ToInt16(index);
        //index = 0/1
        if (ChangeMainControllerCallBackEvent != null)
            ChangeMainControllerCallBackEvent(index);

    }

    public void changeHandnessCallback(string index)
    {
        PLOG.I("PvrLog changeHandnessCallback" + index);
        controllerlink.handness = (Pvr_Controller.UserHandNess) Convert.ToInt16(index);
        if (ChangeHandNessCallBackEvent != null)
            ChangeHandNessCallBackEvent(index);
    }

    private void ShowToast(int type)
    {
        if (toast != null)
        {
            switch (type)
            {
                case 0:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip0");
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 1:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip1");
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 2:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip2");
                        AutoConnectHbController(6000);
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 3:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip3");
                        AutoConnectHbController(6000);
                        Invoke("HideToast", 5.0f);
                    }
                    break;
                case 4:
                    {
                        toast.text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("servicetip4");
                        Invoke("HideToast", 10.0f);
                    }
                    break;
                default:
                    return;
            }
        }
    }

    private void HideToast()
    {
        if (toast != null)
        {
            toast.text = "";
        }
    }

    private void CheckControllerService()
    {
        if (!controllerServicestate)
        {
            ShowToast(4);
        }
    }

    private void GetCVControllerState()
    {
        var state0 = GetControllerConnectionState(0);
        var state1 = GetControllerConnectionState(1);
        PLOG.I("PvrLog CVconnect" + state0 + state1);
        if (state0 == -1 && state1 == -1)
        {
            Invoke("GetCVControllerState", 0.02f);
        }
        if (state0 != -1 && state1 != -1)
        {
            controllerlink.controller0Connected = state0 == 1;
            controllerlink.controller1Connected = state1 == 1;
            if (!controllerlink.controller0Connected && controllerlink.controller1Connected)
            {
                if (controllerlink.GetMainControllerIndex() == 0)
                {
                    Controller.UPvr_SetMainHandNess(1);
                }
            }

            if (controllerlink.controller0Connected || controllerlink.controller1Connected)
            {
                controllerlink.controllerType = controllerlink.GetControllerType();
            }

            controllerlink.mainHandID = controllerlink.GetMainControllerIndex();
            controllerlink.handness = (Pvr_Controller.UserHandNess)controllerlink.getHandness();
        }
    }

    private void SetSystemKey()
    {
        if (controllerlink.switchHomeKey)
        {
            if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME) || Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
            {
                if (Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                {
                    Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 0);
                }
                else
                {
                    if (controllerlink.trackingmode == 0 || controllerlink.trackingmode == 1)
                    {
                        Pvr_UnitySDKSensor.Instance.ResetUnitySDKSensor();
                    }
                    else
                    {
                        Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
                    }
                }
                if (Pvr_UnitySDKManager.SDK.ControllerOnlyrot || controllerlink.controller0Connected && Controller.UPvr_GetControllerPOS(0).Equals(Vector3.zero) || controllerlink.controller1Connected && Controller.UPvr_GetControllerPOS(1).Equals(Vector3.zero))
                {
                    if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME))
                        ResetController(0);
                    if (Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
                        ResetController(1);
                }
            }
        }
        if (controllerlink.picoDevice)
        {
            if (controllerlink.switchHomeKey)
            {
                if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.HOME) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.HOME) && !stopConnect)
                {
                    controllerlink.RebackToLauncher();
                }
            }
            if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.VOLUMEUP))
            {
                controllerlink.TurnUpVolume();
            }
            if (Controller.UPvr_GetKeyClick(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKeyClick(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                controllerlink.TurnDownVolume();
            }
            if (!Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                cTime = 1.0f;
            }
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnUpVolume();
                }
            }
            if (!Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.HOME) && (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN)))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnDownVolume();
                }
            }
        }
        if (controllerlink.goblinserviceStarted)
        {
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME) && Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !stopConnect)
            {
                disConnectTime += Time.deltaTime;
                if (disConnectTime > 1.0)
                {
                    DisConnectBLE();
                    controllerlink.hummingBirdMac = "";
                    stopConnect = true;
                    disConnectTime = 0;
                }
            }
        }
    }

    /// <summary>
    /// Data transformation, encapsulating key values as apis
    /// </summary>
    private void TransformData(ControllerHand hand, int handId, int[] data)
    {
        keyOffset = handId == 1 ? 67 : 0;

        hand.TouchPadPosition.x = data[0 + keyOffset];
        hand.TouchPadPosition.y = data[5 + keyOffset];

        TransSingleKey(hand.Home, 10 + keyOffset, data);
        TransSingleKey(hand.App, 15 + keyOffset, data);
        TransSingleKey(hand.Touch, 20 + keyOffset, data);
        TransSingleKey(hand.VolumeUp, 25 + keyOffset, data);
        TransSingleKey(hand.VolumeDown, 30 + keyOffset, data);
        TransSingleKey(hand.Trigger, 35 + keyOffset, data);

        if (controllerlink.goblinserviceStarted && !controllerlink.neoserviceStarted)
        {
            hand.TriggerNum = controllerlink.GetHBKeyValue();
        }

        if (!controllerlink.goblinserviceStarted && controllerlink.neoserviceStarted)
        {
            hand.TriggerNum = controllerlink.GetCVTriggerValue(handId);
        }

        hand.Battery = data[40 + keyOffset];

        if (data.Length == 47)
        {
            hand.SwipeDirection = (SwipeDirection)data[45];
            hand.TouchPadClick = (TouchPadClick)data[46];
        }
        else
        {
            switch (handId)
            {
                case 0:
                    TransSingleKey(hand.X, 45 + keyOffset, data);
                    TransSingleKey(hand.Y, 50 + keyOffset, data);
                    TransSingleKey(hand.Left, 60 + keyOffset, data);
                    break;
                case 1:
                    TransSingleKey(hand.A, 45 + keyOffset, data);
                    TransSingleKey(hand.B, 50 + keyOffset, data);
                    TransSingleKey(hand.Right, 55 + keyOffset, data);
                    break;
            }

            hand.SwipeDirection = (SwipeDirection)data[65 + keyOffset];
            hand.TouchPadClick = (TouchPadClick)data[66 + keyOffset];
        }

        hand.GripValue = controllerlink.GetNeo3GripValue(handId);

        TransformTouchData(hand,handId, controllerlink.GetNeo3TouchData(handId));
    }

    private void TransSingleKey(PvrControllerKey key, int beginIndex, int[] data)
    {
        key.State = Convert.ToBoolean(data[beginIndex]);
        key.PressedDown = Convert.ToBoolean(data[beginIndex + 1]);
        key.PressedUp = Convert.ToBoolean(data[beginIndex + 2]);
        key.LongPressed = Convert.ToBoolean(data[beginIndex + 3]);
        key.Click = Convert.ToBoolean(data[beginIndex + 4]);
    }

    private void TransformTouchData(ControllerHand hand, int handId, int[] data)
    {
        switch (handId)
        {
            case 0:
                TransSingleTouchValue(hand.X, 0, data);
                TransSingleTouchValue(hand.Y, 3, data);
                break;
            case 1:
                TransSingleTouchValue(hand.A, 0, data);
                TransSingleTouchValue(hand.B, 3, data);
                break;
        }
        TransSingleTouchValue(hand.Touch, 6, data);
        TransSingleTouchValue(hand.Trigger, 9 , data);
        TransSingleTouchValue(hand.Thumbrest, 12, data);
    }

    private void TransSingleTouchValue(PvrControllerKey key, int beginIndex, int[] data)
    {
        key.Touch = Convert.ToBoolean(data[beginIndex]);
        key.TouchDown = Convert.ToBoolean(data[beginIndex + 1]);
        key.TouchUp = Convert.ToBoolean(data[beginIndex + 2]);
    }

    #endregion

}
