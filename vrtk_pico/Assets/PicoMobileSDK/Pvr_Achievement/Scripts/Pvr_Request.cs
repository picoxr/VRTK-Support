using UnityEngine;

namespace Pvr_UnitySDKAPI.Achievement
{
    public sealed class Pvr_Request<T> : Request
    {
        private Pvr_Message<T>.Callback callback_ = null;

        public Pvr_Request(long requestID) : base(requestID) { }

        public Pvr_Request<T> OnComplete(Pvr_Message<T>.Callback callback)
        {
            if (callback_ != null)
            {
                throw new UnityException("Attempted to attach multiple handlers to a Request.  This is not allowed.");
            }

            callback_ = callback;
            Pvr_Callback.AddRequest(this);
            return this;
        }

        override public void HandleMessage(Pvr_Message msg)
        {
            if (!(msg is Pvr_Message<T>))
            {
                Debug.LogError("Unable to handle message: " + msg.GetType());
                return;
            }

            if (callback_ != null)
            {
                callback_((Pvr_Message<T>)msg);
                return;
            }

            throw new UnityException("Request with no handler.  This should never happen.");
        }
    }

    public class Request
    {
        private Pvr_Message.Callback callback_;

        public Request(long requestID) { RequestID = requestID; }
        public long RequestID { get; set; }

        public Request OnComplete(Pvr_Message.Callback callback)
        {
            callback_ = callback;
            Pvr_Callback.AddRequest(this);
            return this;
        }

        virtual public void HandleMessage(Pvr_Message msg)
        {
            if (callback_ != null)
            {
                callback_(msg);
                return;
            }

            throw new UnityException("Request with no handler.  This should never happen.");
        }

        public static void RunCallbacks(uint limit = 0)
        {
            // default of 0 will run callbacks on all messages on the queue
            if (limit == 0)
            {
                Pvr_Callback.RunCallbacks();
            }
            else
            {
                Pvr_Callback.RunLimitedCallbacks(limit);
            }
        }
    }
}
