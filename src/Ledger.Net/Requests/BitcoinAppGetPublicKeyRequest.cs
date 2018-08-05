using System;

namespace Ledger.Net.Requests
{
    public class BitcoinAppGetPublicKeyRequest : RequestBase
    {
        public override byte Argument1 => throw new NotImplementedException();

        public override byte Argument2 => throw new NotImplementedException();

        public override byte Cla => Constants.CLA;

        public override byte Ins => Constants.BTCHIP_INS_GET_WALLET_PUBLIC_KEY;
    }
}
