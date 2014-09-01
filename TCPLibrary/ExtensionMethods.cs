using System;

namespace TCPLibrary
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Safely invoke this EventHandler, avoiding a NullReferenceException.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        public static void SafeInvoke(this EventHandler handler, object sender)
        {
            if (handler != null) handler(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Safely invoke this EventHandler, avoiding a NullReferenceException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void SafeInvoke<T>(
            this EventHandler<T> handler,
            object sender, T args) where T : EventArgs
        {
            if (handler != null) handler(sender, args);
        }
    }
}