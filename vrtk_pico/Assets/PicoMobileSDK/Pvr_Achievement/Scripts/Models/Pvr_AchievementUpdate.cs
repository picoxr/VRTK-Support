using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
  public class Pvr_AchievementUpdate
  {
    public readonly bool JustUnlocked;
    public readonly string Name;


    public Pvr_AchievementUpdate(AndroidJavaObject msg)
    {
      JustUnlocked = Pvr_AchievementAPI.pvr_AchievementUpdate_GetJustUnlocked(msg);
      Name = Pvr_AchievementAPI.pvr_AchievementUpdate_GetName(msg);
    }
  }

}
