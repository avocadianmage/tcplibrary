using System;

namespace TCPLibrary
{
    public sealed class MessageEventArgs : EventArgs
    {
        public byte[] Message { get { return message; } }

        readonly byte[] message;

        public MessageEventArgs(byte[] message)
        {
            this.message = message;
        }
    }
}