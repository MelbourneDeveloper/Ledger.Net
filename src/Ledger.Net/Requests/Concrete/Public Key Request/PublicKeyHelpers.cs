using System.IO;

namespace Ledger.Net.Requests.Helpers
{
    public static class PublicKeyHelpers
    {
        public static byte[] GetDerivationPathData(uint coinNumber, uint account, uint index, bool isChange, bool isSegwit)
        {
            return GetByteData(GetDerivationIndices(coinNumber, account, index, isChange, isSegwit));
        }

        private static uint[] GetDerivationIndices(uint coinNumber, uint account, uint index, bool isChange, bool isSegwit)
        {
            // if coinNumber == 60, the coin is Ethereum, so we need one less array index
            uint[] indices = new uint[coinNumber == 60 ? 4 : 5];
            indices[0] = ((isSegwit ? (uint)49 : 44) | Constants.HARDENING_CONSTANT) >> 0;
            indices[1] = (coinNumber | Constants.HARDENING_CONSTANT) >> 0;
            indices[2] = (0 | Constants.HARDENING_CONSTANT) >> (int)account;
            indices[3] = coinNumber != 60 ? isChange ? 1 : (uint)0 : index; 
            // If coinNumber == 60 (ethereum) we use index instead of change.

            // If coinNumber is != 60, we don't have Ethereum, so add in the address index to the last index of array
            if (indices.Length == 5) indices[4] = index;

            return indices;
        }

        private static byte[] GetByteData(uint[] indices)
        {
            byte[] addressIndicesData;
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.WriteByte((byte)indices.Length);
                for (var i = 0; i < indices.Length; i++)
                {
                    var data = indices[i].ToBytes();
                    memoryStream.Write(data, 0, data.Length);
                }
                addressIndicesData = memoryStream.ToArray();
            }

            return addressIndicesData;
        }
    }
}
