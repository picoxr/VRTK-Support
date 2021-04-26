// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System.Collections;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(Camera))]
public class Pvr_UnitySDKEye : MonoBehaviour
{
    public static List<Pvr_UnitySDKEye> Instances = new List<Pvr_UnitySDKEye>();

    /************************************    Properties  *************************************/
    #region Properties
    public Eye eyeSide;

    public Camera eyecamera { get; private set; }

    #region BoundarySystem
    private int eyeCameraOriginCullingMask;
    private CameraClearFlags eyeCameraOriginClearFlag;
    private Color eyeCameraOriginBackgroundColor;
    private int lastBoundaryState = 0;
    #endregion

    Matrix4x4 realProj = Matrix4x4.identity;
    private const int bufferSize = 3;
    private int IDIndex;

    private RenderEventType eventType = 0;

    private int previousId = 0;

    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        Instances.Add(this);
        eyecamera = GetComponent<Camera>();
    }

    void Start()
    {
        Setup(eyeSide);
        SetupUpdate();
        if (eyecamera != null)
        {
            #region BoundarySystem
            // record
            eyeCameraOriginCullingMask = eyecamera.cullingMask;
            eyeCameraOriginClearFlag = eyecamera.clearFlags;
            eyeCameraOriginBackgroundColor = eyecamera.backgroundColor;
            #endregion
        }
    }

    void Update()
    {
        // boundary
        if (eyecamera != null && eyecamera.enabled)
        {
            int currentBoundaryState = BoundarySystem.UPvr_GetSeeThroughState();

            if (currentBoundaryState != this.lastBoundaryState)
            {
                if (currentBoundaryState == 2) // close camera render(close camera render) and limit framerate(if needed)
                {
                    // record
                    eyeCameraOriginCullingMask = eyecamera.cullingMask;
                    eyeCameraOriginClearFlag = eyecamera.clearFlags;
                    eyeCameraOriginBackgroundColor = eyecamera.backgroundColor;

                    // close render
                    eyecamera.cullingMask = 0;
                    eyecamera.clearFlags = CameraClearFlags.SolidColor;
                    eyecamera.backgroundColor = Color.black;
                }
                else if (currentBoundaryState == 1) // open camera render, but limit framerate(if needed)
                {
                    if (this.lastBoundaryState == 2)
                    {
                        if (eyecamera.cullingMask == 0)
                        {
                            eyecamera.cullingMask = eyeCameraOriginCullingMask;
                        }
                        if (eyecamera.clearFlags == CameraClearFlags.SolidColor)
                        {
                            eyecamera.clearFlags = eyeCameraOriginClearFlag;
                        }
                        if (eyecamera.backgroundColor == Color.black)
                        {
                            eyecamera.backgroundColor = eyeCameraOriginBackgroundColor;
                        }                       
                    }
                }
                else // open camera render(recover)
                {
                    if ((this.lastBoundaryState == 2 || this.lastBoundaryState == 1))
                    {
                        if (eyecamera.cullingMask == 0)
                        {
                            eyecamera.cullingMask = eyeCameraOriginCullingMask;
                        }
                        if (eyecamera.clearFlags == CameraClearFlags.SolidColor)
                        {
                            eyecamera.clearFlags = eyeCameraOriginClearFlag;
                        }
                        if (eyecamera.backgroundColor == Color.black)
                        {
                            eyecamera.backgroundColor = eyeCameraOriginBackgroundColor;
                        }

                    }
                }

                this.lastBoundaryState = currentBoundaryState;
            }
        }
    }


    void OnEnable()
    {
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += MyPreRender;
#endif
#if UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            RenderPipelineManager.beginCameraRendering += MyPreRender;
            RenderPipelineManager.endCameraRendering += MyPostRender;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_2018_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= MyPreRender;
#endif
#if UNITY_2019_1_OR_NEWER
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
        {
            RenderPipelineManager.beginCameraRendering -= MyPreRender;
            RenderPipelineManager.endCameraRendering -= MyPostRender;
        }
#endif
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    public void MyPreRender(Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPreRender();
    }

    public void MyPreRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPreRender();
    }

    public void MyPostRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject != this.gameObject)
            return;
        OnPostRender();
    }

    public static bool setLevel = false;

    void OnPreRender()
    {
        if (!eyecamera.enabled)
            return;
#if ANDROID_DEVICE
        if (Pvr_UnitySDKRender.Instance.StereoRendering != null)
        {
            if (Pvr_UnitySDKRender.Instance.isSwitchSDK)
            {
                return;
            }
            Pvr_UnitySDKRender.Instance.StereoRendering.OnSDKPreRender();
        }

        SetFFRParameter();
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.BeginEye);
#endif
    }

    void OnPostRender()
    {
        if (!eyecamera.enabled)
            return;
        //DrawVignetteLine();
#if ANDROID_DEVICE
        // eyebuffer
        Pvr_UnitySDKAPI.System.UPvr_UnityEventData(Pvr_UnitySDKAPI.System.UPvr_GetEyeBufferData(Pvr_UnitySDKRender.Instance.eyeTextureIds[IDIndex]));
        Pvr_UnitySDKPluginEvent.Issue(eventType);
        if (Pvr_UnitySDKRender.Instance.StereoRendering != null)
        {
            if (Pvr_UnitySDKRender.Instance.isSwitchSDK)
            {
                return;
            }
            Pvr_UnitySDKRender.Instance.StereoRendering.OnSDKPostRender();
        }
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
#endif
    }

#if UNITY_EDITOR
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        ModifyShadePara();
        Graphics.Blit(source, dest, Pvr_UnitySDKManager.SDK.Eyematerial);
    }

    void ModifyShadePara()
    {
        Matrix4x4 proj = Matrix4x4.identity;
        float near = GetComponent<Camera>().nearClipPlane;
        float far = GetComponent<Camera>().farClipPlane;
        float aspectFix = GetComponent<Camera>().rect.height / GetComponent<Camera>().rect.width / 2;

        proj[0, 0] *= aspectFix;
        Vector2 dir = transform.localPosition; // ignore Z
        dir = dir.normalized * 1.0f;
        proj[0, 2] *= Mathf.Abs(dir.x);
        proj[1, 2] *= Mathf.Abs(dir.y); proj[2, 2] = (near + far) / (near - far);
        proj[2, 3] = 2 * near * far / (near - far);

        Vector4 projvec = new Vector4(proj[0, 0], proj[1, 1],
                                    proj[0, 2] - 1, proj[1, 2] - 1) / 2;

        Vector4 unprojvec = new Vector4(realProj[0, 0], realProj[1, 1],
                                        realProj[0, 2] - 1, realProj[1, 2] - 1) / 2;

        float distortionFactor = 0.0241425f;
        Shader.SetGlobalVector("_Projection", projvec);
        Shader.SetGlobalVector("_Unprojection", unprojvec);
        Shader.SetGlobalVector("_Distortion1",
                                new Vector4(Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k1, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k2, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k3, distortionFactor));
        Shader.SetGlobalVector("_Distortion2",
                               new Vector4(Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k4, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k5, Pvr_UnitySDKManager.SDK.pvr_UnitySDKConfig.device.devDistortion.k6));

    }

#endif
    #endregion

    /************************************ Public Interfaces  *********************************/
    #region Public Interfaces

    public void EyeRender()
    {
        SetupUpdate();
#if !UNITY_EDITOR
        if (Pvr_UnitySDKRender.Instance.eyeTextures[IDIndex] != null)
        {
            Pvr_UnitySDKRender.Instance.eyeTextures[IDIndex].DiscardContents();
            eyecamera.targetTexture = Pvr_UnitySDKRender.Instance.eyeTextures[IDIndex];
        }
#endif
    }

    #endregion

    /************************************ Private Interfaces  *********************************/
    #region Private Interfaces
    private void Setup(Eye eyeSide)
    {
        eyecamera = GetComponent<Camera>();
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            transform.localPosition = Pvr_UnitySDKManager.SDK.EyeOffset(eyeSide);
        }
        else if (eyeSide == Eye.BothEye)
        {
            transform.localPosition = Vector3.zero;
        }
        eyecamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        eyecamera.rect = new Rect(0, 0, 1, 1);
#if UNITY_EDITOR
        eyecamera.rect = Pvr_UnitySDKManager.SDK.EyeRect(eyeSide);
#endif
        //  AW
        if (Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.MultiPass)
        {
            eventType = (eyeSide == Eye.LeftEye) ?
                        RenderEventType.LeftEyeEndFrame :
                        RenderEventType.RightEyeEndFrame;
        }
        else
        {
            eventType = RenderEventType.BothEyeEndFrame;
        }

    }

    private void SetupUpdate()
    {
#if !UNITY_EDITOR
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            eyecamera.enabled = !(Pvr_UnitySDKManager.SDK.Monoscopic || Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.SinglePass);
        }
        else if (eyeSide == Eye.BothEye)
        {
            eyecamera.enabled = Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.SinglePass;
        }
#endif
        eyecamera.fieldOfView = Pvr_UnitySDKRender.Instance.EyeVFoV;
        eyecamera.aspect = Pvr_UnitySDKManager.SDK.EyesAspect;
        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            IDIndex = Pvr_UnitySDKRender.Instance.currEyeTextureIdx + (int)eyeSide * bufferSize;
        }
        else if (eyeSide == Eye.BothEye)
        {
            IDIndex = Pvr_UnitySDKRender.Instance.currEyeTextureIdx;
            Pvr_UnitySDKRender.Instance.isSwitchSDK = Pvr_UnitySDKRender.Instance.lastEyeTextureIdx == Pvr_UnitySDKRender.Instance.currEyeTextureIdx;
            Pvr_UnitySDKRender.Instance.lastEyeTextureIdx = Pvr_UnitySDKRender.Instance.currEyeTextureIdx;
        }
    }

    public void RefreshCameraPosition(float ipd)
    {
        Pvr_UnitySDKManager.SDK.leftEyeOffset = new Vector3(-ipd / 2, 0, 0);
        Pvr_UnitySDKManager.SDK.rightEyeOffset = new Vector3(ipd / 2, 0, 0);

        if (eyeSide == Eye.LeftEye || eyeSide == Eye.RightEye)
        {
            transform.localPosition = Pvr_UnitySDKManager.SDK.EyeOffset(eyeSide);
        }
        else if (eyeSide == Eye.BothEye)
        {
            eyecamera.stereoSeparation = ipd;
        }
    }

    #region  DrawVignetteLine

    private Material mat_Vignette;

    void DrawVignetteLine()
    {
        if (null == mat_Vignette)
        {
            mat_Vignette = new Material(Shader.Find("Diffuse"));//Mobile/
            if (null == mat_Vignette)
            {
                return;
            }
        }
        GL.PushMatrix();
        mat_Vignette.SetPass(0);
        GL.LoadOrtho();
        vignette();
        GL.PopMatrix();
    }

    void vignette()
    {
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);
        //top
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.995f, 0.0f);
        GL.Vertex3(0.0f, 0.995f, 0.0f);
        //bottom
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.005f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        //left
        GL.Vertex(new Vector3(0.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(0.005f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.0f, 0.0f, 0.0f));
        //right
        GL.Vertex(new Vector3(0.995f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 1.0f, 0.0f));
        GL.Vertex(new Vector3(1.0f, 0.0f, 0.0f));
        GL.Vertex(new Vector3(0.995f, 0.0f, 0.0f));
        GL.End();
    }

    #endregion

    #endregion

    private void SetFFRParameter()
    {

        Vector3 eyePoint = Vector3.zero;
        if (Pvr_UnitySDKManager.SDK.isEnterVRMode && Pvr_UnitySDKEyeManager.supportEyeTracking && Pvr_UnitySDKEyeManager.Instance.EyeTracking)
        {
            eyePoint = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingPos();
        }
        int eyeTextureId = Pvr_UnitySDKRender.Instance.eyeTextureIds[IDIndex];
        Pvr_UnitySDKAPI.Render.UPvr_SetFoveationResource(eyeTextureId, previousId, eyePoint.x, eyePoint.y);
        previousId = eyeTextureId;
    }
}