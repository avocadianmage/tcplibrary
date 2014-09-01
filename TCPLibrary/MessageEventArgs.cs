using System;

namespace TCPLibrary
{
    public sealed class MessageEventArgs : EventArgs
    {
        public string Message { get { return message; } }

        readonly string message;

        public MessageEventArgs(byte[] data)
        {
            this.message = Global.BytesToString(data);
        }
    }
}