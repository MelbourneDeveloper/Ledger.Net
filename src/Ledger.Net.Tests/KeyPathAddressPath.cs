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

        public uint Change => KeyPath.Indexes[3];

        public uint AddressIndex => KeyPath.Indexes[4];

        public KeyPathAddressPath(KeyPath keyPath)
        {
            KeyPath = keyPath;
        }

        public uint[] ToUnhardenedArray()
        {
            return new uint[5] { Purpose, CoinType, Account, Change, AddressIndex };
        }

        public uint[] ToHardenedArray()
        {
            return new uint[5] { KeyPath.Indexes[0], KeyPath.Indexes[1], KeyPath.Indexes[2], KeyPath.Indexes[3], KeyPath.Indexes[4] };
        }
    }
}
