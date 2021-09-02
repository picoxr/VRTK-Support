using System.Collections.Generic;
using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public static class Pvr_Callback
    {
        #region Adding and running request handlers
        internal static void AddRequest(Request request)
        {
            if (request.RequestID <= 2)
            {
                switch (request.RequestID)
                {
                    case 0:
                        Debug.LogError("An PARAM_INVALIDE error occurred. Request failed.");
                        break;
                    case 1:
                        Debug.LogError("An NETWORK_INVALIDE error occurred. Request failed.");
                        break;
                    case 2:
                        Debug.LogError("An NOT_INTIALIZE error occurred. Request failed.");
                        break;
                    default:
                        Debug.LogError("An unknown error occurred. Request failed.");
                        break;
                }
                return;
            }
            requestIDsToRequests[request.RequestID] = request;
        }

        internal static void RunCallbacks()
        {
            while (true)
            {
                var msg = Pvr_Message.PopMessage();
                if (msg == null)
                {
                    break;
                }

                HandleMessage(msg);
            }

        }

        internal static void RunLimitedCallbacks(uint limit)
        {
            for (var i = 0; i < limit; ++i)
            {
                var msg = Pvr_Message.PopMessage();
                if (msg == null)
                {
                    break;
                }

                HandleMessage(msg);
            }
        }

        internal static void OnApplicationQuit()
        {
            requestIDsToRequests.Clear();
            notificationCallbacks.Clear();
        }

        #endregion

        #region Callback Internals
        private static Dictionary<long, Request> requestIDsToRequests = new Dictionary<long, Request>();
        private static Dictionary<Pvr_Message.MessageType, RequestCallback> notificationCallbacks = new Dictionary<Pvr_Message.MessageType, RequestCallback>();



        private class RequestCallback
        {
            private Pvr_Message.Callback messageCallback;

            public RequestCallback() { }

            public RequestCallback(Pvr_Message.Callback callback)
            {
                this.messageCallback = callback;
            }

            public virtual void HandleMessage(Pvr_Message msg)
            {
                if (messageCallback != null)
                {
                    messageCallback(msg);
                }
            }
        }

        private sealed class RequestCallback<T> : RequestCallback
        {
            private Pvr_Message<T>.Callback callback;
            public RequestCallback(Pvr_Message<T>.Callback callback)
            {
                this.callback = callback;
            }

            public override void HandleMessage(Pvr_Message msg)
            {
                if (callback != null)
                {

                    if (msg is Pvr_Message<T>)
                    {
                        callback((Pvr_Message<T>)msg);
                    }
                    else
                    {
                        Debug.LogError("Unable to handle message: " + msg.GetType());
                    }
                }
            }
        }

        internal static void HandleMessage(Pvr_Message msg)
        {
            Request request;
            if (msg.RequestID != 0 && requestIDsToRequests.TryGetValue(msg.RequestID, out request))
            {
                try
                {
                    request.HandleMessage(msg);
                }
                finally
                {
                    requestIDsToRequests.Remove(msg.RequestID);
                }
                return;
            }

            RequestCallback callbackHolder;
            if (notificationCallbacks.TryGetValue(msg.Type, out callbackHolder))
            {
                callbackHolder.HandleMessage(msg);
            }
        }

        #endregion
    }
}
