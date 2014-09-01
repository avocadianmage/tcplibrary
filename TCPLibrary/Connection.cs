using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using TCPLibrary.Modules;

namespace TCPLibrary
{
    public sealed class Connection : SocketAsyncEventArgs
    {
        /// <summary>
        /// The socket associated with this connection.
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// Event handler that fires when a message has been fully received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Event handler that fires when a connection has terminated.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Event handler that fires when a connection has been accepted.
        /// </summary>
        public event EventHandler Accepted;

        /// <summary>
        /// Event handler that fires when a connection has succeeded.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Object for handling message receive implementation.
        /// </summary>
        readonly ReceiveModule receiveModule;

        /// <summary>
        /// Object for handling message send implementation.
        /// </summary>
        readonly SendModule sendModule;

        /// <summary>
        /// Initialize the connection.
        /// </summary>
        public Connection()
            : base()
        {
            Completed += onCompleted;

            // Initialize receive operation handling.
            receiveModule = new ReceiveModule(this);
            receiveModule.MessageReceived += (sender, message) =>
            {
                MessageReceived(this, message);
            };

            // Initialize send operation handling.
            sendModule = new SendModule(this);
        }

        /// <summary>
        /// Event that fires when a socket operation has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    processAccept();
                    break;
                case SocketAsyncOperation.Connect:
                    processConnect();
                    break;
                case SocketAsyncOperation.Receive:
                    receiveModule.Callback();
                    break;
                case SocketAsyncOperation.Send:
                    // Do nothing.
                    break;
                default: throw new InvalidOperationException(
                    "An invalid socket operation was executed.");
            }
        }

        /// <summary>
        /// Perform a socket operation to connect to the remote endpoint.
        /// </summary>
        /// <param name="clientSocket">The socket to use to connect.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        public void Connect(Socket clientSocket, EndPoint remoteEndPoint)
        {
            RemoteEndPoint = remoteEndPoint;
            if (!clientSocket.ConnectAsync(this)) processConnect();
        }

        /// <summary>
        /// This method executes after a connection has been established.
        /// </summary>
        void processConnect()
        {
            // Prepare for communication.
            prepareForCommunication(ConnectSocket);

            // Fire event indicating a connection has been established.
            if (Connected != null) Connected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Perform a socket operation to accept a connection with a remote
        /// end point.
        /// </summary>
        /// <param name="listenSocket"></param>
        public void Accept(Socket listenSocket)
        {
            if (!listenSocket.AcceptAsync(this)) processAccept();
        }

        /// <summary>
        /// This method executes after a connection has been accepted.
        /// </summary>
        void processAccept()
        {
            // Prepare for communication.
            prepareForCommunication(AcceptSocket);

            // Fire event indicating the connection has been accepted.
            if (Accepted != null) Accepted(this, EventArgs.Empty);
        }

        void prepareForCommunication(Socket socket)
        {
            // Set the appropriate socket.
            Socket = socket;

            // Initialize buffer for communication.
            SetBuffer(new byte[Global.BufferSize], 0, Global.BufferSize);

            // Start receiving messages.
            receiveModule.Start();
        }

        /// <summary>
        /// Perform a socket operation to send the specified message.
        /// </summary>
        /// <param name="message">The message, in bytes, to be sent.</param>
        public void Send(byte[] message)
        {
            sendModule.SendMessage(message);
        }

        /// <summary>
        /// Release the socket associated with this connection.
        /// </summary>
        public void Close()
        {
            // Fire event indicating the connection has closed.
            if (Closed != null) Closed(this, EventArgs.Empty);

            // Close the socket.
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Socket = null;
        }
    }
}