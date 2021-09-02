using Pvr_UnitySDKAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Pvr_UnityEyeMask : MonoBehaviour
{
    class EyeMaskData
    {
        public Eye eyeSide;
        public Camera camera;
        public CommandBuffer cmdBuf;
    }

    private Shader eyeMaskShader = null;
    private Material eyeMaskMaterial = null;
    private Mesh eyeMaskMeshLeft = null;
    private Mesh eyeMaskMeshRight = null;
    private Mesh eyeMaskMeshBoth = null;
    private float zDir = -1;

    private Color eyeMaskColor = Color.black;

    private List<EyeMaskData> cameraDataList = new List<EyeMaskData>();


    private void Awake()
    {
        Debug.Log("DISFT EyeMask = Enable");
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
        {
            zDir = -1;
        }
        else
        {
            if (SystemInfo.usesReversedZBuffer)
                zDir = 1;
            else
                zDir = 0;
        }
    }

    private void OnEnable()
    {
        if (this.eyeMaskShader == null)
        {
            this.eyeMaskShader = Shader.Find("Pvr_UnitySDK/Pvr_EyeMask");
        }

        if (this.eyeMaskMaterial == null && this.eyeMaskShader != null)
        {
            this.eyeMaskMaterial = new Material(this.eyeMaskShader);
            this.eyeMaskMaterial.SetColor("_Color", eyeMaskColor);
        }

        if (this.eyeMaskMaterial == null)
        {
            this.enabled = false;
            Debug.LogWarning("EyeMask materil is null or EyeMask shader not found!");
            return;
        }

        this.PrepareCameras();
        // register
        Camera.onPreRender += OnCustomPreRender;

        this.CreateCommandBuffer();
    }

    private void OnDisable()
    {
        // unregister
        Camera.onPreRender -= OnCustomPreRender;

        // remove commandbuffer
        foreach (var data in cameraDataList)
        {
            if (data.camera != null && data.cmdBuf != null)
            {
                this.RemoveCameraCommandBuffer(data);
            }
        }

        // clean
        this.CleanEyeMask();
    }


    void CreateCommandBuffer()
    {
        if (!this.VerifyCommadBuffer()) 
        {
            if (this.eyeMaskMeshLeft == null || this.eyeMaskMeshRight == null)
            {
                this.eyeMaskMeshLeft = this.GetStencilMesh(Eye.LeftEye);
                this.eyeMaskMeshRight = this.GetStencilMesh(Eye.RightEye);

                if (this.eyeMaskMeshLeft == null || this.eyeMaskMeshRight == null)
                {
                    Debug.LogWarning("Stencil Mesh is not exist, disable EyeMask.");
                    this.enabled = false;
                    return;
                }
            }

            // create commandbuffer
            foreach (EyeMaskData data in cameraDataList)
            {
                if (data.eyeSide == Eye.LeftEye)
                {
                    CommandBuffer cmdBuf = new CommandBuffer();
                    cmdBuf.name = "EyeMaskLeft";
                    cmdBuf.DrawMesh(this.eyeMaskMeshLeft, Matrix4x4.identity, this.eyeMaskMaterial, 0, 0);
                    data.cmdBuf = cmdBuf;
                }
                else if (data.eyeSide == Eye.RightEye)
                {
                    CommandBuffer cmdBuf = new CommandBuffer();
                    cmdBuf.name = "EyeMaskRight";
                    cmdBuf.DrawMesh(this.eyeMaskMeshRight, Matrix4x4.identity, this.eyeMaskMaterial, 0, 0);
                    data.cmdBuf = cmdBuf;
                }
                else if(data.eyeSide == Eye.BothEye)
                {
                    if (this.eyeMaskMeshBoth == null)
                    {
                        this.eyeMaskMeshBoth = this.GetStencilMeshBoth(this.eyeMaskMeshLeft, this.eyeMaskMeshRight);
                        float meshOffsetX = Mathf.Max(Mathf.Abs(eyeMaskMeshLeft.bounds.max.x), Mathf.Abs(eyeMaskMeshRight.bounds.min.x));
                        this.eyeMaskMaterial.SetFloat("_MeshOffsetX", meshOffsetX);
                    }

                    CommandBuffer cmdBuf = new CommandBuffer();
                    cmdBuf.name = "EyeMaskBoth";
                    cmdBuf.DrawMesh(this.eyeMaskMeshBoth, Matrix4x4.identity, this.eyeMaskMaterial, 0, 1);
                    data.cmdBuf = cmdBuf;
                }
            }
        }
    }

    void OnCustomPreRender(Camera cam)
    {
        if (!this.VerifyCommadBuffer())
        {
            Debug.LogWarning("Verify CommandBuffer failed!");
            return;
        }

        foreach (var data in cameraDataList)
        {
            if (data.camera != cam)
            {
                continue;
            }

            // remove commadbuffer
            this.RemoveCameraCommandBuffer(data);
            this.AddCameraCommandBuffer(data);
        }
    }

    Mesh GetStencilMesh(Eye eyeSide)
    {
        int vertexCount = 0;
        int triangleCount = 0;
        IntPtr vertexDataPtr = IntPtr.Zero;
        IntPtr triangleDataPtr = IntPtr.Zero;

        Pvr_UnitySDKAPI.Render.UPvr_GetStencilMesh((int)eyeSide, ref vertexCount, ref triangleCount, ref vertexDataPtr, ref triangleDataPtr);

        if (vertexCount <= 0 || triangleCount <= 0 || vertexDataPtr == IntPtr.Zero || triangleDataPtr == IntPtr.Zero)
        {
            return null;
        }

        Vector3[] VerticesUnity = new Vector3[vertexCount];
        int[] IndicesUnity = new int[triangleCount * 3];

        float[] vertexData = new float[vertexCount * 3];
        int[] indexData = new int[triangleCount * 3]; // right hand coordinate?
        Marshal.Copy(vertexDataPtr, vertexData, 0, vertexCount * 3);
        Marshal.Copy(triangleDataPtr, indexData, 0, triangleCount * 3);


        for (int i = 0; i < vertexCount; i++)
        {
            VerticesUnity[i] = new Vector3(vertexData[3 * i], vertexData[3 * i + 1], zDir);
        }

        for (int i = 0; i < triangleCount; i++)
        {
            IndicesUnity[3 * i] = indexData[3 * i + 2];
            IndicesUnity[3 * i + 1] = indexData[3 * i + 1];
            IndicesUnity[3 * i + 2] = indexData[3 * i];
        }

        Mesh mesh = new Mesh();
        mesh.name = "EyeMaskMesh";
        mesh.vertices = VerticesUnity;
        mesh.SetIndices(IndicesUnity, MeshTopology.Triangles, 0);

        return mesh;
    }

    Mesh GetStencilMeshBoth(Mesh leftMesh, Mesh rightMesh)
    {
        var meshOffsetX = Mathf.Max(Mathf.Abs(leftMesh.bounds.max.x), Mathf.Abs(rightMesh.bounds.min.x));

        var both = new Mesh();
        both.name = "EyeMaskBoth";

        var cil = new CombineInstance();
        cil.mesh = leftMesh;
        var matrixL = Matrix4x4.identity;
        matrixL.SetTRS(Vector3.left * meshOffsetX, Quaternion.identity, Vector3.one);
        cil.transform = matrixL;

        var cir = new CombineInstance();
        cir.mesh = rightMesh;
        var matrixR = Matrix4x4.identity;
        matrixR.SetTRS(Vector3.right * meshOffsetX, Quaternion.identity, Vector3.one);
        cir.transform = matrixR;

        CombineInstance[] cis = new CombineInstance[] { cil, cir };
        both.CombineMeshes(cis);

        return both;
    }

    bool VerifyCommadBuffer()
    {
        if (this.cameraDataList == null || this.cameraDataList.Count <= 0)
        {
            return false;
        }

        foreach (var data in cameraDataList)
        {
            if (data == null || data.cmdBuf == null)
            {
                return false;
            }
        }

        return true;
    }

    void PrepareCameras()
    {
        // clear
        this.cameraDataList.Clear();

        if (Pvr_UnitySDKRender.Instance.StereoRenderPath == StereoRenderingPathPico.SinglePass)
        {
            if (Pvr_UnitySDKEyeManager.Instance.BothEyeCamera == null)
            {
                Debug.LogWarning("BothEye Camera is null!");
                return;
            }

            EyeMaskData data = new EyeMaskData();
            data.eyeSide = Eye.BothEye;
            data.camera = Pvr_UnitySDKEyeManager.Instance.BothEyeCamera;
            this.cameraDataList.Add(data);
        }
        else
        {
            if (Pvr_UnitySDKEyeManager.Instance.LeftEyeCamera == null || Pvr_UnitySDKEyeManager.Instance.RightEyeCamera == null)
            {
                Debug.LogWarning("LeftEye or RightEye Camera is null!");
                return;
            }

            EyeMaskData data_L = new EyeMaskData();
            data_L.eyeSide = Eye.LeftEye;
            data_L.camera = Pvr_UnitySDKEyeManager.Instance.LeftEyeCamera;
            data_L.cmdBuf = null;
            this.cameraDataList.Add(data_L);

            EyeMaskData data_R = new EyeMaskData();
            data_R.eyeSide = Eye.RightEye;
            data_R.camera = Pvr_UnitySDKEyeManager.Instance.RightEyeCamera;
            data_R.cmdBuf = null;
            this.cameraDataList.Add(data_R);
        }
    }


    void AddCameraCommandBuffer(EyeMaskData data)
    {
        data.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, data.cmdBuf);
    }

    void RemoveCameraCommandBuffer(EyeMaskData data)
    {
        // remove commadbuffer
        data.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, data.cmdBuf);
    }

    void CleanEyeMask()
    {
        this.cameraDataList.Clear();

        this.eyeMaskMeshLeft = null;
        this.eyeMaskMeshRight = null;
        this.eyeMaskMeshBoth = null;
        this.eyeMaskShader = null;
        this.eyeMaskMaterial = null;
    }
}
