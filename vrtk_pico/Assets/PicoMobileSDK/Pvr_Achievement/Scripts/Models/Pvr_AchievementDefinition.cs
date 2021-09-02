using System.Collections.Generic;
using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public class Pvr_AchievementDefinition
    {
        public readonly AchievementType Type;
        public readonly string Name;
        public readonly int BitfieldLength;
        public readonly long Target;
        public readonly string Title;
        public readonly string Description;
        public readonly string UnlockedDescription;
        public readonly string UnlockedIcon;
        public readonly string LockedIcon;
        public readonly bool IsSecrect;


        public Pvr_AchievementDefinition(AndroidJavaObject msg)
        {
            Type = Pvr_AchievementAPI.pvr_AchievementDefinition_GetType(msg);
            Name = Pvr_AchievementAPI.pvr_AchievementDefinition_GetName(msg);
            BitfieldLength = Pvr_AchievementAPI.pvr_AchievementDefinition_GetBitfieldLength(msg);
            Target = Pvr_AchievementAPI.pvr_AchievementDefinition_GetTarget(msg);
            Title  = Pvr_AchievementAPI.pvr_AchievementDefinition_GetTitle(msg);
            Description = Pvr_AchievementAPI.pvr_AchievementDefinition_GetDescription(msg);
            UnlockedDescription = Pvr_AchievementAPI.pvr_AchievementDefinition_GetUnlockedDescription(msg);
            UnlockedIcon = Pvr_AchievementAPI.pvr_AchievementDefinition_GetUnlockedIcon(msg);
            LockedIcon = Pvr_AchievementAPI.pvr_AchievementDefinition_GetLockedIcon(msg);
            IsSecrect = Pvr_AchievementAPI.pvr_AchievementDefinition_GetIsSecrect(msg);
        }
    }

    public class Pvr_AchievementDefinitionList : Pvr_DeserializableList<Pvr_AchievementDefinition>
    {
        public Pvr_AchievementDefinitionList(AndroidJavaObject msg)
        {
            var count = Pvr_AchievementAPI.pvr_AchievementDefinitionArray_GetSize(msg);
            data = new List<Pvr_AchievementDefinition>(count);
            for (int i = 0; i < count; i++)
            {
                data.Add(new Pvr_AchievementDefinition(Pvr_AchievementAPI.pvr_AchievementDefinitionArray_GetElement(msg, i)));
            }

            nextUrl = Pvr_AchievementAPI.pvr_AchievementDefinitionArray_GetNextUrl(msg);
        }

    }
}
