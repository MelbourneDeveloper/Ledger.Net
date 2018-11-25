using System.IO;

namespace Ledger.Net
{
    internal static class EntensionMethods
    {
        internal static byte[] ToBytes(this uint value)
        {
            return new[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }

        internal static byte[] ReadAllBytes(this Stream stream, int totalByteCount)
        {
            var data = new byte[totalByteCount];
            var totalReadCount = 0;
            int readCount;
            do
            {
                totalReadCount += (readCount = stream.Read(data, totalReadCount, totalByteCount - totalReadCount));
            } while (readCount > 0 && totalReadCount < totalByteCount);
            return data;
        }
    }
}
