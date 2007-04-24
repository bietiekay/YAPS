using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// This implements a Ring Buffer type of data storage that holds video data as long as 
    /// </summary>
    class TimeShiftProcessor
    {
        private List<byte[]> RingBuffer;

        private int MaxNumberOfBufferElements;

        // Create the buffer
        public TimeShiftProcessor(int NumberOfBuffers)
        {
            MaxNumberOfBufferElements = NumberOfBuffers;

            RingBuffer = new List<byte[]>();
        }

        #region Read from the Buffer
        public byte[] TimeShiftRead()
        {
            byte[] returnElement = null;

            if (RingBuffer.Count > 0)
            {
                lock (RingBuffer)
                {
                    // get the first element
                    returnElement = RingBuffer[0];
                    // and remove it from the buffer
                    RingBuffer.RemoveAt(0);
                }
            }

            return returnElement;
        }
        #endregion

        #region Write to the Buffer
        public bool TimeShiftWrite(byte[] buffer, int length)
        {
            // TODO: add something to not only store the data into memory but also on harddisk (more space available...)

            try
            {
                lock (RingBuffer)
                {
                    // so let's check if we have to delete an old buffer first
                    if (RingBuffer.Count == MaxNumberOfBufferElements)
                        RingBuffer.RemoveAt(0);

                    RingBuffer.Add(buffer);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
