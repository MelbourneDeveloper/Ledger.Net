using Hardwarewallets.Net.AddressManagement;
using Hardwarewallets.Net.Model;
using NBitcoin;

namespace Ledger.Net.Tests
{
    public class KeyPathAddressPath : IAddressPath
    {
        private KeyPath KeyPath { get; set; }

        public uint Purpose => AddressUtilities.UnhardenNumber(KeyPath.Indexes[0]);

        public uint CoinType => AddressUtilities.UnhardenNumber(KeyPath.Indexes[1]);

        public uint Account => AddressUtilities.UnhardenNumber(KeyPath.Indexes[2]);

        public uint Change => KeyPath.Indexes.Length == 5 ? KeyPath.Indexes[3] : 0;

        public uint AddressIndex => KeyPath.Indexes.Length == 4 ? KeyPath.Indexes[3] : KeyPath.Indexes[4];

        public KeyPathAddressPath(KeyPath keyPath)
        {
            KeyPath = keyPath;
        }

        public uint[] ToUnhardenedArray()
        {
            if (KeyPath.Indexes.Length == 5)
            {
                return new uint[5] { Purpose, CoinType, Account, Change, AddressIndex };
            }
            else
            {
                return new uint[4] { Purpose, CoinType, Account, AddressIndex };
            }
        }

        public uint[] ToHardenedArray()
        {
            if (KeyPath.Indexes.Length == 5)
            {
                return new uint[5] { KeyPath.Indexes[0], KeyPath.Indexes[1], KeyPath.Indexes[2], KeyPath.Indexes[3], KeyPath.Indexes[4] };
            }
            else
            {
                return new uint[4] { KeyPath.Indexes[0], KeyPath.Indexes[1], KeyPath.Indexes[2], KeyPath.Indexes[3] };
            }
        }
    }
}
