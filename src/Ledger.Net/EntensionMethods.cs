using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net
{
    internal static class EntensionMethods
    {
        internal static byte[] ToBytes(this uint value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value,
            };
        }
    }
}
