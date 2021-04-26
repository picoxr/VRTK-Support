using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pvr_ToBService : MonoBehaviour
{
    private void Awake()
    {
        InitToBService();
    }
    private void Start()
    {
        BindToBService();
    }
    private void OnDestory()
    {
        UnBindToBService();
    }
    private void InitToBService()
    {
        Pvr_UnitySDKAPI.ToBService.UPvr_InitToBService();
        Pvr_UnitySDKAPI.ToBService.UPvr_SetUnityObjectName(name);
    }
    private void BindToBService()
    {
        Pvr_UnitySDKAPI.ToBService.UPvr_BindToBService();
    }
    private void UnBindToBService()
    {
        Pvr_UnitySDKAPI.ToBService.UPvr_UnBindToBService();
    }
    private void BoolCallback(string value)
    {
        if (Pvr_UnitySDKAPI.ToBService.BoolCallback != null) Pvr_UnitySDKAPI.ToBService.BoolCallback(bool.Parse(value));
        Pvr_UnitySDKAPI.ToBService.BoolCallback = null;
    }
    private void IntCallback(string value)
    {
        if (Pvr_UnitySDKAPI.ToBService.IntCallback != null) Pvr_UnitySDKAPI.ToBService.IntCallback(int.Parse(value));
        Pvr_UnitySDKAPI.ToBService.IntCallback = null;
    }
    private void LongCallback(string value)
    {
        if (Pvr_UnitySDKAPI.ToBService.LongCallback != null) Pvr_UnitySDKAPI.ToBService.LongCallback(int.Parse(value));
        Pvr_UnitySDKAPI.ToBService.LongCallback = null;
    }

    #region Test
    public Text deviceInfoText;
    public void StateGetDeviceInfo()
    {
        string result = Pvr_UnitySDKAPI.ToBService.UPvr_StateGetDeviceInfo(Pvr_UnitySDKAPI.PBS_SystemInfoEnum.PUI_VERSION);
        deviceInfoText.text = "PUI_VERSION:" + result;
    }

    public void ControlSetDeviceAction()
    {
        Pvr_UnitySDKAPI.ToBService.UPvr_ControlSetDeviceAction(Pvr_UnitySDKAPI.PBS_DeviceControlEnum.DEVICE_CONTROL_SHUTDOWN, ControlSetDeviceActionCallBack);
    }
    private void ControlSetDeviceActionCallBack(int value)
    {
        Debug.Log("ControlSetDeviceActionCallBack : " + value);
    }

    public void AppManager()
    {
        Pvr_UnitySDKAPI.ToBService.UPvr_ControlAPPManger(Pvr_UnitySDKAPI.PBS_PackageControlEnum.PACKAGE_SILENCE_UNINSTALL, "com.pico.ipd.test", AppManagerCallBack);
    }
    private void AppManagerCallBack(int value)
    {
        Debug.Log("AppManagerCallBack : " + value);
    }

    #endregion
}
