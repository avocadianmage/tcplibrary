using System;
using System.Linq;
using System.Net.Sockets;

namespace TCPLibrary.Modules
{
    sealed class SendModule
    {
        readonly Connection connection;

        public SendModule(Connection connection)
        {
            this.connection = connection;
        }

        public void SendMessage(byte[] message)
        {
            // Do not send a message if the socket is closed.
            if (connection.Socket == null) return;

            // Create new SocketAsyncEventArgs object for the send operation.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += onCompleted;

            // Build byte message with prefixed length.
            byte[] prefix = BitConverter.GetBytes(message.Length);
            byte[] data = prefix.Concat(message).ToArray();

            // Populate buffer with data.
            args.SetBuffer(data, 0, data.Length);

            // Execute the socket operation.
            if (!connection.Socket.SendAsync(args)) onCompleted(this, args);
        }

        void onCompleted(object sender, SocketAsyncEventArgs args)
        {
            // Check if the operation failed.
            if (connection.SocketError != SocketError.Success)
            {
                connection.Close();
            }
        }
    }
}