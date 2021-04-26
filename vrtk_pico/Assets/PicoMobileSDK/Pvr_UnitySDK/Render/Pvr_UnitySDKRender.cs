// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using UnityEngine;
using Pvr_UnitySDKAPI;
using UnityEngine.Rendering;

public class Pvr_UnitySDKRender
{
    private static Pvr_UnitySDKRender instance = null;
    public static Pvr_UnitySDKRender Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Pvr_UnitySDKRender();
            }
            return instance;

        }
        set { instance = value; }
    }

    public Pvr_UnitySDKRender()
    {
        if (!canConnecttoActivity)
        {
            ConnectToAndriod();
            PLOG.I("PvrLog Init Render Ability Success!");
            isInitrenderThread = false;
        }
        Init();
    }

    /************************************    Properties  *************************************/
    #region Properties
#if ANDROID_DEVICE
    public AndroidJavaObject activity;
    public static AndroidJavaClass javaVrActivityClass;
    public static AndroidJavaClass javaSysActivityClass;  
    public static AndroidJavaClass javaserviceClass;
	public static AndroidJavaClass javaVrActivityLongReceiver;
    public static AndroidJavaClass javaVrActivityClientClass;
#endif

    private bool canConnecttoActivity = false;
    private bool isInitrenderThread = true;
    private string model;
    private Vector2 prefinger1 = new Vector2(0.0f, 0.0f);
    private Vector2 prefinger2 = new Vector2(0.0f, 0.0f);

    public int eyeTextureCount = 6;
    public RenderTexture[] eyeTextures;
    public int[] eyeTextureIds;
    public int currEyeTextureIdx = 0;
    public int nextEyeTextureIdx = 1;
    public int lastEyeTextureIdx = 0;
    public bool isSwitchSDK = false;
    public int RenderviewNumber = 0;
    public bool isFirstStartup = true;
    public bool isShellMode = false;
    private StereoRenderingPathPico stereoRenderPath = StereoRenderingPathPico.MultiPass;
    public StereoRenderingPathPico StereoRenderPath
    {
        get
        {
            return stereoRenderPath;
        }
    }
    public SDKStereoRendering StereoRendering { get; private set; }

    private float rtScaleFactor = 1;
    public float RtScaleFactor
    {
        get
        {
            return rtScaleFactor;
        }
        set
        {
            if (value != rtScaleFactor)
            {
                rtScaleFactor = value;
                ReCreateEyeBuffer();
            }
        }
    }

    private float eyeVFov = 90.0f;
    public float EyeVFoV
    {
        get
        {
            return eyeVFov;
        }
        set
        {
            if (value != eyeVFov)
            {
                eyeVFov = value;
            }
        }
    }

    private float eyeHFov = 90.0f;
    public float EyeHFoV
    {
        get
        {
            return eyeHFov;
        }
        set
        {
            if (value != eyeHFov)
            {
                eyeHFov = value;
            }
        }
    }

    #endregion

    /************************************   Public Interfaces **********************************/
    #region       PublicInterfaces

    public void ConnectToAndriod()
    {
#if ANDROID_DEVICE
        try
        {      
            Debug.Log("PvrLog SDK Version :  " + Pvr_UnitySDKAPI.System.UPvr_GetSDKVersion().ToString() + "  Unity Script Version :" +  Pvr_UnitySDKAPI.System.UPvr_GetUnitySDKVersion().ToString());
            UnityEngine.AndroidJavaClass unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            javaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaserviceClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.UnityClient");
			javaVrActivityLongReceiver = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.HomeKeyReceiver");
            javaSysActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.SysActivity");
            javaVrActivityClientClass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.PvrClient");
			Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaVrActivityClass.GetRawClass());
            model = javaVrActivityClass.CallStatic<string>("Pvr_GetBuildModel");

            double[] parameters = new double[5];
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref parameters, javaVrActivityClass, "getDPIParameters", activity);
            int platformType = -1 ;
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.PLATFORM_TYPE;
            Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex,ref platformType);

            string systemfps = "";
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref systemfps, javaserviceClass, "getSysproc", "persist.pvr.debug.appfps");
            if(systemfps != "")
                Pvr_UnitySDKManager.SDK.systemFPS = Convert.ToBoolean(Convert.ToInt16(systemfps));
        
            if (platformType == 0)
            {
                 Pvr_UnitySDKAPI.Render.UPvr_ChangeScreenParameters(model, (int)parameters[0], (int)parameters[1], parameters[2], parameters[3], parameters[4]);				 
				 Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
            {
                bool setMonoPresentation = Pvr_UnitySDKAPI.System.UPvr_SetMonoPresentation();
                Debug.Log("ConnectToAndriod set monoPresentation success ?-------------" + setMonoPresentation.ToString());

                bool isPresentationExisted = Pvr_UnitySDKAPI.System.UPvr_IsPresentationExisted();
                Debug.Log("ConnectToAndriod presentation existed ?-------------" + isPresentationExisted.ToString());
            }
            isShellMode = GetIsShellMode();
            Debug.Log("ConnectToAndriod isShellMode ?-------------" + isShellMode.ToString());
        }
        catch (AndroidJavaException e)
        {
            PLOG.E("ConnectToAndriod--catch" + e.Message);
        }
#endif
        canConnecttoActivity = true;
    }

    public void Init()
    {
#if ANDROID_DEVICE
        if (InitRenderAbility())
        {
            Debug.Log("PvrLog Init Render Ability Success!");
            isInitrenderThread = false;
        }
        else
            Debug.Log("PvrLog Init Render Ability Failed!");
#endif
    }

    /************************************  Private Interfaces **********************************/
    #region     Private Interfaces

    private bool InitRenderAbility()
    {
        if (UpdateRenderParaFrame())
        {
            if (CreateEyeBuffer())
            {
                float separation = Pvr_UnitySDKAPI.System.UPvr_GetIPD();
                Pvr_UnitySDKManager.SDK.leftEyeOffset = new Vector3(-separation / 2, 0, 0);
                Pvr_UnitySDKManager.SDK.rightEyeOffset = new Vector3(separation / 2, 0, 0);
                return true;
            }
        }
        return false;
    }

    private bool UpdateRenderParaFrame()
    {
        EyeVFoV = GetEyeVFOV();
        EyeHFoV = GetEyeHFOV();
        Pvr_UnitySDKManager.SDK.EyesAspect = EyeHFoV / EyeVFoV;
        return true;
    }

    private bool CreateEyeBuffer()
    {
        Vector2 resolution = GetEyeBufferResolution();
        if (isFirstStartup)
        {
            InitSinglePass();
            Pvr_UnitySDKAPI.System.UPvr_SetSinglePassDepthBufferWidthHeight((int)resolution.x, (int)resolution.y);
        }
        eyeTextures = new RenderTexture[eyeTextureCount];
        eyeTextureIds = new int[eyeTextureCount];

        // eye buffer
        for (int i = 0; i < eyeTextureCount; i++)
        {
            if (null == eyeTextures[i])
            {
                try
                {
                    ConfigureEyeBuffer(i, resolution);
                }
                catch (Exception e)
                {
                    PLOG.E("ConfigureEyeBuffer ERROR " + e.Message);
                    throw;
                }
            }

            if (!eyeTextures[i].IsCreated())
            {
                eyeTextures[i].Create();
                eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
            eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
        }
        return true;
    }

    private void InitSinglePass()
    {
#if ANDROID_DEVICE
        bool supportSinglePass = true;
#if UNITY_2018_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            supportSinglePass = false;
            if (!Pvr_UnitySDKAPI.BoundarySystem.UPvr_EnableLWRP(true))
            {
                Debug.Log("UPvr_EnableLWRP return false");
            }
            Vector2 resolution = GetEyeBufferResolution();
            if (!Pvr_UnitySDKAPI.BoundarySystem.UPvr_SetViewportSize((int)resolution.x, (int)resolution.y))
            {
                Debug.Log("UPvr_SetViewportSize return false");
            }
        }

#endif
        if (Pvr_UnitySDKProjectSetting.GetProjectConfig().usesinglepass)
        {
            bool result = false;
            if (supportSinglePass)
            {
                result = Pvr_UnitySDKAPI.System.UPvr_EnableSinglePass(true);
            }
            if (result)
            {
                StereoRendering = new Pvr_UnitySDKSinglePass();
                stereoRenderPath = StereoRenderingPathPico.SinglePass;
                eyeTextureCount = 3;
            }
            Debug.Log("EnableSinglePass supportSinglePass " + supportSinglePass.ToString() + " result " + result);
        }
#endif
    }

    public float GetEyeVFOV()
    {
        float fov = 102;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.VFOV;
            Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref fov);
            if (fov <= 0)
            {
                fov = 102;
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("GetEyeVFOV ERROR! " + e.Message);
            throw;
        }

        return fov;
    }

    public float GetEyeHFOV()
    {
        float fov = 102;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalFloatConfigs.HFOV;
            Pvr_UnitySDKAPI.Render.UPvr_GetFloatConfig(enumindex, ref fov);
            if (fov <= 0)
            {
                fov = 102;
            }
        }
        catch (System.Exception e)
        {
            PLOG.E("GetEyeHFOV ERROR! " + e.Message);
            throw;
        }

        return fov;
    }

    private void ConfigureEyeBuffer(int eyeTextureIndex, Vector2 resolution)
    {
        int x = (int)resolution.x;
        int y = (int)resolution.y;
        eyeTextures[eyeTextureIndex] = new RenderTexture(x, y, (int)Pvr_UnitySDKProjectSetting.GetProjectConfig().rtBitDepth, Pvr_UnitySDKProjectSetting.GetProjectConfig().rtFormat);

        if (StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            eyeTextures[eyeTextureIndex].anisoLevel = 0;
            eyeTextures[eyeTextureIndex].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)Pvr_UnitySDKProjectSetting.GetProjectConfig().rtAntiAlising);
            Debug.Log("MultiPass ConfigureEyeBuffer eyeTextureIndex " + eyeTextureIndex);
        }
        else if (StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            eyeTextures[eyeTextureIndex].useMipMap = false;
            eyeTextures[eyeTextureIndex].wrapMode = TextureWrapMode.Clamp;
            eyeTextures[eyeTextureIndex].filterMode = FilterMode.Bilinear;
            eyeTextures[eyeTextureIndex].anisoLevel = 1;
            eyeTextures[eyeTextureIndex].dimension = TextureDimension.Tex2DArray;
            eyeTextures[eyeTextureIndex].volumeDepth = 2;
            Debug.Log("SinglePass ConfigureEyeBuffer eyeTextureIndex " + eyeTextureIndex);
        }
        eyeTextures[eyeTextureIndex].Create();
        if (eyeTextures[eyeTextureIndex].IsCreated())
        {
            eyeTextureIds[eyeTextureIndex] = eyeTextures[eyeTextureIndex].GetNativeTexturePtr().ToInt32();
            Debug.Log("eyeTextureIndex : " + eyeTextureIndex.ToString());
        }

    }

    public bool ReCreateEyeBuffer()
    {
        if (!Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultRenderTexture)
        {
            for (int i = 0; i < eyeTextureCount; i++)
            {
                if (eyeTextures[i] != null)
                {
                    eyeTextures[i].Release();
                }
            }

            Array.Clear(eyeTextures, 0, eyeTextures.Length);

            return CreateEyeBuffer();
        }

        return false;
    }

    #endregion

    public void ReInit()
    {
        if (canConnecttoActivity && isInitrenderThread)
        {
            Init();
        }
    }

    public void IssueRenderThread()
    {
        if (canConnecttoActivity && !isInitrenderThread)
        {
            ColorSpace colorSpace = QualitySettings.activeColorSpace;
            if (colorSpace == ColorSpace.Gamma)
            {
                Pvr_UnitySDKAPI.Render.UPvr_SetColorspaceType(0);
            }
            else if (colorSpace == ColorSpace.Linear)
            {
                Pvr_UnitySDKAPI.Render.UPvr_SetColorspaceType(1);
            }
            
            //for casting color space , using Gamma set  0, Linear ---- 1
            //Pvr_UnitySDKAPI.Render.UPvr_SetCastingColorspaceType(0);
            
            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.InitRenderThread);
            isInitrenderThread = true;
            if (StereoRendering != null)
            {
                StereoRendering.OnSDKRenderInited();
            }
            Debug.Log("PvrLog IssueRenderThread end");
        }
        else
        {
            PLOG.I("PvrLog IssueRenderThread  canConnecttoActivity = " + canConnecttoActivity);
        }
    }

    private void AutoAdpatForPico1s()
    {
        Vector2 finger1 = Input.touches[0].position;
        Vector2 finger2 = Input.touches[1].position;
        if (Vector2.Distance(prefinger1, finger1) > 2.0f && Vector2.Distance(prefinger2, finger2) > 2.0f)
        {
            float x = (Input.touches[0].position.x + Input.touches[1].position.x) / Screen.width - 1.0f;
            float y = (Input.touches[0].position.y + Input.touches[1].position.y) / Screen.height - 1.0f;
            Pvr_UnitySDKAPI.Render.UPvr_SetRatio(x, y);
        }
        prefinger1 = finger1;
        prefinger2 = finger2;
    }

    public Vector2 GetEyeBufferResolution()
    {
        Vector2 eyeBufferResolution;
        int w = 1024;
        int h = 1024;
        if (Pvr_UnitySDKProjectSetting.GetProjectConfig().usedefaultRenderTexture)
        {
            try
            {
                int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION0;
                Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref w);
                enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.EYE_TEXTURE_RESOLUTION1;
                Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref h);
            }
            catch (System.Exception e)
            {
                PLOG.E("GetEyeBufferResolution ERROR! " + e.Message);
                throw;
            }
        }
        else
        {
            w = (int)(Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize.x * RtScaleFactor);
            h = (int)(Pvr_UnitySDKProjectSetting.GetProjectConfig().customRTSize.y * RtScaleFactor);
        }

        eyeBufferResolution = new Vector2(w, h);
        Debug.Log("DISFT Customize RenderTexture:" + eyeBufferResolution + ", scaleFactor: " + RtScaleFactor);

        return eyeBufferResolution;
    }

    public bool GetUsePredictedMatrix()
    {
        return true;
    }


    public bool GetIsShellMode()
    {
#if ANDROID_DEVICE
        if (null == activity )
        {
            return false;
        }
        AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
        using (AndroidJavaObject applicationInfo =
            packageManager.Call<AndroidJavaObject>("getApplicationInfo",  activity.Call<string>("getPackageName"),0x00000080))
        {
            var metaData = applicationInfo.Get<AndroidJavaObject>("metaData");
            if (metaData != null)
            {
                int shellModeValue = 0;
                shellModeValue  = metaData.Call<int>("getInt", "shell_mode");
                if (shellModeValue == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
             
        }
#endif
        return false;
    }
    
    #endregion
}
