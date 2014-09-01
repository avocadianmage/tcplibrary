using System.Text;

namespace TCPLibrary
{
    static class Global
    {
        /// <summary>
        /// Size of receive buffer.
        /// </summary>
        public static int BufferSize { get { return 128; } }

        /// <summary>
        /// The maximum number of client allowed in the queue while waiting for
        /// connection to the server.
        /// </summary>
        public static int ConnectionBacklog { get { return 100; } }

        /// <summary>
        /// Convert the specified string into a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] StringToBytes(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        /// <summary>
        /// Convert the specified byte array into a string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToString(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }
    }
}