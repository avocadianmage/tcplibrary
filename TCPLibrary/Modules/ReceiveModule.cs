using System;
using System.Net.Sockets;

namespace TCPLibrary.Modules
{
    sealed class ReceiveModule
    {
        /// <summary>
        /// Fires when a message has been fully received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// The message being received. If null, then no message is currently in
        /// the process of being received.
        /// </summary>
        byte[] receivedMessage;

        /// <summary>
        /// The number of bytes of the message being received that have been read so
        /// far.
        /// </summary>
        int receivedMessageBytesRead;

        /// <summary>
        /// The connection used to perform socket receive operations.
        /// </summary>
        readonly Connection connection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connection"></param>
        public ReceiveModule(Connection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Perform the socket operation to begin receiving data from the socket.
        /// </summary>
        public void Start()
        {
            if (!connection.Socket.ReceiveAsync(connection)) Callback();
        }

        /// <summary>
        /// Read the message that we received.
        /// </summary>
        public void Callback()
        {
            // Check if the operation failed.
            if (connection.BytesTransferred <= 0
                || connection.SocketError != SocketError.Success)
            {
                // The connection has already been terminated, so release the 
                // socket.
                connection.Close();
                return;
            }

            // Read the buffer for messages.
            parseBuffer();

            // Continue reading.
            Start();
        }

        /// <summary>
        /// Read the current contents of the buffer for messages.
        /// </summary>
        void parseBuffer()
        {
            int bytesProcessed = 0;
            while (bytesProcessed < connection.BytesTransferred)
            {
                // Check if a new message has started.
                if (receivedMessage == null)
                {
                    // Read the prefix of the new message.
                    int length = connection.Buffer[bytesProcessed++];

                    // Prepare to read the message.
                    receivedMessage = new byte[length];
                    receivedMessageBytesRead = 0;
                }

                // Read bytes in the message.
                int bytesToRead = Math.Min(
                    receivedMessage.Length - receivedMessageBytesRead,
                    connection.BytesTransferred - bytesProcessed);
                Array.Copy(
                    connection.Buffer,
                    bytesProcessed,
                    receivedMessage,
                    receivedMessageBytesRead,
                    bytesToRead);

                // Update the amount of bytes that have been read for the current
                // message.
                receivedMessageBytesRead += bytesToRead;

                // Update the amount of bytes in the buffer that have been read.
                bytesProcessed += bytesToRead;

                // Check if the message has been fully received.
                if (receivedMessageBytesRead == receivedMessage.Length)
                {
                    // Fire event indicating we have finished reading the message.
                    MessageReceived(this, new MessageEventArgs(receivedMessage));

                    // Reset for next read.
                    receivedMessage = null;
                }
            }
        }
    }
}