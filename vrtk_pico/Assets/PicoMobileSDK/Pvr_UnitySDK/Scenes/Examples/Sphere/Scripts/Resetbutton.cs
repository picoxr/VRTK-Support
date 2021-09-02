// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using UnityEngine;

public class Resetbutton : MonoBehaviour
{
    public void DemoResetTracking()
    {
#if UNITY_EDITOR
        if (Pvr_UnitySDKManager.SDK.pvr_UnitySDKEditor != null)
        {
            Pvr_UnitySDKManager.SDK.pvr_UnitySDKEditor.ResetUnitySDKSensor();
        }
#else
        if (Pvr_UnitySDKSensor.Instance != null)
        {
            Pvr_UnitySDKSensor.Instance.ResetUnitySDKSensor();
        }
#endif
    }
}
