using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace TCPLibrary.Client
{
    public sealed class AsyncClient
    {
        /// <summary>
        /// This event fires when a connection has been successfully established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event first when a message has been received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event fires when the connection has terminated.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Connection object that handles communication with the server.
        /// </summary>
        Connection connection;

        /// <summary>
        /// Connect to a remote device.
        /// </summary>
        public void Start(string ipAddress, int port)
        {
            // Establish the remote endpoint for the socket.
            IPAddress remoteAddress = IPAddress.Parse(ipAddress);
            IPEndPoint remoteEndPoint = new IPEndPoint(remoteAddress, port);

            // Create a TCP/IP socket.
            Socket clientSocket = new Socket(
                remoteEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Initialize connection object.
            connection = new Connection();
            connection.Connected += onConnected;
            connection.Closed += onClosed;
            connection.MessageReceived += onMessageReceived;

            // Connect to the remote endpoint.
            connection.Connect(clientSocket, remoteEndPoint);
        }

        /// <summary>
        /// Send the specified message to the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void Send(byte[] message)
        {
            connection.Send(message);
        }

        /// <summary>
        /// Event that fires when a connection with the server has been established.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onConnected(object sender, EventArgs e)
        {
            // Fire connected event.
            Connected.SafeInvoke(sender);
        }

        /// <summary>
        /// Event that fires when the connection with the server has been terminated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onClosed(object sender, EventArgs e)
        {
            // Fire closed event.
            Closed.SafeInvoke(sender);
        }

        /// <summary>
        /// Event that fires when a message has been fully received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        void onMessageReceived(object sender, MessageEventArgs e)
        {
            // Fire message-received event.
            MessageReceived.SafeInvoke(sender, e);
        }
    }
}