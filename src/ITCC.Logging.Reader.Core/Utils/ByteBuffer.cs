// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.IO;
using System.Text;

namespace ITCC.Logging.Reader.Core.Utils
{
    internal class ByteBuffer
    {
        #region public

        public ByteBuffer(int capacity)
        {
            Data = new byte[capacity];
            Capacity = capacity;
            Count = 0;
        }

        public bool Add(byte[] data) => Add(data, 0);

        public bool Add(byte[] data, int offset) => Add(data, offset, data.Length - offset);

        public bool Add(byte[] data, int offset, int count)
        {
            if (count > Capacity - Count)
                return false;
            Array.Copy(data, offset, Data, Count, count);
            Count += count;
            return true;
        }

        public bool CopyFrom(ByteBuffer buffer) => CopyFrom(buffer, buffer.Count);

        public bool CopyFrom(ByteBuffer buffer, int count)
        {
            if (FreeSize < count)
                return false;

            Array.Copy(buffer.Data, 0, Data, Count, count);
            Count += count;
            return true;
        }

        public void TruncateStart(int index)
        {
            for (var i = 0; i < index; ++i)
            {
                Data[i] = Data[i + index];
            }
            Count -= index;
        }

        public void Flush()
        {
            Count = 0;
        }

        public int ReadStream(Stream stream)
        {
            var readCount = stream.Read(Data, Count, FreeSize);
            Count += readCount;
            return readCount;
        }

        public string ToUtf8String() => Encoding.UTF8.GetString(Data, 0, Count);

        public int Capacity { get; }

        public int Count { get; private set; }

        public byte[] Data { get; }

        public int FreeSize => Capacity - Count;
        public bool IsFull => Count == Capacity;

        #endregion
    }
}
