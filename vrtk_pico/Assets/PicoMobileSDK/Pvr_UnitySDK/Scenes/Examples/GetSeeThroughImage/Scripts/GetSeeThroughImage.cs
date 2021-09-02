using UnityEngine;
using UnityEngine.UI;

public class GetSeeThroughImage : MonoBehaviour
{
    public RawImage viewImage_left,viewImage_right;
    private int width, height;
    private RenderTexture cameraTex_left,cameraTex_right;
    private bool cameraPreview;
    void Start()
    {
        CreateTexture();
    }
    void Update()
    {
        //pico neo2 - Confirm Key  
        if (Input.GetKey(KeyCode.JoystickButton0))
        {
            DrawTexture();
        }
    }

    void OnDestory()
    {
        if (cameraPreview)
        {
            cameraPreview = false;
            Pvr_UnitySDKAPI.BoundarySystem.UPvr_StopCameraFrame(); 
        }
    }
    private void CreateTexture()
    {
        width = 600;
        height = 600;

        Pvr_UnitySDKAPI.BoundarySystem.UPvr_BoundarySetCameraImageRect(width, height);

        cameraTex_left = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
        cameraTex_right = new RenderTexture(width, height, 24, RenderTextureFormat.Default);

        cameraTex_left.Create();
        cameraTex_right.Create();

        viewImage_left.texture = cameraTex_left;
        viewImage_right.texture = cameraTex_right;
    }
    private void DrawTexture()
    {
        if (!cameraPreview)
        {
            cameraPreview = true;
            Pvr_UnitySDKAPI.BoundarySystem.UPvr_StartCameraFrame();
        }
        //draw left camera
        Pvr_UnitySDKAPI.BoundarySystem.UPvr_BoundaryGetSeeThroughData(0, cameraTex_left);
        //draw right camera
        Pvr_UnitySDKAPI.BoundarySystem.UPvr_BoundaryGetSeeThroughData(1, cameraTex_right);
    }
}
