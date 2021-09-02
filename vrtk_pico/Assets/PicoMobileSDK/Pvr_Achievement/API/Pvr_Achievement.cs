using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public sealed class AchievementCore
    {
        private static bool IsPlatformInitialized = true;
        public static bool IsInitialized()
        {
            return IsPlatformInitialized;
        }

        public static void Initialize()
        {
        }

        public static void RegisterNetwork()
        {
            Pvr_AchievementAPI.RegisterNetwork();
        }

        public static void UnRegisterNetwork()
        {
            Pvr_AchievementAPI.UnRegisterNetwork();
        }

        // If LogMessages is true, then the contents of each request response
        // will be printed using Debug.Log. This allocates a lot of heap memory,
        // and so should not be called outside of testing and debugging.
        public static bool LogMessages = false;
    }

    public static partial class Achievements
    {
        public static Pvr_Request<Pvr_AchievementUpdate> Init()
        {

            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementUpdate>(Pvr_AchievementAPI.Init());
            }
            return null;
        }
        /*Add 'count' to the achievement with the given name. This must be a COUNT
         achievement. The largest number that is supported by this method is the max
        value of a signed 64-bit integer. If the number is larger than that, it is
        clamped to that max value before being passed to the servers.
        */
        public static Pvr_Request<Pvr_AchievementUpdate> AddCount(string name, long count)
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementUpdate>(Pvr_AchievementAPI.pvr_Achievements_AddCount(name, count));
            }

            return null;
        }

        /// Unlock fields of a BITFIELD achievement.
        /// \param name The name of the achievement to unlock
        /// \param fields A string containing either '0' or '1' characters. Every '1' will unlock the field in the corresponding position.
        ///
        public static Pvr_Request<Pvr_AchievementUpdate> AddFields(string name, string fields)
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementUpdate>(Pvr_AchievementAPI.pvr_Achievements_AddFields(name, fields));
            }

            return null;
        }

        /// Request all achievement definitions for the app.
        ///
        public static Pvr_Request<Pvr_AchievementDefinitionList> GetAllDefinitions()
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementDefinitionList>(Pvr_AchievementAPI.pvr_Achievements_GetAllDefinitions());
            }

            return null;
        }

        /// Request the progress for the user on all achievements in the app.
        ///
        public static Pvr_Request<Pvr_AchievementProgressList> GetAllProgress()
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementProgressList>(Pvr_AchievementAPI.pvr_Achievements_GetAllProgress());
            }

            return null;
        }

        /// Request the achievement definitions that match the specified names.
        ///
        public static Pvr_Request<Pvr_AchievementDefinitionList> GetDefinitionsByName(string[] names)
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementDefinitionList>(Pvr_AchievementAPI.pvr_Achievements_GetDefinitionsByName(names, (names != null ? names.Length : 0)));
            }

            return null;
        }

        /// Request the user's progress on the specified achievements.
        ///
        public static Pvr_Request<Pvr_AchievementProgressList> GetProgressByName(string[] names)
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementProgressList>(Pvr_AchievementAPI.pvr_Achievements_GetProgressByName(names, (names != null ? names.Length : 0)));
            }

            return null;
        }

        /// Unlock the achievement with the given name. This can be of any achievement
        /// type.
        ///
        public static Pvr_Request<Pvr_AchievementUpdate> Unlock(string name)
        {
            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementUpdate>(Pvr_AchievementAPI.pvr_Achievements_Unlock(name));
            }

            return null;
        }

        public static Pvr_Request<Pvr_AchievementDefinitionList> GetNextAchievementDefinitionListPage(Pvr_AchievementDefinitionList list)
        {
            if (!list.HasNextPage)
            {
                Debug.LogWarning("Platform.GetNextAchievementDefinitionListPage: List has no next page");
                return null;
            }

            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementDefinitionList>(
                  Pvr_AchievementAPI.pvr_HTTP_GetWithMessageType(
                    list.NextUrl,
                    Pvr_Message.MessageType.Achievements_GetNextAchievementDefinitionArrayPage
                  )
                );
            }

            return null;
        }

        public static Pvr_Request<Pvr_AchievementProgressList> GetNextAchievementProgressListPage(Pvr_AchievementProgressList list)
        {
            if (!list.HasNextPage)
            {
                Debug.LogWarning("Platform.GetNextAchievementProgressListPage: List has no next page");
                return null;
            }

            if (AchievementCore.IsInitialized())
            {
                return new Pvr_Request<Pvr_AchievementProgressList>(
                  Pvr_AchievementAPI.pvr_HTTP_GetWithMessageType(
                    list.NextUrl,
                    Pvr_Message.MessageType.Achievements_GetNextAchievementProgressArrayPage
                  )
                );
            }

            return null;
        }

    }
}
