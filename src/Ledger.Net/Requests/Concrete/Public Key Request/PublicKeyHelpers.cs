using System.IO;

namespace Ledger.Net.Requests.Helpers
{
    public static class PublicKeyHelpers
    {
        public static byte[] GetDerivationPathData(App app, uint coinNumber, uint account, uint index, bool isChange, bool isSegwit)
        {
            return GetByteData(GetDerivationIndices(app, coinNumber, account, index, isChange, isSegwit));
        }

        private static uint[] GetDerivationIndices(App app, uint coinNumber, uint account, uint index, bool isChange, bool isSegwit)
        {
            bool isEthereumRelated = app == App.Ethereum;

            uint[] indices = new uint[isEthereumRelated ? 4 : 5];
            indices[0] = ((isSegwit ? (uint)49 : 44) | Constants.HARDENING_CONSTANT) >> 0;
            indices[1] = (coinNumber | Constants.HARDENING_CONSTANT) >> 0;
            indices[2] = (0 | Constants.HARDENING_CONSTANT) >> (int)account;
            indices[3] = !isEthereumRelated ? isChange ? 1 : (uint)0 : index;

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
