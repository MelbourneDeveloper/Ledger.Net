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
            //BIP 44 - https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki
            //Except for Ethereum (https://ledger.readthedocs.io/en/latest/background/hd_use_cases.html)
            //Coin Numbers here: https://github.com/satoshilabs/slips/blob/master/slip-0044.md

            var isEthereumRelated = app == App.Ethereum;

            var indices = new uint[isEthereumRelated ? 4 : 5];

            //Purpose
            indices[0] = ((isSegwit ? (uint)49 : 44) | Constants.HARDENING_CONSTANT) >> 0;

            //Coin type (Coin Number)
            indices[1] = (coinNumber | Constants.HARDENING_CONSTANT) >> 0;

            //Account
            indices[2] = (account | Constants.HARDENING_CONSTANT) >> 0;

            if (isEthereumRelated)
            {
                //BIP44 Deviation for Ledger
                //Index
                indices[3] =  index;
            }
            else
            {
                //Change
                indices[3] = isChange ? 1 : (uint)0;

                //Index
                indices[4] = index;
            }

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
