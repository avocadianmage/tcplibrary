﻿using System.Text;

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
    }
}