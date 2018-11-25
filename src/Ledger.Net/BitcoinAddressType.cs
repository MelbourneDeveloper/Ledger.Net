using System;

namespace Ledger.Net
{
    [Flags]
    public enum BitcoinAddressType
    {
        Legacy = 0x00,
        Segwit = 0x01,
        NativeSegwit = 0x02
    }
}
