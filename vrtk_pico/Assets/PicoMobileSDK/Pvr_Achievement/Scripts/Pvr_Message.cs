using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public abstract class Pvr_Message<T> : Pvr_Message
    {
        public new delegate void Callback(Pvr_Message<T> message);
        public Pvr_Message(AndroidJavaObject msg) : base(msg)
        {
            if (!IsError)
            {
                data = GetDataFromMessage(msg);
            }
        }

        public T Data { get { return data; } }
        protected abstract T GetDataFromMessage(AndroidJavaObject msg);
        private T data;
    }

    public class Pvr_Message
    {
        public delegate void Callback(Pvr_Message message);
        public Pvr_Message(AndroidJavaObject msg)
        {
            type = (MessageType)Pvr_AchievementAPI.pvr_Message_GetType(msg);
            var isError = Pvr_AchievementAPI.pvr_Message_IsError(msg);
            requestID = Pvr_AchievementAPI.pvr_Message_GetRequestID(msg);
            if (isError)
            {
                error = new Error(
                  Pvr_AchievementAPI.pvr_Error_GetCode(msg),
                  Pvr_AchievementAPI.pvr_Error_GetMessage(msg),
                  Pvr_AchievementAPI.pvr_Error_GetHttpCode(msg));
            }
            else if (AchievementCore.LogMessages)
            {
                var message = Pvr_AchievementAPI.pvr_Message_GetString(msg);
                if (message != null)
                {
                    Debug.Log(message);
                }
                else
                {
                    Debug.Log(string.Format("null message string {0}", msg));
                }
            }
        }
        public enum MessageType : uint
        {
            Unknown,

            Achievements_AddCount = 0x03E76231,
            Achievements_AddFields = 0x14AA2129,
            Achievements_GetAllDefinitions = 0x03D3458D,
            Achievements_GetAllProgress = 0x4F9FDE1D,
            Achievements_GetDefinitionsByName = 0x629101BC,
            Achievements_GetNextAchievementDefinitionArrayPage = 0x2A7DD255,
            Achievements_GetNextAchievementProgressArrayPage = 0x2F42E727,
            Achievements_GetProgressByName = 0x152663B1,
            Achievements_Unlock = 0x593CCBDD,
            Achievements_WriteAchievementProgress = 0x736BBDD,
            Achievements_VerifyAccessToken = 0x032D103C
        };

        public MessageType Type { get { return type; } }
        public bool IsError { get { return error != null; } }
        public long RequestID { get { return requestID; } }

        private MessageType type;
        private long requestID;
        private Error error;

        public virtual Error GetError() { return error; }
        public virtual Pvr_AchievementDefinitionList GetAchievementDefinitions() { return null; }
        public virtual Pvr_AchievementProgressList GetAchievementProgressList() { return null; }
        public virtual Pvr_AchievementUpdate GetAchievementUpdate() { return null; }
        public virtual string GetString() { return null; }

        internal static Pvr_Message ParseMessageHandle(AndroidJavaObject messageHandle)
        {
            if (messageHandle == null)
            {
                return null;
            }

            Pvr_Message message = null;
            MessageType message_type = (MessageType)Pvr_AchievementAPI.pvr_Message_GetType(messageHandle);

            switch (message_type)
            {
                case MessageType.Achievements_GetAllDefinitions:
                case MessageType.Achievements_GetDefinitionsByName:
                case MessageType.Achievements_GetNextAchievementDefinitionArrayPage:
                    message = new MessageWithAchievementDefinitions(messageHandle);
                    break;

                case MessageType.Achievements_GetAllProgress:
                case MessageType.Achievements_GetNextAchievementProgressArrayPage:
                case MessageType.Achievements_GetProgressByName:
                    message = new MessageWithAchievementProgressList(messageHandle);
                    break;

                case MessageType.Achievements_AddCount:
                case MessageType.Achievements_AddFields:
                case MessageType.Achievements_Unlock:
                case MessageType.Achievements_VerifyAccessToken:
                    message = new MessageWithAchievementUpdate(messageHandle);
                    break;

            }

            return message;
        }

        public static Pvr_Message PopMessage()
        {
            var messageHandle = Pvr_AchievementAPI.PopMessage();

            Pvr_Message message = ParseMessageHandle(messageHandle);

            return message;
        }

        internal delegate Pvr_Message ExtraMessageTypesHandler(AndroidJavaObject messageHandle, MessageType message_type);
        internal static ExtraMessageTypesHandler HandleExtraMessageTypes { set; private get; }
    }


    public class MessageWithAchievementDefinitions : Pvr_Message<Pvr_AchievementDefinitionList>
    {
        public MessageWithAchievementDefinitions(AndroidJavaObject msg) : base(msg) { }
        public override Pvr_AchievementDefinitionList GetAchievementDefinitions() { return Data; }
        protected override Pvr_AchievementDefinitionList GetDataFromMessage(AndroidJavaObject msg)
        {
            return new Pvr_AchievementDefinitionList(msg);
        }

    }
    public class MessageWithAchievementProgressList : Pvr_Message<Pvr_AchievementProgressList>
    {
        public MessageWithAchievementProgressList(AndroidJavaObject msg) : base(msg) { }
        public override Pvr_AchievementProgressList GetAchievementProgressList() { return Data; }
        protected override Pvr_AchievementProgressList GetDataFromMessage(AndroidJavaObject msg)
        {
            return new Pvr_AchievementProgressList(msg);
        }

    }
    public class MessageWithAchievementUpdate : Pvr_Message<Pvr_AchievementUpdate>
    {
        public MessageWithAchievementUpdate(AndroidJavaObject msg) : base(msg) { }
        public override Pvr_AchievementUpdate GetAchievementUpdate() { return Data; }
        protected override Pvr_AchievementUpdate GetDataFromMessage(AndroidJavaObject msg)
        {
            return new Pvr_AchievementUpdate(msg);
        }

    }
    public class MessageWithString : Pvr_Message<string>
    {
        public MessageWithString(AndroidJavaObject msg) : base(msg) { }
        public override string GetString() { return Data; }
        protected override string GetDataFromMessage(AndroidJavaObject msg)
        {
            return Pvr_AchievementAPI.pvr_Message_GetString(msg);
        }
    }
    public class Error
    {
        public Error(int code, string message, int httpCode)
        {
            Message = message;
            Code = code;
            HttpCode = httpCode;
        }

        public readonly int Code;
        public readonly int HttpCode;
        public readonly string Message;
    }

}
