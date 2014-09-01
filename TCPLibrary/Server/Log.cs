using System;
using System.Net;
using System.Net.Sockets;

namespace TCPLibrary.Server
{
    static class Log
    {
        public static void WriteStartup()
        {
            Console.WriteLine("Server is online.");
            Console.WriteLine("Press any key to terminate the process...");
            Console.ReadKey();
        }

        public static void WriteConnection(Connection connection, bool connected)
        {
            writeClientInfo(connection);
            Console.WriteLine(
                "connection {0}.", 
                connected ? "established" : "terminated");
        }

        public static void WriteCommunication(
            Connection connection, 
            string message,
            bool received)
        {
            writeClientInfo(connection);
            Console.WriteLine(
                "{0} {1}",
                received ? "<--" : "-->", 
                message);
        }

        static void writeClientInfo(Connection connection)
        {
            Console.Write("[{0}] {1} ", 
                DateTime.Now.ToString("MM.dd.yyyy hh:mm:ss.ff"),
                connection.Socket.RemoteEndPoint);
        }
    }
}