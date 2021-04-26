// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

using System.Runtime.InteropServices;
using UnityEngine.Networking.Types;
using Debug = UnityEngine.Debug;

[InitializeOnLoad] 
public static class Pvr_UnitySDKBuildCheck
{
    private  static bool dontShowAgain = false;
    
     static Pvr_UnitySDKBuildCheck()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler( OnBuild );
        dontShowAgain = GetDontshowBuildWaring();
        Debug.Log("[Build Check] RegisterBuildPlayerHandler,Already Do not show: "+dontShowAgain);
    }
      static bool GetDontshowBuildWaring()
     {
         string path = Pvr_SDKSetting.assetPath + typeof(CPicoSDKSettingAsset).ToString() + ".asset";
         if (File.Exists(path))
         {
             CPicoSDKSettingAsset asset = AssetDatabase.LoadAssetAtPath<CPicoSDKSettingAsset>(path);
             if (asset != null)
             {
                 return asset.DontshowBuildWaring;
             }
         }
         return false;
     }
      
      public static void SetDontshowBuildWaring()
      {
          string path = Pvr_SDKSetting.assetPath + typeof(CPicoSDKSettingAsset).ToString() + ".asset";
          CPicoSDKSettingAsset asset = AssetDatabase.LoadAssetAtPath<CPicoSDKSettingAsset>(path);
          if (File.Exists(path))
          {
              
              asset.DontshowBuildWaring = true;
          }
          else
          {
              asset = new CPicoSDKSettingAsset();
              ScriptableObjectUtility.CreateAsset<CPicoSDKSettingAsset>(asset, Pvr_SDKSetting.assetPath);
          }
          asset.DontshowBuildWaring = true;
          EditorUtility.SetDirty(asset);
          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh();//must Refresh
      }
     public static void OnBuild(BuildPlayerOptions options)
    {
        if (!dontShowAgain)
        {
            if (!Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)
            {
                int result = EditorUtility.DisplayDialogComplex("Start-time Entitlement Check",
                    "EntitlementCheck is highly recommend which can\nprotect the copyright of app. Enable it now?",
                    "OK", "Ignore", "Ignore, Don't remind again");
                
                switch (@result)
                {
                    // ok
                    case 0:
                        Pvr_UnitySDKPlatformSettingEditor.Edit();
                        throw new System.OperationCanceledException("Build was canceled by the user.");
                        break;
                    //cancel
                    case 1:
                        Debug.LogWarning("Warning: EntitlementCheck is highly recommended which can protect the copyright of app. You can enable it when App start-up in the Inspector of \"Menu/Pvr_UnitySDK/PlatformSettings\" and Enter your APPID. If you want to call the APIs as needed, please refer to the development Document.");
                        Debug.Log("[Build Check] Start-time Entitlement Check Cancel The StartTime Entitlement Check status: " +Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck.ToString());
                        UpdateXMLEntitlementCheck(String.Format("{0}",Convert.ToInt32(Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)));
                        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                        break;
                    //alt
                    case 2:
                        dontShowAgain = true;
                        SetDontshowBuildWaring();
                        Debug.LogWarning("Warning: EntitlementCheck is highly recommended which can protect the copyright of app. You can enable it when App start-up in the Inspector of \"Menu/Pvr_UnitySDK/PlatformSettings\" and Enter your APPID. If you want to call the APIs as needed, please refer to the development Document.");
                        Debug.Log("[Build Check] Start-time Entitlement Check Do not show again The StartTime Entitlement Check status: " +Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck.ToString());
                        UpdateXMLEntitlementCheck(String.Format("{0}",Convert.ToInt32(Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)));
                        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                        break;
                }
            }
            else
            {
                Debug.Log("[Build Check]1 Enable Start-time Entitlement Check:"+Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck+", your AppID :" + Pvr_UnitySDKPlatformSetting.Instance.appID);
                UpdateXMLEntitlementCheck(String.Format("{0}",Convert.ToInt32(Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)));
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
        }
        else
        {
            Debug.Log("[Build Check]2 Enable Start-time Entitlement Check:"+Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck+", your AppID :" + Pvr_UnitySDKPlatformSetting.Instance.appID);
            UpdateXMLEntitlementCheck(String.Format("{0}",Convert.ToInt32(Pvr_UnitySDKPlatformSetting.StartTimeEntitlementCheck)));
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
    }
 
     
     static void UpdateXMLEntitlementCheck(string on)
     {
         XNamespace android = "http://schemas.android.com/apk/res/android";
         string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
         if (File.Exists(m_sXmlPath))
         {
             XmlDocument xmlDoc = new XmlDocument();
             xmlDoc.Load(m_sXmlPath);
             XmlNodeList nodeList;
             XmlElement root = xmlDoc.DocumentElement;

             nodeList = root.SelectNodes("/manifest/application/meta-data");
             foreach (XmlNode node in nodeList)
             {
                 if (node.Attributes.GetNamedItem("name", android.NamespaceName) != null)
                 {
                     if (node.Attributes.GetNamedItem("name", android.NamespaceName).Value == "enable_entitlementcheck")
                     {
                         if (node.Attributes.GetNamedItem("value", android.NamespaceName).Value == on)
                         {
                             PLOG.I("enable_entitlementcheck = " + on.ToString());
                         }
                         else
                         {
                             node.Attributes.GetNamedItem("value", android.NamespaceName).Value = on;
                             xmlDoc.Save(m_sXmlPath);
                         }
                         return;
                     }
                 }
             }
             XmlNode applicationNode = xmlDoc.SelectSingleNode("/manifest/application");
             XmlElement xmlEle = xmlDoc.CreateElement("meta-data");
             xmlEle.SetAttribute("name", android.NamespaceName, "enable_entitlementcheck");
             xmlEle.SetAttribute("value", android.NamespaceName, on);
             applicationNode.AppendChild(xmlEle);
             xmlDoc.Save(m_sXmlPath);
         }
     }
}
