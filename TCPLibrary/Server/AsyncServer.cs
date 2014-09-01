using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPLibrary.Server
{
    /// <summary>
    /// Implements the connection logic for the socket server.
    /// </summary>
    public sealed class AsyncServer
    {
        /// <summary>
        /// This event fires when a client connection has been successfully 
        /// accepted.
        /// </summary>
        public event EventHandler Accepted;

        /// <summary>
        /// This event first when a message has been received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event fires when a client connection has terminated.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Semaphore for limiting client connections.
        /// </summary>
        Semaphore maxAcceptedClients;

        /// <summary>
        /// List of all active connections to clients.
        /// </summary>
        IList<Connection> clientConnections;

        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        Socket listenSocket;

        public AsyncServer(int maxConnections)
        {
            maxAcceptedClients = new Semaphore(maxConnections, maxConnections);
            clientConnections = new List<Connection>(maxConnections);
        }

        /// <summary>
        /// Starts the server such that it is listening for incoming connection 
        /// requests.
        /// </summary>
        public void Start(int port)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = host.AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create the socket which listens for incoming connections.
            listenSocket = new Socket(
                localEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);

            // Start the server.
            listenSocket.Listen(Global.ConnectionBacklog);

            // Post accepts on the listening socket.
            startAccept();

            // Output that the server is now running.
            Log.WriteStartup();
        }

        /// <summary>
        /// Send the specified message to the specified clients.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        /// <param name="recipients"></param>
        public void Send(
            string message, 
            Connection connection, 
            Recipients recipients)
        {
            // Build the list of client recipients to send the message to.
            IEnumerable<Connection> recipientList;
            switch (recipients)
            {
                case Recipients.All:
                    recipientList = clientConnections;
                    break;
                case Recipients.AllExceptThis:
                    recipientList = clientConnections
                        .Where(recipient => recipient != connection);
                    break;
                case Recipients.This:
                    recipientList = new List<Connection> { connection };
                    break;
                default: throw new NotSupportedException(
                    "This Recipients type is not supported.");
            }

            // Send iterate through all recipients.
            foreach (Connection recipient in recipientList)
            {
                // Send message to recipient.
                byte[] data = Global.StringToBytes(message);
                recipient.Send(data);

                // Output that the message was sent.
                Log.WriteCommunication(
                    recipient, 
                    Global.BytesToString(data), 
                    false);
            }
        }

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        void startAccept()
        {
            // Decrement the semaphore pool.
            maxAcceptedClients.WaitOne();

            // Initialize connection object representing a connection to the
            // client.
            Connection connection = new Connection();
            connection.Accepted += onAccepted;
            connection.Closed += onClosed;
            connection.MessageReceived += onMessageReceived;
            clientConnections.Add(connection);
            
            // Accept the connection request from the client.
            connection.Accept(listenSocket);
        }

        /// <summary>
        /// Event that fires when a client connection has been closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onClosed(object sender, EventArgs e)
        {
            Connection connection = sender as Connection;

            // Output that the client has disconnected.
            Log.WriteConnection(connection, false);

            // Remove connection from list.
            clientConnections.Remove(connection);

            // Increment the semaphore pool.
            maxAcceptedClients.Release();

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
            // Output the message that was received from the client.
            Log.WriteCommunication(sender as Connection, e.Message, true);

            // Fire message-received event.
            MessageReceived.SafeInvoke(sender, e);
        }

        /// <summary>
        /// Event that fires when a connection with a client has been accepted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onAccepted(object sender, EventArgs e)
        {
            Connection connection = sender as Connection;

            // Output that a new connection with a client has been established.
            Log.WriteConnection(sender as Connection, true);

            // Fire accepted event.
            Accepted.SafeInvoke(sender);

            // Accept the next connection request.
            startAccept();
        }
    }
}