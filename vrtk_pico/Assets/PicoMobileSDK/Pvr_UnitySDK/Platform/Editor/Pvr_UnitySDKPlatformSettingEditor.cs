// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKPlatformSetting))]
public class Pvr_UnitySDKPlatformSettingEditor : Editor {

    private SerializedProperty deviceSNList;

    private void OnEnable()
    {
        deviceSNList = serializedObject.FindProperty("deviceSN");
    }

    [MenuItem("Pvr_UnitySDK" + "/Platform Settings")]
    public static void Edit()
    {
        Selection.activeObject = Pvr_UnitySDKPlatformSetting.Instance;
    }

    public override void OnInspectorGUI()
    {
        
        var startEntitleCheckTip = "If selected, you will need to enter the APPID that is obtained from" +
                                  " Pico Developer Platform after uploading the app for an entitlement check upon the app launch.";
        var startEntitleCheckLabel = new GUIContent("User Entitlement Check[?]", startEntitleCheckTip);
        
        Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck = EditorGUILayout.Toggle(startEntitleCheckLabel, Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck);
        if (Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)
        {
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("App ID ", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            Pvr_UnitySDKPlatformSetting.Instance.appID=EditorGUILayout.TextField(Pvr_UnitySDKPlatformSetting.Instance.appID, GUILayout.Width(350.0f));
            EditorGUILayout.EndHorizontal();
            if (Pvr_UnitySDKPlatformSetting.Instance.appID=="")
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
                EditorGUILayout.HelpBox("APPID is required for Entitlement Check", MessageType.Error, true);
                EditorGUILayout.EndHorizontal();
            }

            if (Pvr_SDKSetting.AppID != Pvr_UnitySDKPlatformSetting.Instance.appID)
            {
                Pvr_SDKSetting.AppID = Pvr_UnitySDKPlatformSetting.Instance.appID;
            }
            
            
            EditorGUILayout.BeginHorizontal();             
            GUILayout.Label("The APPID is required to run an Entitlement Check. Create / Find your APPID Here:", GUILayout.Width(500));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUIStyle style = new GUIStyle();
            style.normal.textColor = new Color(0, 122f / 255f, 204f / 255f);
            if (GUILayout.Button("" + "https://developer.pico-interactive.com/developer/overview", style, GUILayout.Width(200)))
            {
                Application.OpenURL("https://developer.pico-interactive.com/developer/overview");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("If you do not need user Entitlement Check, please uncheck it.", GUILayout.Width(500));
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            var simulationTip = "If true,Development devices will simulate Entitlement Check," +
                                "you should enter a valid device SN codes list." + 
                                "The SN code can be obtain in Settings-General-Device serial number or input  \"adb devices\" in cmd";
            var simulationLabel = new GUIContent("Entitlement Check Simulation [?]", simulationTip);

            Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation = EditorGUILayout.Toggle(simulationLabel, Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation);
            if (Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(deviceSNList, true);
                serializedObject.ApplyModifiedProperties();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(Pvr_UnitySDKPlatformSetting.Instance);
                GUI.changed = false;
            }
        }
        
      
    }
}
