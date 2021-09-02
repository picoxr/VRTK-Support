using System;
using UnityEngine;
using UnityEngine.UI;
using Pvr_UnitySDKAPI.Achievement;

public class AchievementSample : MonoBehaviour
{

    public Text dataOutput;
    public InputField getDefinitionsByNameInputField;
    public InputField getProgressByNameInputField;
    public InputField unLockInputField;
    public InputField addCountInputField;
    public InputField addFieldsInputField;

    void Start()
    {
        AchievementCore.RegisterNetwork();
    }

    void OnDestroy()
    {
        AchievementCore.UnRegisterNetwork();
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            printOutputLine("Achievement Init");
            achievementInit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle all messages being returned
        Request.RunCallbacks();
    }

    public void AsyncInitialize()
    {
        dataOutput.text = "> ";
        printOutputLine("AsyncInitialize");
        achievementInit();
    }
    public void GetDefinitionsByName()
    {
        dataOutput.text = "> ";
        printOutputLine("GetDefinitionsByName");
        string currentText = getDefinitionsByNameInputField.text;

        if (currentText != "")
        {
            string[] commandParams = currentText.Split(',');
            getAchievementDefinition(commandParams);
            getDefinitionsByNameInputField.text = "";
        }
        else
        {
            printOutputLine("Invalid Command");
        }
    }
    public void GetProgressByName()
    {
        dataOutput.text = "> ";
        printOutputLine("GetProgressByName");
        string currentText = getProgressByNameInputField.text;

        if (currentText != "")
        {
            string[] commandParams = currentText.Split(',');
            getAchievementProgress(commandParams);
            getProgressByNameInputField.text = "";
        }
        else
        {
            printOutputLine("Invalid Command");
        }
    }
    public void GetAllDefinitions()
    {
        dataOutput.text = "> ";
        printOutputLine("GetAllDefinitions");
        getAchievementAllDefinition();
    }
    public void GetAllProgress()
    {
        dataOutput.text = "> ";
        printOutputLine("GetAllProgress");
        getAchievementAllProgress();
    }
    public void UnLock()
    {
        dataOutput.text = "> ";
        printOutputLine("UnLock");
        string currentText = unLockInputField.text;

        if (currentText != "")
        {
            unlockAchievement(currentText);
            unLockInputField.text = "";
        }
        else
        {
            printOutputLine("Invalid Command");
        }
    }
    public void AddCount()
    {
        dataOutput.text = "> ";
        printOutputLine("AddCount");
        string currentText = addCountInputField.text;

        if (currentText != "")
        {
            string[] commandParams = currentText.Split(',');
            if (commandParams.Length > 1)
            {
                try
                {
                    Convert.ToInt64(commandParams[1]);
                }
                catch (Exception)
                {
                    printOutputLine("Invalid Command");
                    throw;
                }
                addCountAchievement(commandParams[0], commandParams[1]);
                addCountInputField.text = "";
            }
            else
            {
                printOutputLine("Invalid Command");
            }
        }
        else
        {
            printOutputLine("Invalid Command");
        }
    }
    public void AddFields()
    {
        dataOutput.text = "> ";
        printOutputLine("AddFields");
        string currentText = addFieldsInputField.text;

        if (currentText != "")
        {
            string[] commandParams = currentText.Split(',');
            if (commandParams.Length > 1)
            {
                addFieldsAchievement(commandParams[0], commandParams[1]);
                addFieldsInputField.text = "";
            }
            else
            {
                printOutputLine("Invalid Command");
            }
        }
        else
        {
            printOutputLine("Invalid Command");
        }
    }

    private void achievementInit()
    {
        Achievements.Init().OnComplete(initAchievementCallback);
    }

    private void getAchievementAllDefinition()
    {
        Achievements.GetAllDefinitions().OnComplete(achievementAllDefinitionCallback);
    }

    private void getAchievementAllProgress()
    {
        Achievements.GetAllProgress().OnComplete(achievementAllProgressCallback);
    }

    void addFieldsAchievement(string achievementName, string fields)
    {
        Achievements.AddFields(achievementName, fields).OnComplete(achievementFieldsCallback);
    }

    void addCountAchievement(string achievementName, string count)
    {
        Achievements.AddCount(achievementName, Convert.ToInt64(count)).OnComplete(achievementCountCallback);
    }

    void unlockAchievement(string achievementName)
    {
        Achievements.Unlock(achievementName).OnComplete(achievementUnlockCallback);
    }

    void getAchievementProgress(string[] achievementNames)
    {
        Achievements.GetProgressByName(achievementNames).OnComplete(achievementProgressCallback);
    }

    void getAchievementDefinition(string[] achievementNames)
    {
        Achievements.GetDefinitionsByName(achievementNames).OnComplete(achievementDefinitionCallback);
    }

    void printOutputLine(string newLine)
    {
        dataOutput.text = "> " + newLine + Environment.NewLine + dataOutput.text;
    }

    // Callbacks

    void achievementAllDefinitionCallback(Pvr_Message<Pvr_AchievementDefinitionList> msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Received achievement definitions success");
            Pvr_AchievementDefinitionList definitionList = msg.GetAchievementDefinitions();
            if (definitionList.HasNextPage)
            {
                Achievements.GetNextAchievementDefinitionListPage(definitionList).OnComplete(achievementAllDefinitionCallback);
            }

            foreach (var definition in definitionList)
            {
                printOutputLine("Bitfield Name: " + definition.Name.ToString());
                switch (definition.Type)
                {
                    case AchievementType.Simple:
                        printOutputLine("Achievement Type: Simple");
                        break;
                    case AchievementType.Bitfield:
                        printOutputLine("Achievement Type: Bitfield");
                        printOutputLine("Bitfield Length: " + definition.BitfieldLength.ToString());
                        printOutputLine("Target: " + definition.Target.ToString());
                        break;
                    case AchievementType.Count:
                        printOutputLine("Achievement Type: Count");
                        printOutputLine("Target: " + definition.Target.ToString());
                        break;
                    case AchievementType.Unknown:
                    default:
                        printOutputLine("Achievement Type: Unknown");
                        break;
                }
                printOutputLine("Bitfield Title: " + definition.Title.ToString());
                printOutputLine("Bitfield Description: " + definition.Description.ToString());
                printOutputLine("Bitfield UnlockedDescription: " + definition.UnlockedDescription.ToString());
                printOutputLine("Bitfield UnlockedIcon: " + definition.UnlockedIcon.ToString());
                printOutputLine("Bitfield LockedIcon: " + definition.LockedIcon.ToString());
                printOutputLine("Bitfield IsSecrect: " + definition.IsSecrect.ToString());
            }
        }
        else
        {
            printOutputLine("Received achievement definitions error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementAllProgressCallback(Pvr_Message<Pvr_AchievementProgressList> msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Received achievement progress success");
            Pvr_AchievementProgressList progressList = msg.GetAchievementProgressList();

            if (progressList.HasNextPage)
            {
                Achievements.GetNextAchievementProgressListPage(progressList).OnComplete(achievementAllProgressCallback);
            }

            foreach (var progress in progressList)
            {
                printOutputLine("Current Name: " + progress.Name.ToString());
                if (progress.IsUnlocked)
                {
                    printOutputLine("Achievement Unlocked");
                }
                else
                {
                    printOutputLine("Achievement Locked");
                }
                printOutputLine("Current Bitfield: " + progress.Bitfield.ToString());
                printOutputLine("Current Count: " + progress.Count.ToString());
                printOutputLine("Current UnlockTime: " + progress.UnlockTime.ToString());
            }
        }
        else
        {
            printOutputLine("Received achievement progress error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementFieldsCallback(Pvr_Message msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Achievement fields added.");
        }
        else
        {
            printOutputLine("Received achievement fields add error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementCountCallback(Pvr_Message msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Achievement count added.");
        }
        else
        {
            printOutputLine("Received achievement count add error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementUnlockCallback(Pvr_Message msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Achievement unlocked");
        }
        else
        {
            printOutputLine("Received achievement unlock error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementProgressCallback(Pvr_Message<Pvr_AchievementProgressList> msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Received achievement progress success");
            Pvr_AchievementProgressList progressList = msg.GetAchievementProgressList();

            foreach (var progress in progressList)
            {
                if (progress.IsUnlocked)
                {
                    printOutputLine("Achievement Unlocked");
                }
                else
                {
                    printOutputLine("Achievement Locked");
                }
                printOutputLine("Current Bitfield: " + progress.Bitfield.ToString());
                printOutputLine("Current Count: " + progress.Count.ToString());
            }
        }
        else
        {
            printOutputLine("Received achievement progress error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void achievementDefinitionCallback(Pvr_Message<Pvr_AchievementDefinitionList> msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Received achievement definitions success");
            Pvr_AchievementDefinitionList definitionList = msg.GetAchievementDefinitions();

            foreach (var definition in definitionList)
            {
                switch (definition.Type)
                {
                    case AchievementType.Simple:
                        printOutputLine("Achievement Type: Simple");
                        break;
                    case AchievementType.Bitfield:
                        printOutputLine("Achievement Type: Bitfield");
                        printOutputLine("Bitfield Length: " + definition.BitfieldLength.ToString());
                        printOutputLine("Target: " + definition.Target.ToString());
                        break;
                    case AchievementType.Count:
                        printOutputLine("Achievement Type: Count");
                        printOutputLine("Target: " + definition.Target.ToString());
                        break;
                    case AchievementType.Unknown:
                    default:
                        printOutputLine("Achievement Type: Unknown");
                        break;
                }
            }
        }
        else
        {
            printOutputLine("Received achievement definitions error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void initAchievementCallback(Pvr_Message msg)
    {
        if (!msg.IsError)
        {
            printOutputLine("Received init success");
        }
        else
        {
            printOutputLine("Received init error");
            Error error = msg.GetError();
            printOutputLine("Error: Message:" + error.Message + " Code: " + error.Code + " HttpCode: " + error.HttpCode);
        }
    }

    void LoginCallback(string LoginInfo)
    {
        Debug.Log("Received loginCallback:" + LoginInfo);
        printOutputLine("Received loginCallback: " + LoginInfo);
    }
}
