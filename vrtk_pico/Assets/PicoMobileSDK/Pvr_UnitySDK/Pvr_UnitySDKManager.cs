// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_UnitySDKManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    #region Properties
    public static PlatForm platform;

    private static Pvr_UnitySDKManager sdk = null;
    public static Pvr_UnitySDKManager SDK
    {
        get
        {
            if (sdk == null)
            {
                sdk = UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKManager>();
            }
            return sdk;
        }
    }

    [HideInInspector]
    public float EyesAspect = 1.0f;
    
    [HideInInspector]
    public int posStatus = 0;
    [HideInInspector]
    public bool ismirroring;
    [HideInInspector]
    public Vector3 resetBasePos = new Vector3();
    [HideInInspector]
    public int trackingmode = -1;
    [HideInInspector]
    public int systemprop = -1;
    [HideInInspector]
    public bool systemFPS = false;

    [HideInInspector]
    public float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    
    [SerializeField]
    private bool rotfoldout = false;

    public bool Rotfoldout
    {
        get { return rotfoldout; }
        set
        {
            if (value != rotfoldout)
                rotfoldout = value;
        }
    }

    [SerializeField]
    private bool hmdOnlyrot =false;

    public bool HmdOnlyrot
    {
        get { return hmdOnlyrot; }
        set
        {
            if (value != hmdOnlyrot)
                hmdOnlyrot = value;
        }
    }
    [SerializeField]
    private bool controllerOnlyrot = false;

    public bool ControllerOnlyrot
    {
        get { return controllerOnlyrot; }
        set
        {
            if (value != controllerOnlyrot)
                controllerOnlyrot = value;
        }
    }
    /// <summary>
    /// Represents how the SDK is reporting pose data.(EyeLevel for Default)
    /// </summary>
    [SerializeField]
    private TrackingOrigin trackingOrigin = TrackingOrigin.EyeLevel;
    public TrackingOrigin TrackingOrigin
    {
        get
        {
            return this.trackingOrigin;
        }

        set
        {
            if (value != this.trackingOrigin)
            {
                this.trackingOrigin = value;

                Pvr_UnitySDKAPI.Sensor.UPvr_SetTrackingOriginType(value);
            }
        }
    }

    /// <summary>
    /// Reset Tracker OnLoad
    /// </summary>
    public bool ResetTrackerOnLoad = false;

    // Becareful, you must excute this before Pvr_UnitySDKManager script
    public void ChangeDefaultCustomRtSize(int w, int h)
    {
        Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize = new Vector2(w, h);
    }

    public Vector3 EyeOffset(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeOffset : rightEyeOffset;
    }
    [HideInInspector]
    public Vector3 leftEyeOffset;
    [HideInInspector]
    public Vector3 rightEyeOffset;
    public Rect EyeRect(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeRect : rightEyeRect;
    }
    [HideInInspector]
    public Rect leftEyeRect;
    [HideInInspector]
    public Rect rightEyeRect;
    [HideInInspector]
    public Matrix4x4 leftEyeView;
    [HideInInspector]
    public Matrix4x4 rightEyeView;

    // unity editor
    [HideInInspector]
    public Pvr_UnitySDKEditor pvr_UnitySDKEditor;
    [SerializeField]
    private bool vrModeEnabled = true;
    [HideInInspector]
    public bool VRModeEnabled
    {

        get
        {
            return vrModeEnabled;
        }
        set
        {
            if (value != vrModeEnabled)
                vrModeEnabled = value;

        }
    }
    [HideInInspector]
    public Material Eyematerial;
    [HideInInspector]
    public Material Middlematerial;
    [HideInInspector]
    public bool picovrTriggered { get; set; }
    [HideInInspector]
    public bool newPicovrTriggered = false;

    // FPS
    [SerializeField]
    private bool showFPS;
    public bool ShowFPS
    {
        get
        {
            return showFPS;
        }
        set
        {
            if (value != showFPS)
            {
                showFPS = value;
            }
        }
    }

    //Neck model
    [HideInInspector]
    public Vector3 neckOffset = new Vector3(0, 0.075f, 0.0805f);


    [SerializeField]
    private bool pVRNeck = true;
    public bool PVRNeck
    {
        get { return pVRNeck; }
        set
        {
            if (value != pVRNeck)
                pVRNeck = value;
        }
    }
    [HideInInspector]
    public bool UseCustomNeckPara = false;

    // life
    [HideInInspector]
    public bool onResume = false;
    [HideInInspector]
    public bool isEnterVRMode = false;

    public bool isHasController = false;
    public Pvr_UnitySDKConfigProfile pvr_UnitySDKConfig;

    private GameObject calltoast;
    private GameObject msgtoast;
    private GameObject lowhmdBatterytoast;
    private GameObject lowphoneBatterytoast;
    private GameObject LowPhoneHealthtoast;
    private GameObject LowcontrollerBatterytoast;
    private bool lowControllerpowerstate = false;
    private float controllerpowershowtime = 0f;
    private bool UseToast = true;
    private int iPhoneHMDModeEnabled;

    private GameObject G3LiteTips;
    
    [SerializeField]
    private bool monoscopic = false;

    [HideInInspector]
    public bool Monoscopic
    {
        get { return monoscopic; }
        set
        {
            if (value != monoscopic)
            {
                monoscopic = value;
                // if monoscopick change, reset mono mode
                Pvr_UnitySDKAPI.Render.UPvr_SetMonoMode(monoscopic);
            }
        }
    }

    private bool mIsAndroid7 = false;
    public static Func<bool> eventEnterVRMode;

    [HideInInspector]
    public bool ShowVideoSeethrough = false;

    public int SystemDebugFFRLevel = -1;
    public int SystemFFRLevel = -1;
    //Entitlement Check Result
    public int AppCheckResult = 100;
    public delegate void EntitlementCheckResult(int ReturnValue);
    public static event EntitlementCheckResult EntitlementCheckResultEvent;
    #endregion

    /************************************ Private Interfaces  *********************************/

    private bool SDKManagerInit()
    {
        if (SDKManagerInitConfigProfile())
        {
            mIsAndroid7 = SystemInfo.operatingSystem.Contains("Android OS 7.");
            PLOG.I("Android 7 = " + mIsAndroid7);
#if UNITY_EDITOR
            if (SDKManagerInitEditor())
                return true;
            else
                return false;
#else

            if (SDKManagerInitCoreAbility())

                return true;
            else
                return false;
#endif
        }
        else
            return false;
    }

    private bool SDKManagerInitCoreAbility()
    {
        Pvr_UnitySDKAPI.Sensor.UPvr_SetTrackingOriginType(this.trackingOrigin);
        Pvr_UnitySDKAPI.Render.UPvr_SetMonoMode(this.monoscopic);
        if (Pvr_UnitySDKRender.Instance == null)
        {
            PLOG.I("pvr_UnitySDKRender init failed");
        }
        if (Pvr_UnitySDKSensor.Instance == null)
        {
            PLOG.I("pvr_UnitySDKSensor init failed");
        }
        
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);

        return true;
    }

    public void smsReceivedCallback(string msg)
    {
        PLOG.I("PvrLog MSG" + msg);

        var Jdmsg = LitJson.JsonMapper.ToObject(msg);

        string name = "";
        if (msg.Contains("messageSender"))
        {
            name = (string)Jdmsg["messageSender"];
        }

        string number = "";
        if (msg.Contains("messageAdr"))
        {
            number = (string)Jdmsg["messageAdr"];
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number = TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        //string body = "";
        //if (msg.Contains("messageBody"))
        //{
        //    body = (string)Jdmsg["messageBody"];
        //}
        //DateTime dt = DateTime.Parse("1970-01-01 00:00:00").AddMilliseconds(Convert.ToInt64((Int64)Jdmsg["messageTime"]));
        //string time = dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (UseToast)
        {
            msgtoast.transform.Find("number").GetComponent<Text>().text = number;
            msgtoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }

            StartCoroutine(ToastManager(2, true, 0f));
            StartCoroutine(ToastManager(2, false, 5.0f));
        }
    }

    public void phoneStateCallback(string state)
    {
        PLOG.I("PvrLog phone" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string number = "";
        if (state.Contains("phoneNumber"))
        {
            number = (string)Jdstate["phoneNumber"];
            
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number =TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        string name = "";
        if (state.Contains("contactName"))
        {
           name = (string)Jdstate["contactName"];
        }
        
        if (UseToast)
        {
            calltoast.transform.Find("number").GetComponent<Text>().text = number;
            calltoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }
            
            StartCoroutine(ToastManager(1, true, 0f));
            StartCoroutine(ToastManager(1, false, 5.0f));
        }
    }

    public void phoneBatteryStateCallback(string state)
    {
        PLOG.I("PvrLog phoneBatteryState" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string level = "";
        if (state.Contains("phoneBatteryLevel"))
        {
            level = (string)Jdstate["phoneBatteryLevel"];
        }
        string health = "";
        if (state.Contains("phoneBatteryHealth"))
        {
            health = (string)Jdstate["phoneBatteryHealth"];
        }
        
        if (UseToast)
        {
            if (Convert.ToInt16(level) <= 5)
            {
                if (lowhmdBatterytoast.activeSelf == false)
                {
                    StartCoroutine(ToastManager(4, true, 0f));
                    StartCoroutine(ToastManager(4, false, 3.0f));
                }
                else
                {
                    StartCoroutine(ToastManager(4, true, 5.0f));
                    StartCoroutine(ToastManager(4, false, 8.0f));
                }
                
            }
            if (Convert.ToInt16(health) == 3)
            {
                StartCoroutine(ToastManager(5, true, 0f));
                StartCoroutine(ToastManager(5, false, 5.0f));
            }
        }
    }
    public void hmdLowBatteryCallback(string level)
    {
        PLOG.I("PvrLog hmdLowBatteryCallback" + level);

        if (UseToast)
        {
            if (lowphoneBatterytoast.activeSelf == false)
            {
                StartCoroutine(ToastManager(3, true, 0f));
                StartCoroutine(ToastManager(3, false, 3.0f));
            }
            else
            {
                StartCoroutine(ToastManager(3, true, 5.0f));
                StartCoroutine(ToastManager(3, false, 8.0f));
            }
            
        }
    }
    private string TransformNumber(string number)
    {
        if (number.Length == 11)
        {
            //0xy-1234-1234
            //x = 3,4,5,6
            //y = 1,2,3,4,5

            //01x-1234-1234
            //x=0,1,6,7,8...
            var part1 = number.Substring(0, 3);
            var part2 = number.Substring(3, 4);
            var part3 = number.Substring(7, 4);

            number = part1 + "-" + part2 + "-" + part3;
        }
        else if (number.Length == 10)
        {
            //01x-123-1234
            if (number.Substring(1, 1) == "1")
            {
                var part1 = number.Substring(0, 3);
                var part2 = number.Substring(3, 3);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            //02-1234-1234
            else
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 4);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
        }
        //02-123-1234
        else if (number.Length == 9)
        {
            if (number.Substring(1, 1) == "2")
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 3);
                var part3 = number.Substring(5, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            else
            {
                number = "+82" + number.Remove(0, 1);
            }
        }
        return number;
    }
    //Head reset is complete
    public void onHmdOrientationReseted()
    {

    }

    private IEnumerator ToastManager(int type,bool state,float time)
    {
        yield return new WaitForSeconds(time);

        switch (type)
        {
            //call toast
            case 1:
                {
                    calltoast.SetActive(state);
                    break;
                }
            //msg toast
            case 2:
                {
                    msgtoast.SetActive(state);
                    break;
                }
            //low hmd battery toast
            case 3:
                {
                    lowhmdBatterytoast.SetActive(state);
                    break;
                }
            //low phone battery toast
            case 4:
                {
                    lowphoneBatterytoast.SetActive(state);
                    break;
                }
            //low phone health toast
            case 5:
                {
                    LowPhoneHealthtoast.SetActive(state);
                    break;
                }
            //low controller battery toast
            case 6:
                {
                    LowcontrollerBatterytoast.SetActive(state);
                    break;
                }
        }

    }

    private void CheckControllerStateForG2(string state)
    {
        if (iPhoneHMDModeEnabled == 1)
        {
            if (Convert.ToBoolean(Convert.ToInt16(state)) && Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                StartCoroutine(ToastManager(6, true, 0f));
                StartCoroutine(ToastManager(6, false, 3.0f));
            }
        }
    }

    //-1:unknown 0:sms 1:call 2:msg 3:lowbat 4:overheat 5:general
    public void notificationCallback(string data)
    {
        LitJson.JsonData jdata = LitJson.JsonMapper.ToObject(data);
        if (G3LiteTips == null)
        {
            G3LiteTips = Instantiate(Resources.Load("Prefabs/G3LiteTips") as GameObject, transform.Find("Head"), false);
        }
        string tmp =  jdata["str"].ToString();
        LitJson.JsonData callbackdata = LitJson.JsonMapper.ToObject(tmp);
        switch ((int)jdata["type"])
        {
            case -1:
                {
                    //unknown
                }
                break;
            case 0:
                {
                    //sms
                    SetProperty(0, callbackdata,"Sms");
                }
                break;
            case 1:
                {
                    //call
                    SetProperty(1, callbackdata, "Call");
                }
                break;
            case 2:
                {
                    //msg
                    SetProperty(2, callbackdata, "Warnning");
                }
                break;
            case 3:
                {
                    //lowbat
                    SetProperty(3, callbackdata, "Warnning");
                }
                break;
            case 4:
                {
                    //overheat
                    SetProperty(4, callbackdata, "Warnning");
                }
                break;
            case 5:
                {
                    //general
                    var image = G3LiteTips.transform.Find("Onlyimage");
                    SetBaseProperty(image, callbackdata["General"], "");
                    SetImageProperty(image, callbackdata["General"], "");
                    image.gameObject.SetActive(true);
                    StartCoroutine(G3TipsManager(image.gameObject, (int)callbackdata["General"]["time"]));
                }
                break;
        }
    }

    private Sprite LoadSprite(Vector2 size, string filepath)
    {
        int t_w = (int)size.x;
        int t_h = (int)size.y;
        var m_tex = new Texture2D(t_w, t_h);
        m_tex.LoadImage(ReadTex(filepath));
        Sprite sp = Sprite.Create(m_tex, new Rect(0, 0, m_tex.width, m_tex.height), new Vector2(0.5f, 0.5f));
        return sp;
    }

    private byte[] ReadTex(string path)
    {
        if (path == "")
        {
            return new byte[0];
        }
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] binary = new byte[fileStream.Length];
        fileStream.Read(binary, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        return binary;
    }

    private void SetProperty(int type, LitJson.JsonData data, string value)
    {
        var trans = G3LiteTips.transform.Find(value);
        SetBaseProperty(trans, data, "");
        SetImageProperty(trans, data, "");
        trans.gameObject.SetActive(true);
        StartCoroutine(G3TipsManager(trans.gameObject, (int)data["time"]));

        var icon = trans.transform.Find("icon");
        SetBaseProperty(icon, data, "icon_");
        SetImageProperty(icon, data, "icon_");

        var title = trans.transform.Find("title");
        SetBaseProperty(title, data, "title_");
        SetTextProperty(title, data, "title_");

        if (type != 1)
        {
            var details = trans.transform.Find("details");
            SetBaseProperty(details, data, "details_");
            SetTextProperty(details, data, "details_");

            var image1 = trans.transform.Find("image1");
            SetBaseProperty(image1, data, "image1_");
            SetImageProperty(image1, data, "image1_");
        }
        if (type == 0 || type == 1)
        {
            var explain = trans.transform.Find("explain");
            SetBaseProperty(explain, data, "explain_");
            SetTextProperty(explain, data, "explain_");

            var source = trans.transform.Find("source");
            SetBaseProperty(source, data, "source_");
            SetTextProperty(source, data, "source_");
        }
        if (type == 0)
        {
            var time = trans.transform.Find("time");
            SetBaseProperty(time, data, "system_time_");
            SetTextProperty(time, data, "system_time_");
        }

        var btn = trans.transform.Find("Button");
        SetBaseProperty(btn, data, "button_");
        SetImageProperty(btn, data, "button_");
        btn.GetComponent<Button>().onClick.AddListener(delegate () { StartCoroutine(G3TipsManager(trans.gameObject, 0f)); });

        var btntext = btn.transform.Find("Text");
        SetBaseProperty(btntext, data, "button_text_");
        SetTextProperty(btntext, data, "button_text_");
    }

    private void SetBaseProperty(Transform trans, LitJson.JsonData data, string value)
    {
        string spos = value + "pos";
        string sangles = value + "angles";
        string ssize = value + "size";
        string sscale = value + "scale";
        trans.GetComponent<RectTransform>().anchoredPosition3D =
            new Vector3(JsonToFloat(data[spos][0]), JsonToFloat(data[spos][1]), JsonToFloat(data[spos][2]));
        trans.GetComponent<RectTransform>().eulerAngles =
            new Vector3(JsonToFloat(data[sangles][0]), JsonToFloat(data[sangles][1]), JsonToFloat(data[sangles][2]));
        trans.GetComponent<RectTransform>().sizeDelta =
            new Vector2(JsonToFloat(data[ssize][0]), JsonToFloat(data[ssize][1]));
        trans.GetComponent<RectTransform>().localScale =
            new Vector3(JsonToFloat(data[sscale][0]), JsonToFloat(data[sscale][1]), JsonToFloat(data[sscale][2]));
    }

    private void SetImageProperty(Transform image, LitJson.JsonData data, string value)
    {
        string spath = value + "sprite";
        string scolor = value + "color";
        string ssize = value + "size";
        image.GetComponent<Image>().sprite =
            LoadSprite(new Vector2(JsonToFloat(data[ssize][0]), JsonToFloat(data[ssize][1])), (string)data[spath]);
        image.GetComponent<Image>().color =
            new Color(JsonToFloat(data[scolor][0]), JsonToFloat(data[scolor][1]), JsonToFloat(data[scolor][2]), JsonToFloat(data[scolor][3]));
    }

    private void SetTextProperty(Transform text, LitJson.JsonData data, string value)
    {
        string scolor = value + "color";
        string ssize = value + "font_size";
        string sstyle = value + "font_style";
        string stext = value + "text";
        text.GetComponent<Text>().text = (string)data[stext];
        text.GetComponent<Text>().color =
            new Color(JsonToFloat(data[scolor][0]), JsonToFloat(data[scolor][1]), JsonToFloat(data[scolor][2]), JsonToFloat(data[scolor][3])); ;
        text.GetComponent<Text>().fontSize = (int)data[ssize];
        text.GetComponent<Text>().fontStyle = (FontStyle)(int)data[sstyle];
    }

    private IEnumerator G3TipsManager(GameObject tip, float time)
    {
        yield return new WaitForSeconds(time);
        tip.SetActive(false);
    }

    private float JsonToFloat(LitJson.JsonData data)
    {
        return Convert.ToSingle((string)data);
    }

    private bool SDKManagerInitFPS()
    {
        Transform[] father;
        father = GetComponentsInChildren<Transform>(true);
        GameObject FPS = null;
        foreach (Transform child in father)
        {
            if (child.gameObject.name == "FPS")
            {
                FPS = child.gameObject;
            }
        }
        if (FPS != null)
        {
            if (systemFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            int fps = 0;
#if !UNITY_EDITOR
            int rate = (int)GlobalIntConfigs.iShowFPS;
            Render.UPvr_GetIntConfig(rate, ref fps);
#endif
            if (Convert.ToBoolean(fps))
            {
                FPS.SetActive(true);
                return true;
            }
            if (ShowFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            return false;
        }
        return false;
    }

    private bool SDKManagerInitConfigProfile()
    {
        pvr_UnitySDKConfig = Pvr_UnitySDKConfigProfile.Default;
        return true;
    }

    private bool SDKManagerInitEditor()
    {
        if (pvr_UnitySDKEditor == null)
        {
            pvr_UnitySDKEditor = this.gameObject.AddComponent<Pvr_UnitySDKEditor>();
        }
        else
        {
            pvr_UnitySDKEditor = null;
            pvr_UnitySDKEditor = this.gameObject.AddComponent<Pvr_UnitySDKEditor>();
        }
        return true;
    }

    private bool SDKManagerInitPara()
    {
        return true;
    }

    public void SDKManagerLongHomeKey()
    {
        if (Pvr_UnitySDKSensor.Instance != null)
        {
            if (isHasController)
            {
                if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
                    Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
                {
                    Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(0, 1);
                }
                else
                {
                    Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
                }
            }
            else
            {
                Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
            }

        }
    }

    public Action longPressHomeKeyAction;

    private void setLongHomeKey()
    {
        if (sdk.HmdOnlyrot)
        {
            if (Pvr_UnitySDKSensor.Instance != null)
            {
                PLOG.I(Pvr_UnitySDKSensor.Instance.ResetUnitySDKSensor()
                    ? "Long Home Key to Reset Sensor Success!"
                    : "Long Home Key to Reset Sensor Failed!");
            }
        }
        else
        {
            if (trackingmode == 4 || trackingmode == 5 || trackingmode == 6)
            {
                Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);

            }
            else
            {
                if (trackingmode == 2 || trackingmode == 3)
                {
                    if (isHasController && (Controller.UPvr_GetControllerState(0) == ControllerState.Connected || Controller.UPvr_GetControllerState(1) == ControllerState.Connected))
                    {
                        Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(0, 1);
                    }
                    else
                    {
                        Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
                    }
                }

                if (trackingmode == 0 || trackingmode == 1)
                {
                    Pvr_UnitySDKSensor.Instance.ResetUnitySDKSensor();
                }

            }
            if (longPressHomeKeyAction != null)
            {
                longPressHomeKeyAction();
            }
        }
    }

    public void verifyAPPCallback(string code)
    {
        Debug.Log("PvrLog verifyAPPCallback" + code);
        AppCheckResult = Convert.ToInt32(code);
        if (EntitlementCheckResultEvent != null)
        {
            EntitlementCheckResultEvent(AppCheckResult);
        }  
    }

    public void IpdRefreshCallBack(string ipd)
    {
        Debug.Log("PvrLog IpdRefreshCallBack");
        foreach (var t in Pvr_UnitySDKEyeManager.Instance.Eyes)
        {
            t.RefreshCameraPosition(Convert.ToSingle(ipd));
        }
    }
    /*************************************  Unity API ****************************************/

#region Unity API

    void Awake()
    {
#if ANDROID_DEVICE
        Debug.Log("DISFT Unity Version:" + Application.unityVersion);
        Debug.Log("DISFT Customize NeckOffset:" + neckOffset);
        Debug.Log("DISFT MSAA :" + Pvr_UnitySDKProjectSetting.GetProjectConfig().rtAntiAlising.ToString());
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            Debug.Log("DISFT LWRP = Enable");
        }
        Debug.Log("DISFT Content Proctect :" + Pvr_UnitySDKProjectSetting.GetProjectConfig().usecontentprotect.ToString());
        
        int isrot = 0;
        int rot = 0;
        LoadIsMirroringValue();
        if (!ismirroring)
        {
            rot = (int)GlobalIntConfigs.Enable_Activity_Rotation;
            Render.UPvr_GetIntConfig(rot, ref isrot);
            if (isrot == 1)
            {
                Debug.Log("DISFT ScreenOrientation.Portrait = Enable");
                Screen.orientation = ScreenOrientation.Portrait;
            }
        }
        else
        {
            rot = (int)GlobalIntConfigs.GetDisplay_Orientation;
            Render.UPvr_GetIntConfig(rot, ref isrot);
            Screen.orientation = isrot == 0 ? ScreenOrientation.Portrait : ScreenOrientation.LandscapeLeft;
        }

#endif

#if ANDROID_DEVICE
        var javaVrActivityClass = new AndroidJavaClass("com.psmart.vrlib.VrActivity");
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
        var controllermanager = FindObjectOfType<Pvr_ControllerManager>();
        isHasController = controllermanager != null;
        PLOG.getConfigTraceLevel();

        int enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
        Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
        //setting of fps
        Application.targetFrameRate = 61;
#if ANDROID_DEVICE
        int ability6dof = 0;
        enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
        if (ability6dof == 0)
        {
            SDK.HmdOnlyrot = true;
        }
        int fps = -1;
        int rate = (int) GlobalIntConfigs.TARGET_FRAME_RATE;
        Render.UPvr_GetIntConfig(rate, ref fps);
        float ffps = 0.0f;
        int frame = (int) GlobalFloatConfigs.DISPLAY_REFRESH_RATE;
        Render.UPvr_GetFloatConfig(frame, ref ffps);
        Application.targetFrameRate = fps > 0 ? fps : (int)ffps;

        if (!Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultfps)
        {
            if (Pvr_UnitySDKProjectSetting.GetProjectConfig().customfps <= ffps)
            {
                Application.targetFrameRate = Pvr_UnitySDKProjectSetting.GetProjectConfig().customfps;
            }
            else
            {
                Application.targetFrameRate = (int)ffps;
            }
        }
        Debug.Log("DISFT Customize FPS :" + Application.targetFrameRate);

#endif

        //setting of neck model 
#if ANDROID_DEVICE
        if (!UseCustomNeckPara)
        {
            float neckx = 0.0f;
            float necky = 0.0f;
            float neckz = 0.0f;
            int modelx = (int) GlobalFloatConfigs.NECK_MODEL_X;
            int modely = (int) GlobalFloatConfigs.NECK_MODEL_Y;
            int modelz = (int) GlobalFloatConfigs.NECK_MODEL_Z;
            Render.UPvr_GetFloatConfig(modelx, ref neckx);
            Render.UPvr_GetFloatConfig(modely, ref necky);
            Render.UPvr_GetFloatConfig(modelz, ref neckz);
            if (neckx != 0.0f || necky != 0.0f || neckz != 0.0f)
            {
                neckOffset = new Vector3(neckx, necky, neckz);
            }
        }
#endif
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.iPhoneHMDModeEnabled, ref iPhoneHMDModeEnabled);

        Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForG2;
#if ANDROID_DEVICE
        InitUI();
        RefreshTextByLanguage();
#endif
    }


    //wait for unity to start rendering
    IEnumerator Start()
    {
        if (SDKManagerInit())
        {
            PLOG.I("SDK Init success.");
        }
        else
        {
            PLOG.E("SDK Init Failed.");
            Application.Quit();
        }
        if (Pvr_UnitySDKRender.Instance != null)
        {
            Pvr_UnitySDKRender.Instance.ReInit();
        }
        SDKManagerInitFPS();
        if (Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)
        {
            if (! (PlatformSettings.UPvr_IsCurrentDeviceValid() == Pvr_UnitySDKPlatformSetting.simulationType.Valid))
            {
                Debug.Log("DISFT Entitlement Check Simulation DO NOT PASS");
                string appID = Pvr_UnitySDKPlatformSetting.Instance.appID;
                Debug.Log("DISFT Start-time Entitlement Check Enable");
                PLOG.I("DISFT Start-time Entitlement Check APPID :" + appID);
                // 0:success -1:invalid params -2:service not exist -3:time out
                PlatformSettings.UPvr_AppEntitlementCheckExtra(appID);
            }
            else
            {
                Debug.Log("DISFT Entitlement Check Simulation PASS");
            }
                  
        }
#if UNITY_EDITOR
        yield break;
#else
        yield return StartCoroutine(InitRenderThreadRoutine());
#endif
    }

    IEnumerator InitRenderThreadRoutine()
    {
        PLOG.I("InitRenderThreadRoutine begin");
        for (int i = 0; i < 2; ++i)
        {
            yield return null;
        }
        Debug.Log("InitRenderThreadRoutine after a wait");

        if (Pvr_UnitySDKRender.Instance != null)
        {
            Pvr_UnitySDKRender.Instance.IssueRenderThread();
        }
        else
        {
            Debug.Log("InitRenderThreadRoutine pvr_UnitySDKRender == null");
        }

        Debug.Log("InitRenderThreadRoutine end");
        yield break;
    }


    void Update()
    {
        if (isHasController && iPhoneHMDModeEnabled == 1)
        {
            if (Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.controller0Connected && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                if (!lowControllerpowerstate)
                {
                    StartCoroutine(ToastManager(6, true, 0f));
                    StartCoroutine(ToastManager(6, false, 3.0f));
                    lowControllerpowerstate = true;
                }

                controllerpowershowtime += Time.deltaTime;
                if (controllerpowershowtime >= 3600f)
                {
                    lowControllerpowerstate = false;
                    controllerpowershowtime = 0f;
                }
            }
        }
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                newPicovrTriggered = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            newPicovrTriggered = true;
        }
#if ANDROID_DEVICE
        if (Pvr_UnitySDKSensor.Instance != null)
        {
            Pvr_UnitySDKSensor.Instance.SensorUpdate();
        }
#endif
        picovrTriggered = newPicovrTriggered;
        newPicovrTriggered = false;

    }
    void OnDestroy()
    {
        if (sdk == this)
        {
            sdk = null;
        }
        RenderTexture.active = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForG2;
    }

    private void OnEnable()
    {
        if (sdk == null)
        {
            sdk = this;
        }
        else
        {
            if (sdk != this)
                sdk = this;
        }
    }
    void OnDisable()
    {
#if UNITY_EDITOR
        if (pvr_UnitySDKEditor != null)
        {
            pvr_UnitySDKEditor = null;
        }
#endif
        StopAllCoroutines();
    }

    private void OnPause()
    {
        Pvr_UnitySDKAPI.System.UPvr_StopHomeKeyReceiver();
        this.LeaveVRMode();
        if (Pvr_UnitySDKSensor.Instance != null)
        {
            Pvr_UnitySDKSensor.Instance.StopUnitySDKSensor();
        }
    }

    private void OnApplicationPause(bool pause)
    {
		bool unityPause = pause;
        Debug.Log("OnApplicationPause-------------------------" + (unityPause ? "true" : "false"));
       
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity() && !Pvr_UnitySDKRender.Instance.isShellMode)
        {
            bool state = Pvr_UnitySDKAPI.System.UPvr_GetMainActivityPauseStatus();
            Debug.Log("OnApplicationPause-------------------------Activity Pause State:" + state);
            pause = state;
        }
		if(unityPause == pause)
        {
			if (pause)
			{ 
				onResume = false;
				OnPause();
			}
			else
			{             
				onResume = true;
				GL.InvalidateState();
				StartCoroutine(OnResume());
			}
		}
		else
		{
			if (pause)
			{ 
				Debug.Log("OnApplicationPause-------------------------Activity pause Unity resume");
				GL.InvalidateState();
				StartCoroutine(OnResume());
				onResume = false;
				OnPause();
			}
			else
			{    
				Debug.Log("OnApplicationPause-------------------------Activity resume Unity pause");		
				OnPause();		
				onResume = true;
				GL.InvalidateState();
				StartCoroutine(OnResume());
			}
		}
#endif

        
       
    }

    public void EnterVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Resume);
        this.isEnterVRMode = true;
        if (eventEnterVRMode != null)
        {
            eventEnterVRMode();
        }
    }

    public void LeaveVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Pause);
        this.isEnterVRMode = false;
    }

    public void SixDofForceQuit()
    {
        Application.Quit();
    }

    private void InitUI()
    {
        if (iPhoneHMDModeEnabled == 1)
        {
            var flamingo2Tips = Instantiate(Resources.Load("Prefabs/flamingo2Tips") as GameObject, transform.Find("Head"), false).transform;
            calltoast = flamingo2Tips.Find("Call").gameObject;
            msgtoast = flamingo2Tips.Find("Msg").gameObject;
            lowhmdBatterytoast = flamingo2Tips.Find("LowHmdBattery").gameObject;
            lowphoneBatterytoast = flamingo2Tips.Find("LowPhoneBattery").gameObject;
            LowPhoneHealthtoast = flamingo2Tips.Find("LowPhoneHealth").gameObject;
            LowcontrollerBatterytoast = flamingo2Tips.Find("LowControllerBattery").gameObject;
        }
    }

    private void RefreshTextByLanguage()
    {
        if (msgtoast != null)
        {
            msgtoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("msgtoast0");
            msgtoast.transform.Find("string").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("msgtoast1");
            calltoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("calltoast0");
            calltoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("calltoast1");
            lowhmdBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("lowhmdBatterytoast");
            lowphoneBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("lowphoneBatterytoast");
            LowPhoneHealthtoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("LowPhoneHealthtoast");
            LowcontrollerBatterytoast.transform.Find("Text").GetComponent<Text>().text = Pvr_UnitySDKAPI.System.UPvr_GetLangString("LowcontrollerBatterytoast");
        }
    }

    private void LoadIsMirroringValue()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManagerObj = jo.Call<AndroidJavaObject>("getPackageManager");
        string packageName = jo.Call<string>("getPackageName");
        AndroidJavaObject applicationInfoObj = packageManagerObj.Call<AndroidJavaObject>("getApplicationInfo", packageName, 128);
        AndroidJavaObject bundleObj = applicationInfoObj.Get<AndroidJavaObject>("metaData");
        ismirroring = Convert.ToBoolean(bundleObj.Call<int>("getInt", "bypass_presentation",0));
    }
#endregion

    /************************************    IEnumerator  *************************************/
    private IEnumerator OnResume()
    {
        int ability6dof = 0;
        int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.ABILITY6DOF;
        Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref ability6dof);
        
        RefreshTextByLanguage();
        if (Pvr_UnitySDKSensor.Instance != null)
        {
            Pvr_UnitySDKSensor.Instance.StartUnitySDKSensor();

            int iEnable6Dof = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
            int iEnable6DofGlobalTracking = (int) GlobalIntConfigs.ENBLE_6DOF_GLOBAL_TRACKING;
            Render.UPvr_GetIntConfig(iEnable6DofGlobalTracking, ref iEnable6Dof);
#endif
            if (iEnable6Dof != 1)
            {
                int sensormode = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
                int isensormode = (int) GlobalIntConfigs.SensorMode;
                Render.UPvr_GetIntConfig(isensormode, ref sensormode);
#endif
                if (sensormode != 8)
                {
                    Pvr_UnitySDKSensor.Instance.ResetUnitySDKSensor();
                }
            }
        }

        if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
        {
            bool setMonoPresentation = Pvr_UnitySDKAPI.System.UPvr_SetMonoPresentation();
            PLOG.I("onresume set monoPresentation success ?-------------" + setMonoPresentation.ToString());

            bool isPresentationExisted = Pvr_UnitySDKAPI.System.UPvr_IsPresentationExisted();
            PLOG.I("onresume presentation existed ?-------------" + isPresentationExisted.ToString());
        }

        for (int i = 0; i < Pvr_UnitySDKEyeManager.Instance.Eyes.Length; i++)
        {
            Pvr_UnitySDKEyeManager.Instance.Eyes[i].RefreshCameraPosition(Pvr_UnitySDKAPI.System.UPvr_GetIPD());
        }

        var waitNum = 15;
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.GetWaitFrameNum, ref waitNum);
        var resetNum = 10;
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.GetResetFrameNum, ref resetNum);

        for (int i = 0; i < waitNum; i++)
        {
            if (i == resetNum)
            {
                if (ResetTrackerOnLoad && ability6dof == 1)
                {
                    Debug.Log("Reset Tracker OnLoad");
                    Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
                }
            }
            yield return null;
        }
        
        this.EnterVRMode();
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);
        Pvr_UnitySDKEye.setLevel = false;
        if (longPressHomeKeyAction != null)
        {
            longPressHomeKeyAction();
        }
        if ( Pvr_UnitySDKAPI.Render.UPvr_GetIntSysProc("pvrsist.foveation.level",ref SystemDebugFFRLevel) ) 
        {
            Pvr_UnitySDKAPI.Render.SetFoveatedRenderingLevel((EFoveationLevel)(SystemDebugFFRLevel));
            Debug.Log("DISFT OnResume Get System Debug ffr level is : " + SystemDebugFFRLevel);
        }
        else
        {
            Debug.Log("DISFT OnResume Get System Debug ffr level Error,ffr level is : " + SystemDebugFFRLevel);
        }
            
        if (SystemDebugFFRLevel == -1)
        {
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig((int)GlobalIntConfigs.EnableFFRBYSYS, ref SystemFFRLevel);
            if (SystemFFRLevel != -1)
            {
                Pvr_UnitySDKAPI.Render.SetFoveatedRenderingLevel((EFoveationLevel)(SystemFFRLevel));
                Debug.Log("DISFT OnResume Get System ffr level is : " + SystemFFRLevel);
            }
        } 
        
    }
}
