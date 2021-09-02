// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;

public class Pvr_UnitySDKEyeManager : MonoBehaviour
{
    private static Pvr_UnitySDKEyeManager instance;
    public static Pvr_UnitySDKEyeManager Instance
    {
        get
        {
            if (instance == null)
            {
                PLOG.E("Pvr_UnitySDKEyeManager instance is not init yet...");
            }
            return instance;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    /// <summary>
    /// Eyebuffer Layers
    /// </summary>
    private Pvr_UnitySDKEye[] eyes = null;
    public Pvr_UnitySDKEye[] Eyes
    {
        get
        {
            if (eyes == null)
            {
                eyes = Pvr_UnitySDKEye.Instances.ToArray();
            }
            return eyes;
        }
    }

    [HideInInspector]
    public Camera LeftEyeCamera;
    [HideInInspector]
    public Camera RightEyeCamera;
    /// <summary>
    /// Mono Camera(only enable when Monoscopic switch on)
    /// </summary>
	[HideInInspector]
    public Camera MonoEyeCamera;
    [HideInInspector]
    public Camera BothEyeCamera;
    /// <summary>
    /// Mono Eye RTexture ID
    /// </summary>
    private int MonoEyeTextureID = 0;

    // wait for a number of frames, because custom splash screen(2D loading) need display time when first start-up.
    private readonly int WaitSplashScreenFrames = 3;
    private int frameNum = 0;

    [SerializeField]
    [HideInInspector]
    private bool foveatedRendering;
    [HideInInspector]
    public bool FoveatedRendering
    {
        get
        {
            return foveatedRendering;
        }

        set
        {
            if (value != foveatedRendering)
            {
                foveatedRendering = value;
                if (Application.isPlaying)
                {
                    Pvr_UnitySDKAPI.Render.UPvr_EnableFoveation(true);
                    if (!foveatedRendering)
                    {
                        Pvr_UnitySDKAPI.Render.SetFoveatedRenderingLevel((EFoveationLevel)(-1));
                    }
                }
            }
            
        }
    }


    [SerializeField]
    [HideInInspector]
    private EFoveationLevel foveationLevel = EFoveationLevel.Low;
    [HideInInspector]
    public EFoveationLevel FoveationLevel
    {
        get
        {
            return foveationLevel;
        }
        set
        {
            if (value != foveationLevel)
            {
                foveationLevel = value;
            }
        }
    }
    #endregion

    /************************************ Process Interface  *********************************/
    #region  Process Interface
    private void SetCameraEnableEditor()
    {
        MonoEyeCamera.enabled = !Pvr_UnitySDKManager.SDK.VRModeEnabled || Pvr_UnitySDKManager.SDK.Monoscopic;
        for (int i = 0; i < Eyes.Length; i++)
        {
            if (Eyes[i].eyeSide == Eye.LeftEye || Eyes[i].eyeSide == Eye.RightEye)
            {
                Eyes[i].eyecamera.enabled = Pvr_UnitySDKManager.SDK.VRModeEnabled;
            }
            else if (Eyes[i].eyeSide == Eye.BothEye)
            {
                Eyes[i].eyecamera.enabled = false;
            }
        }
    }

    private void SetCamerasEnableByStereoRendering()
    {
        MonoEyeCamera.enabled = Pvr_UnitySDKManager.SDK.Monoscopic && Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.MultiPass;
    }

    private void SetupMonoCamera()
    {
        transform.localPosition = Vector3.zero;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void SetupUpdate()
    {
        MonoEyeCamera.fieldOfView = Pvr_UnitySDKRender.Instance.EyeVFoV;
        MonoEyeCamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        MonoEyeTextureID = Pvr_UnitySDKRender.Instance.currEyeTextureIdx;
    }

    private void MonoEyeRender()
    {
        SetupUpdate();
#if !UNITY_EDITOR
        if (Pvr_UnitySDKRender.Instance.eyeTextures[MonoEyeTextureID] != null)
        {
            Pvr_UnitySDKRender.Instance.eyeTextures[MonoEyeTextureID].DiscardContents();
            MonoEyeCamera.targetTexture = Pvr_UnitySDKRender.Instance.eyeTextures[MonoEyeTextureID];
        }
#endif
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    private void Awake()
    {
        if (this.MonoEyeCamera == null)
        {
            this.MonoEyeCamera = this.GetComponent<Camera>();
        }
        if (this.LeftEyeCamera == null)
        {
            this.LeftEyeCamera = this.gameObject.transform.Find("LeftEye").GetComponent<Camera>();
        }
        if (this.RightEyeCamera == null)
        {
            this.RightEyeCamera = this.gameObject.transform.Find("RightEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera == null)
        {
            this.BothEyeCamera = this.gameObject.transform.Find("BothEye").GetComponent<Camera>();
        }
        if (this.BothEyeCamera != null)
        {
            this.BothEyeCamera.transform.GetComponent<Pvr_UnitySDKEye>().eyeSide = Eye.BothEye;
        }

        //screen fade
        CreateFadeMesh();
        SetCurrentAlpha(0);
       
        // FFR
        Pvr_UnitySDKAPI.Render.UPvr_EnableFoveation(true);
        if (foveatedRendering)
        {
            Pvr_UnitySDKAPI.Render.SetFoveatedRenderingLevel(this.foveationLevel);
        }
        else
        {
            Pvr_UnitySDKAPI.Render.SetFoveatedRenderingLevel((EFoveationLevel)(-1));
        }

        Pvr_UnitySDKManager.eventEnterVRMode += SetEyeTrackingMode;
    }

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
                instance = this;
        }
        if (Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            Pvr_UnitySDKRender.Instance.StereoRendering.InitEye(BothEyeCamera);
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        foreach (var t in Pvr_UnitySDKEyeOverlay.Instances)
        {
            t.RefreshCamera();
            if (t.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
            {
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Cylinder)
                {
                    Debug.Log("DISFT Cylinder OverLay = Enable");
                }
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Equirect)
                {
                    Debug.Log("DISFT 360 OverLay= Enable");
                }
                if (t.overlayShape ==
                    Pvr_UnitySDKEyeOverlay.OverlayShape.Quad)
                {
                    Debug.Log("DISFT 2D OverLay= Enable");
                }
            }
            if (t.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
            {
                Debug.Log("DISFT UnderLay= Enable");
            }
        }
#endif
        GfxDeviceAdvanceFrameGLES();
        StartCoroutine("EndOfFrame");

        if (screenFade)
        {
            StartCoroutine(ScreenFade(1, 0));
        }
    }

    void Start()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        SetCamerasEnableByStereoRendering();
        SetupMonoCamera();
#endif

#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        SetCameraEnableEditor();
#endif
        if (Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            for (int i = 0; i < Eyes.Length; i++)
            {
                if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide == Eye.BothEye)
                {
                    Eyes[i].EyeRender();
                }
            }
        }
        if (Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
            {
                // Open Stero Eye Render
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (Eyes[i].isActiveAndEnabled && Eyes[i].eyeSide != Eye.BothEye)
                    {
                        Eyes[i].EyeRender();
                    }
                }
            }
            else
            {
                // Open Mono Eye Render
                MonoEyeRender();
            }
        }
    }

    private void OnPause()
    {
        Pvr_UnitySDKManager.eventEnterVRMode -= SetEyeTrackingMode;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        //screen fade
        DestoryFadeMesh();
    }

    private void OnPostRender()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        long eventdata = Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(Pvr_UnitySDKRender.Instance.eyeTextureIds[Pvr_UnitySDKRender.Instance.currEyeTextureIdx]);

        // eyebuffer
        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.LeftEyeEndFrame);

        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(eventdata);
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.RightEyeEndFrame);
#endif
    }
    #endregion

    /************************************  End Of Per Frame  *************************************/
    // for eyebuffer params
    private int eyeTextureId = 0;
    private RenderEventType eventType = RenderEventType.LeftEyeEndFrame;

    private Pvr_UnitySDKEyeOverlay compositeLayer;
    private int overlayLayerDepth = 1;
    private int underlayLayerDepth = 0;
    private bool isHeadLocked = false;
    private int layerFlags = 0;

    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
#if !UNITY_EDITOR
            if (!Pvr_UnitySDKManager.SDK.isEnterVRMode)
            {
                // Call GL.clear before Enter VRMode to avoid unexpected graph breaking.
                GL.Clear(false, true, Color.black);
            }
            if (Pvr_UnitySDKRender.Instance.isFirstStartup && frameNum == this.WaitSplashScreenFrames)
            {
                Pvr_UnitySDKAPI.System.UPvr_RemovePlatformLogo();
                if (Pvr_UnitySDKManager.SDK.ResetTrackerOnLoad)
                {
                    Debug.Log("Reset Tracker OnLoad");
                    Pvr_UnitySDKSensor.Instance.OptionalResetUnitySDKSensor(1, 1);
                }

                Pvr_UnitySDKAPI.System.UPvr_StartVRModel();
                Pvr_UnitySDKRender.Instance.isFirstStartup = false;
            }
            else if (Pvr_UnitySDKRender.Instance.isFirstStartup && frameNum < this.WaitSplashScreenFrames)
            {
                PLOG.I("frameNum:" + frameNum);
                frameNum++;
            }

            #region Eyebuffer
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (!Eyes[i].isActiveAndEnabled || !Eyes[i].eyecamera.enabled)
                    {
                        continue;
                    }

                    switch (Eyes[i].eyeSide)
                    {
                        case Pvr_UnitySDKAPI.Eye.LeftEye:
                            eyeTextureId = Pvr_UnitySDKRender.Instance.eyeTextureIds[Pvr_UnitySDKRender.Instance.currEyeTextureIdx];
                            eventType = RenderEventType.LeftEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.RightEye:
                            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
                            {
                                eyeTextureId = Pvr_UnitySDKRender.Instance.eyeTextureIds[Pvr_UnitySDKRender.Instance.currEyeTextureIdx + 3];
                            }
                            else
                            {
                                eyeTextureId = Pvr_UnitySDKRender.Instance.eyeTextureIds[Pvr_UnitySDKRender.Instance.currEyeTextureIdx];
                            }
                            eventType = RenderEventType.RightEyeEndFrame;
                            break;
                        case Pvr_UnitySDKAPI.Eye.BothEye:
                            eyeTextureId = Pvr_UnitySDKRender.Instance.eyeTextureIds[Pvr_UnitySDKRender.Instance.currEyeTextureIdx];
                            eventType = RenderEventType.BothEyeEndFrame;
                            break;
                        default:
                            break;
                    }

                    // eyebuffer
                    Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(eyeTextureId)); ;
                    Pvr_UnitySDKPluginEvent.Issue(eventType);

                    Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
                }
            }
#endif

            #endregion
#endif

            // Composite Layers: if find Overlay then Open Composite Layers feature
            #region Composite Layers
            int boundaryState = BoundarySystem.UPvr_GetSeeThroughState();
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0 && boundaryState != 2)
            {
                overlayLayerDepth = 1;
                underlayLayerDepth = 0;

                Pvr_UnitySDKEyeOverlay.Instances.Sort();

                for (int i = 0; i < Pvr_UnitySDKEyeOverlay.Instances.Count; i++)
                {
                    compositeLayer = Pvr_UnitySDKEyeOverlay.Instances[i];
                    if (!compositeLayer.isActiveAndEnabled) continue;
                    if (compositeLayer.layerTextures[0] == null && compositeLayer.layerTextures[1] == null && !compositeLayer.isExternalAndroidSurface) continue;
                    if (compositeLayer.layerTransform != null && !compositeLayer.layerTransform.gameObject.activeSelf) continue;

                    layerFlags = 0;

                    if (compositeLayer.overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Quad || compositeLayer.overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Cylinder)
                    {
                        if (compositeLayer.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
                        {
                            isHeadLocked = false;
                            if (compositeLayer.layerTransform != null && compositeLayer.layerTransform.parent == this.transform)
                            {
                                isHeadLocked = true;
                            }

                            // external surface
                            if (compositeLayer.isExternalAndroidSurface)
                            {
                                layerFlags = layerFlags | 0x1;
                                this.CreateExternalSurface(compositeLayer, overlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)compositeLayer.overlayType, (int)compositeLayer.overlayShape, compositeLayer.layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, overlayLayerDepth, isHeadLocked, layerFlags, compositeLayer.MVMatrixs[0],
                            compositeLayer.ModelScales[0], compositeLayer.ModelRotations[0], compositeLayer.ModelTranslations[0], compositeLayer.CameraRotations[0], compositeLayer.CameraTranslations[0], compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)compositeLayer.overlayType, (int)compositeLayer.overlayShape, compositeLayer.layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, overlayLayerDepth, isHeadLocked, layerFlags, compositeLayer.MVMatrixs[1],
                            compositeLayer.ModelScales[1], compositeLayer.ModelRotations[1], compositeLayer.ModelTranslations[1], compositeLayer.CameraRotations[1], compositeLayer.CameraTranslations[1], compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());

                            overlayLayerDepth++;
                        }
                        else if (compositeLayer.overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
                        {
                            // external surface
                            if (compositeLayer.isExternalAndroidSurface)
                            {
                                layerFlags = layerFlags | 0x1;
                                this.CreateExternalSurface(compositeLayer, underlayLayerDepth);
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)compositeLayer.overlayType, (int)compositeLayer.overlayShape, compositeLayer.layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, underlayLayerDepth, false, layerFlags, compositeLayer.MVMatrixs[0],
                            compositeLayer.ModelScales[0], compositeLayer.ModelRotations[0], compositeLayer.ModelTranslations[0], compositeLayer.CameraRotations[0], compositeLayer.CameraTranslations[0], compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)compositeLayer.overlayType, (int)compositeLayer.overlayShape, compositeLayer.layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, underlayLayerDepth, false, layerFlags, compositeLayer.MVMatrixs[1],
                            compositeLayer.ModelScales[1], compositeLayer.ModelRotations[1], compositeLayer.ModelTranslations[1], compositeLayer.CameraRotations[1], compositeLayer.CameraTranslations[1], compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());

                            underlayLayerDepth++;
                        }
                    }
                    else if (compositeLayer.overlayShape == Pvr_UnitySDKEyeOverlay.OverlayShape.Equirect)
                    {
                        // external surface
                        if (compositeLayer.isExternalAndroidSurface)
                        {
                            layerFlags = layerFlags | 0x1;
                            this.CreateExternalSurface(compositeLayer, 0);
                        }

                        // 360 Overlay Equirectangular Texture
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.LeftEye, compositeLayer.layerTextureIds[0], (int)compositeLayer.overlayShape, layerFlags, compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.RightEye, compositeLayer.layerTextureIds[1], (int)compositeLayer.overlayShape, layerFlags, compositeLayer.GetLayerColorScale(), compositeLayer.GetLayerColorOffset());
                    }
                }
                #endregion
            }

            // Begin TimeWarp
            //Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(0));
            Pvr_UnitySDKPluginEvent.Issue(RenderEventType.TimeWarp);
            Pvr_UnitySDKRender.Instance.currEyeTextureIdx = Pvr_UnitySDKRender.Instance.nextEyeTextureIdx;
            Pvr_UnitySDKRender.Instance.nextEyeTextureIdx = (Pvr_UnitySDKRender.Instance.nextEyeTextureIdx + 1) % 3;

        }
    }

    /// <summary>
    /// Create External Surface
    /// </summary>
    /// <param name="overlayInstance"></param>
    /// <param name="layerDepth"></param>
    private void CreateExternalSurface(Pvr_UnitySDKEyeOverlay overlayInstance, int layerDepth)
    {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        if (overlayInstance.externalAndroidSurfaceObject == System.IntPtr.Zero)
        {          
            overlayInstance.externalAndroidSurfaceObject = Pvr_UnitySDKAPI.Render.UPvr_CreateLayerAndroidSurface((int)overlayInstance.overlayType, layerDepth);
            Debug.LogFormat("CreateExternalSurface: Overlay Type:{0}, LayerDepth:{1}, SurfaceObject:{2}", overlayInstance.overlayType, layerDepth, overlayInstance.externalAndroidSurfaceObject);

            if (overlayInstance.externalAndroidSurfaceObject != System.IntPtr.Zero)
            {
                if (overlayInstance.externalAndroidSurfaceObjectCreated != null)
                {
                    overlayInstance.externalAndroidSurfaceObjectCreated();
                }
            }
        }
#endif
    }

    #region EyeTrack  
    [HideInInspector]
    public bool EyeTracking = false;
    [HideInInspector]
    public Vector3 eyePoint;
    private EyeTrackingData eyePoseData;
    [HideInInspector]
    public static bool supportEyeTracking = false;

    public bool SetEyeTrackingMode()
    {
        int trackingMode = Pvr_UnitySDKAPI.System.UPvr_GetTrackingMode();
        supportEyeTracking = (trackingMode & (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE) != 0;
        bool result = false;

        if (EyeTracking && supportEyeTracking)
        {
            result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
        }
        Debug.Log("SetEyeTrackingMode EyeTracking " + EyeTracking + " supportEyeTracking " + supportEyeTracking + " result " + result);
        return result;
    }

    public Vector3 GetEyeTrackingPos()
    {
        if (!Pvr_UnitySDKEyeManager.Instance.EyeTracking)
            return Vector3.zero;

        bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyePoseData);
        if (!result)
        {
            PLOG.E("UPvr_getEyeTrackingData failed " + result);
            return Vector3.zero;
        }

        EyeDeviceInfo info = GetDeviceInfo();
        Vector3 frustumSize = Vector3.zero;
        frustumSize.x = 0.5f * (info.targetFrustumLeft.right - info.targetFrustumLeft.left);
        frustumSize.y = 0.5f * (info.targetFrustumLeft.top - info.targetFrustumLeft.bottom);
        frustumSize.z = info.targetFrustumLeft.near;

        var combinedDirection = eyePoseData.foveatedGazeDirection;
        float denominator = Vector3.Dot(combinedDirection, Vector3.forward);
        if (denominator > float.Epsilon)
        {
            eyePoint = combinedDirection * (frustumSize.z / denominator);
            eyePoint.x /= frustumSize.x; // [-1..1]
            eyePoint.y /= frustumSize.y; // [-1..1]
        }
        return eyePoint;
    }

    private EyeDeviceInfo GetDeviceInfo()
    {
        float vfov = Pvr_UnitySDKRender.Instance.EyeVFoV;
        float tanhalfvfov = Mathf.Tan(vfov / 2f * Mathf.Deg2Rad);

        float hfov = Pvr_UnitySDKRender.Instance.EyeHFoV;
        float tanhalfhfov = Mathf.Tan(hfov / 2f * Mathf.Deg2Rad);

        EyeDeviceInfo info;
        info.targetFrustumLeft.left = -(LeftEyeCamera.nearClipPlane * tanhalfhfov);
        info.targetFrustumLeft.right = LeftEyeCamera.nearClipPlane * tanhalfhfov;
        info.targetFrustumLeft.top = LeftEyeCamera.nearClipPlane * tanhalfvfov;
        info.targetFrustumLeft.bottom = -(LeftEyeCamera.nearClipPlane * tanhalfvfov);
        info.targetFrustumLeft.near = LeftEyeCamera.nearClipPlane;
        info.targetFrustumLeft.far = LeftEyeCamera.farClipPlane;

        info.targetFrustumRight.left = -(RightEyeCamera.nearClipPlane * tanhalfhfov);
        info.targetFrustumRight.right = RightEyeCamera.nearClipPlane * tanhalfhfov;
        info.targetFrustumRight.top = RightEyeCamera.nearClipPlane * tanhalfvfov;
        info.targetFrustumRight.bottom = -(RightEyeCamera.nearClipPlane * tanhalfvfov);
        info.targetFrustumRight.near = RightEyeCamera.nearClipPlane;
        info.targetFrustumRight.far = RightEyeCamera.farClipPlane;

        return info;
    }
    #endregion

    #region Screen Fade
    [Tooltip("If true, specific color gradient when switching scenes.")]
    public bool screenFade = false;
    [Tooltip("Define the duration of screen fade.")]
    public float fadeTime = 5.0f;
    [Tooltip("Define the color of screen fade.")]
    public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public int renderQueue = 5000;
    private MeshRenderer fadeMeshRenderer;
    private MeshFilter fadeMeshFilter;
    private Material fadeMaterial = null;
    private float elapsedTime;
    private bool isFading = false;
    private float currentAlpha;
    float nowFadeAlpha;
    private void CreateFadeMesh()
    {
        fadeMaterial = new Material(Shader.Find("Pvr_UnitySDK/Fade"));
        fadeMeshFilter = gameObject.AddComponent<MeshFilter>();
        fadeMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        var mesh = new Mesh();
        fadeMeshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[4];

        float width = 2f;
        float height = 2f;
        float depth = 1f;

        vertices[0] = new Vector3(-width, -height, depth);
        vertices[1] = new Vector3(width, -height, depth);
        vertices[2] = new Vector3(-width, height, depth);
        vertices[3] = new Vector3(width, height, depth);

        mesh.vertices = vertices;

        int[] tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        mesh.normals = normals;

        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;
    }

    private void DestoryFadeMesh()
    {
        if (fadeMeshRenderer != null)
            Destroy(fadeMeshRenderer);

        if (fadeMaterial != null)
            Destroy(fadeMaterial);

        if (fadeMeshFilter != null)
            Destroy(fadeMeshFilter);
    }

    public void SetCurrentAlpha(float alpha)
    {
        currentAlpha = alpha;
        SetMaterialAlpha();
    }

    /// <summary>
    /// Fades alpha from startAlpha to endAlpha
    /// </summary>
    IEnumerator ScreenFade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            nowFadeAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / fadeTime));
            SetMaterialAlpha();
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// show mesh if alpha>0
    /// </summary>
    private void SetMaterialAlpha()
    {
        Color color = fadeColor;
        color.a = Mathf.Max(currentAlpha, nowFadeAlpha);
        isFading = color.a > 0;
        if (fadeMaterial != null)
        {
            fadeMaterial.color = color;
            fadeMaterial.renderQueue = renderQueue;
            fadeMeshRenderer.material = fadeMaterial;
            fadeMeshRenderer.enabled = isFading;
        }
    }

    #endregion

    #region GfxDeviceAdvanceFrameGLES
    public bool GfxDeviceAdvanceFrameGLES()
    {
#if UNITY_2019_3_OR_NEWER
        Type device = typeof(UnityEngine.Android.AndroidDevice);
        var modules = device.GetMethods();
        foreach (var modole in modules)
        {
            if (modole.Name == "VRDeviceUseOwnSurface")
            {
                PLOG.I("Use VRDeviceUseOwnSurface");
                modole.Invoke(null, null);
                return true;
            }
        }
#endif
        return false;
    }
    #endregion

}