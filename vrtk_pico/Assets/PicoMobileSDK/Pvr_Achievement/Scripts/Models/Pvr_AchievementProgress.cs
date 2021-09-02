
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
  public class Pvr_AchievementProgress
  {
    public readonly string Bitfield;
    public readonly long Count;
    public readonly bool IsUnlocked;
    public readonly string Name;
    public readonly DateTime UnlockTime;


    public Pvr_AchievementProgress(AndroidJavaObject msg)
    {
      Bitfield = Pvr_AchievementAPI.pvr_AchievementProgress_GetBitfield(msg);
      Count = Pvr_AchievementAPI.pvr_AchievementProgress_GetCount(msg);
      IsUnlocked = Pvr_AchievementAPI.pvr_AchievementProgress_GetIsUnlocked(msg);
      Name = Pvr_AchievementAPI.pvr_AchievementProgress_GetName(msg);
      UnlockTime = Pvr_AchievementAPI.pvr_AchievementProgress_GetUnlockTime(msg);
    }
  }

  public class Pvr_AchievementProgressList : Pvr_DeserializableList<Pvr_AchievementProgress> {
    public Pvr_AchievementProgressList(AndroidJavaObject msg) {
      var count = Pvr_AchievementAPI.pvr_AchievementProgressArray_GetSize(msg);
      data = new List<Pvr_AchievementProgress>(count);
      for (int i = 0; i < count; i++) {
        data.Add(new Pvr_AchievementProgress(Pvr_AchievementAPI.pvr_AchievementProgressArray_GetElement(msg, i)));
      }

      nextUrl = Pvr_AchievementAPI.pvr_AchievementProgressArray_GetNextUrl(msg);
    }

  }
}
