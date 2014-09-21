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
        /// Buffer storing the byte data that will be converted to a 32-bit
        /// integer denoting the length of the message.
        /// </summary>
        BufferObject messagePrefixBuffer;

        /// <summary>
        /// The message being received. If null, then no message is currently 
        /// in the process of being received.
        /// </summary>
        BufferObject messageBuffer;

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
        /// Perform the socket operation to begin receiving data from the 
        /// socket.
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
                if (messageBuffer == null)
                {
                    // Initialize the message prefix buffer.
                    if (messagePrefixBuffer == null)
                    {
                        messagePrefixBuffer 
                            = new BufferObject(Global.MessagePrefixLength);
                    }

                    // Read bytes in the message prefix and update the amount
                    // of bytes that have been processed.
                    bytesProcessed += messagePrefixBuffer.ReadBufferData(
                        connection.Buffer,
                        bytesProcessed,
                        connection.BytesTransferred);

                    // Do not start reading the message until the prefix has
                    // been fully read.
                    if (!messagePrefixBuffer.IsFull) continue;

                    // Initialize the message buffer.
                    int length = BitConverter.ToInt32(
                        messagePrefixBuffer.Data, 
                        0);
                    messageBuffer = new BufferObject(length);
                }

                // Read bytes in the message and update the amount of bytes
                // that have been processed.
                bytesProcessed += messageBuffer.ReadBufferData(
                    connection.Buffer,
                    bytesProcessed,
                    connection.BytesTransferred);

                // Check if the message has been fully received.
                if (messageBuffer.IsFull)
                {
                    // Fire event indicating we have finished reading the 
                    // message.
                    MessageReceived(
                        this, 
                        new MessageEventArgs(messageBuffer.Data));

                    // Reset for next read.
                    reset();
                }
            }
        }

        // Reset the module for the next message.
        void reset()
        {
            messagePrefixBuffer = null;
            messageBuffer = null;
        }
    }
}