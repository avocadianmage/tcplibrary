using System;

namespace TCPLibrary.Modules
{
    sealed class BufferObject
    {
        /// <summary>
        /// Return true if the buffer data is fully populated and there is no
        /// more to read.
        /// </summary>
        public bool IsFull { get { return index == data.Length; } }

        /// <summary>
        /// Return the raw buffer data.
        /// </summary>
        public byte[] Data { get { return data; } }

        readonly byte[] data;
        int index;

        public BufferObject(int length)
        {
            data = new byte[length];
            index = 0;
        }

        /// <summary>
        /// Copy bytes from the specified buffer to this buffer.
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceBufferIndex"></param>
        /// <param name="sourceBufferBytesTransferred"></param>
        /// <returns>The number of bytes that were read.</returns>
        public int ReadBufferData(
            byte[] sourceBuffer, 
            int sourceBufferIndex,
            int sourceBufferBytesTransferred)
        {
            // Calculate the number of bytes to read.
            int bytesToRead = Math.Min(
                data.Length - index,
                sourceBufferBytesTransferred - sourceBufferIndex);

            // Perform the copy operation.
            Array.Copy(
                sourceBuffer,
                sourceBufferIndex,
                data,
                index,
                bytesToRead);

            // Update index.
            index += bytesToRead;

            // Return the number of bytes read.
            return bytesToRead;
        }
    }
}