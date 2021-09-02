using System;
using System.ComponentModel;
using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public enum AchievementType : int
    {
        [Description("UNKNOWN")]
        Unknown,

        [Description("SIMPLE")]
        Simple,

        [Description("BITFIELD")]
        Bitfield,

        [Description("COUNT")]
        Count,

    }
    public class Pvr_AchievementAPI
    {
#if UNITY_ANDROID
        private static AndroidJavaClass achievementAPI = new AndroidJavaClass("com.pico.achievenment.AchievementAPI");
        private static AndroidJavaClass definitionArrayHandle = new AndroidJavaClass("com.picovr.achievement.utils.pvrAchievementDefinitionArrayHandle");
        private static AndroidJavaClass definitionHandle = new AndroidJavaClass("com.picovr.achievement.utils.pvrAchievementDefinitionHandle");
        private static AndroidJavaClass progressArrayHandle = new AndroidJavaClass("com.picovr.achievement.utils.pvrAchievementProgressArrayHandle");
        private static AndroidJavaClass progressHandle = new AndroidJavaClass("com.picovr.achievement.utils.pvrAchievementProgressHandle");
        private static AndroidJavaClass updateHandle = new AndroidJavaClass("com.picovr.achievement.utils.pvrAchievementUpdateHandle");
        private static AndroidJavaObject errorHandle = new AndroidJavaObject("com.picovr.achievement.utils.pvrAchievementErrorHandle");
        private static AndroidJavaObject unityInterface = new AndroidJavaObject("com.pico.loginpaysdk.UnityInterface");

#endif
        private static string openId;
        private static string accessToken;
        private static string appId = Pvr_UnitySDKPlatformSetting.Instance.appID;

        internal static long Init()
        {
            long returnValue = 0;
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            unityInterface.Call("init", currentActivity);
            unityInterface.Call("authSSO");

            AndroidJavaClass accessTokenKeeper = new AndroidJavaClass("com.pico.loginpaysdk.utils.PicoAccessTokenKeeper");
            AndroidJavaObject accessInfo = accessTokenKeeper.CallStatic<AndroidJavaObject>("readAccessToken", currentActivity);

            accessToken = accessInfo.Call<string>("getAccessToken");
            openId = accessInfo.Call<string>("getOpenId");
            returnValue = achievementAPI.CallStatic<long>("init", accessToken, openId, currentActivity);
#endif
            return returnValue;
        }
        internal static void RegisterNetwork()
        {
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            achievementAPI.CallStatic("registerNetwork", currentActivity);
#endif
        }
        internal static void UnRegisterNetwork()
        {
#if UNITY_ANDROID
            achievementAPI.CallStatic("unregisterNetwork");
#endif
        }
        internal static AndroidJavaObject PopMessage()
        {
#if UNITY_ANDROID
            return achievementAPI.CallStatic<AndroidJavaObject>("pvr_PopMessage");
#else
            return null;
#endif
        }
        internal static string pvr_Error_GetMessage(AndroidJavaObject popMessage)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = errorHandle.CallStatic<string>("pvr_Error_GetMessage", popMessage);
#endif
            return returnValue;
        }
        internal static int pvr_Error_GetHttpCode(AndroidJavaObject popMessage)
        {
            int returnValue = 0;
#if UNITY_ANDROID
            returnValue = errorHandle.CallStatic<int>("pvr_Error_GetHttpCode", popMessage);
#endif
            return returnValue;
        }
        internal static int pvr_Error_GetCode(AndroidJavaObject popMessage)
        {
            int returnValue = 0;
#if UNITY_ANDROID
            returnValue = errorHandle.CallStatic<int>("pvr_Error_GetCode", popMessage);
#endif
            return returnValue;
        }

        internal static long pvr_Achievements_AddCount(string name, long count)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_AddCount", name, count, accessToken);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_AddFields(string name, string fields)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_AddFields", name, fields, accessToken);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_GetAllDefinitions()
        {
            long returnValue = 0;
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetAllDefinitions", appId, currentActivity);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_GetAllProgress()
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetAllProgress", accessToken);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_GetDefinitionsByName(string[] names, int v)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetDefinitionsByName", names, currentActivity);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_GetProgressByName(string[] names, int v)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetProgressByName", names, accessToken);
#endif
            return returnValue;
        }
        internal static long pvr_Achievements_Unlock(string name)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_Unlock", name, accessToken);
#endif
            return returnValue;
        }
        internal static long pvr_HTTP_GetWithMessageType(string nextUrl, Pvr_Message.MessageType messageType)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            switch (messageType)
            {
                case Pvr_Message.MessageType.Achievements_GetNextAchievementDefinitionArrayPage:
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetAllDefinitionsByUrl", nextUrl, currentActivity);
                    break;
                case Pvr_Message.MessageType.Achievements_GetNextAchievementProgressArrayPage:
                    returnValue = achievementAPI.CallStatic<long>("pvr_Achievements_GetAllProgressByUrl", nextUrl);
                    break;
                default:
                    break;
            }
#endif
            return returnValue;
        }


        internal static long pvr_Message_GetType(AndroidJavaObject popMessage)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = popMessage.Call<AndroidJavaObject>("getHandleType").Call<long>("getIndex");
#endif
            return returnValue;
        }

        internal static bool pvr_Message_IsError(AndroidJavaObject popMessage)
        {
            return popMessage.Call<bool>("isMessage_IsError");
        }

        internal static long pvr_Message_GetRequestID(AndroidJavaObject popMessage)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = popMessage.Call<long>("getId");
#endif
            return returnValue;
        }

        internal static string pvr_Message_GetString(AndroidJavaObject popMessage)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = popMessage.Call<string>("getContent");
#endif
            return returnValue;
        }

        internal static bool pvr_AchievementUpdate_GetJustUnlocked(AndroidJavaObject popMessage)
        {
            bool returnValue = true;
#if UNITY_ANDROID
            returnValue = updateHandle.CallStatic<bool>("pvr_AchievementUpdate_GetJustUnlocked", popMessage);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementUpdate_GetName(AndroidJavaObject popMessage)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = updateHandle.CallStatic<string>("pvr_AchievementUpdate_GetName", popMessage);
#endif
            return returnValue;
        }



        internal static int pvr_AchievementProgressArray_GetSize(AndroidJavaObject msg)
        {
            int returnValue = 0;
#if UNITY_ANDROID
            returnValue = progressArrayHandle.CallStatic<int>("pvr_AchievementProgressArray_GetSize", msg);
#endif
            return returnValue;
        }

        internal static AndroidJavaObject pvr_AchievementProgressArray_GetElement(AndroidJavaObject msg, int index)
        {
#if UNITY_ANDROID
            return progressArrayHandle.CallStatic<AndroidJavaObject>("pvr_AchievementProgressArray_GetElement", msg, index);
#else
            return null;
#endif
        }

        internal static string pvr_AchievementProgressArray_GetNextUrl(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = progressArrayHandle.CallStatic<string>("pvr_AchievementProgressArray_GetNextUrl", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementProgress_GetBitfield(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = progressHandle.CallStatic<string>("pvr_AchievementProgress_GetBitfield", msg);
#endif
            return returnValue;
        }

        internal static long pvr_AchievementProgress_GetCount(AndroidJavaObject msg)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = progressHandle.CallStatic<long>("pvr_AchievementProgress_GetCount", msg);
#endif
            return returnValue;
        }

        internal static bool pvr_AchievementProgress_GetIsUnlocked(AndroidJavaObject msg)
        {
            bool returnValue = true;
#if UNITY_ANDROID
            returnValue = progressHandle.CallStatic<bool>("pvr_AchievementProgress_GetIsUnlocked", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementProgress_GetName(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = progressHandle.CallStatic<string>("pvr_AchievementProgress_GetName", msg);
#endif
            return returnValue;
        }

        internal static DateTime pvr_AchievementProgress_GetUnlockTime(AndroidJavaObject msg)
        {
            DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0, 0);
#if UNITY_ANDROID
            returnValue = DateTimeFromNative(progressHandle.CallStatic<long>("pvr_AchievementProgress_GetUnlockTime", msg));
#endif
            return returnValue;
        }
        internal static DateTime DateTimeFromNative(long seconds_since_the_one_true_epoch)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dt.AddSeconds(seconds_since_the_one_true_epoch).ToLocalTime();
        }


        internal static int pvr_AchievementDefinitionArray_GetSize(AndroidJavaObject msg)
        {
            int returnValue = 0;
#if UNITY_ANDROID
            returnValue = definitionArrayHandle.CallStatic<int>("pvr_AchievementDefinitionArray_GetSize", msg);
#endif
            return returnValue;
        }

        internal static AndroidJavaObject pvr_AchievementDefinitionArray_GetElement(AndroidJavaObject msg, int index)
        {
#if UNITY_ANDROID
            return definitionArrayHandle.CallStatic<AndroidJavaObject>("pvr_AchievementDefinitionArray_GetElement", msg, index);
#else
            return null;
#endif
        }

        internal static string pvr_AchievementDefinitionArray_GetNextUrl(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionArrayHandle.CallStatic<string>("pvr_AchievementDefinitionArray_GetNextUrl", msg);
#endif
            return returnValue;
        }

        internal static AchievementType pvr_AchievementDefinition_GetType(AndroidJavaObject msg)
        {
            AchievementType returnValue = AchievementType.Bitfield;
#if UNITY_ANDROID
            AndroidJavaObject ajo = definitionHandle.CallStatic<AndroidJavaObject>("pvr_AchievementDefinition_GetType", msg);
            returnValue = (AchievementType)ajo.Call<int>("getIndex");
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetName(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetName", msg);
#endif
            return returnValue;
        }

        internal static int pvr_AchievementDefinition_GetBitfieldLength(AndroidJavaObject msg)
        {
            int returnValue = 0;
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<int>("pvr_AchievementDefinition_GetBitfieldLength", msg);
#endif
            return returnValue;
        }

        internal static long pvr_AchievementDefinition_GetTarget(AndroidJavaObject msg)
        {
            long returnValue = 0;
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<long>("pvr_AchievementDefinition_GetTarget", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetTitle(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetTitle", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetUnlockedDescription(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetUnlocked_description", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetLockedIcon(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetLocked_image", msg);
#endif
            return returnValue;
        }

        internal static bool pvr_AchievementDefinition_GetIsSecrect(AndroidJavaObject msg)
        {
            bool returnValue = false;
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<bool>("pvr_AchievementDefinition_GetIs_secret", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetUnlockedIcon(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetUnlocked_image", msg);
#endif
            return returnValue;
        }

        internal static string pvr_AchievementDefinition_GetDescription(AndroidJavaObject msg)
        {
            string returnValue = "";
#if UNITY_ANDROID
            returnValue = definitionHandle.CallStatic<string>("pvr_AchievementDefinition_GetDescription", msg);
#endif
            return returnValue;
        }
    }
}