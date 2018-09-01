using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Ledger.Net
{
    public static class EthHelpers
    {
        private const string Format = "X1";
        private static readonly Encoding Encoding = new UTF8Encoding();

        public static byte[] GetTransactionData(byte[] derivationPathData, string hexNonce, string hexGasPrice, string hexGasLimit, string addressTo, string hexValue, string data, string hexChainId)
        {
            // https://github.com/LedgerHQ/ledger-app-eth/blob/master/doc/ethapp.asc
            // See "SIGN ETH TRANSACTION" section

            byte[] transactionData;
            using (var memoryStream = new MemoryStream())
            {
                // We need to write the path data as well when the first argument is 0x00, and not when it is 0x80?
                // New info found: https://github.com/LedgerHQ/ledgerjs/blob/master/packages/hw-app-eth/src/Eth.js#L147
                // The first data block that is sent needs to send with 0x00, and then the rest of them need to be sent with 0x80.
                if (derivationPathData != null)
                {
                    memoryStream.Write(derivationPathData, 0, derivationPathData.Length);
                }

                byte[] combinedByteData = hexNonce.ToHexBytes().Concat(hexGasPrice.ToHexBytes())
                                                               .Concat(hexGasLimit.ToHexBytes())
                                                               .Concat(addressTo.ToHexBytes())
                                                               .Concat(hexValue.ToHexBytes())
                                                               .Concat(data.ToHexBytes())
                                                               .ToArray();

                // v, r, s is included because it seems like it is needed based on a couple of sources.
                // https://github.com/LedgerHQ/ledgerjs/issues/43#issuecomment-366984725
                // https://github.com/LedgerHQ/ledgerjs/blob/master/packages/web3-subprovider/src/index.js#L143

                byte[] vrs = hexChainId.ToHexBytes() // v
                                       .Concat("0".ToHexBytes()) // r
                                       .Concat("0".ToHexBytes()) // s
                                       .ToArray();

                // Not sure if v, r, s should be included in the regular data chunk or separate, or none at all.

                memoryStream.WriteByte((byte)combinedByteData.Length); // Do we need to first write the byte length?
                memoryStream.Write(combinedByteData, 0, combinedByteData.Length);

                memoryStream.WriteByte((byte)vrs.Length);
                memoryStream.Write(vrs, 0, vrs.Length);

                transactionData = memoryStream.ToArray();
            }

            return transactionData;
        }

        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return bytes.Aggregate(string.Empty, (current, theByte) => current + theByte.ToString("X2"));
        }

        public static byte[] ToHexBytes(this string ethString)
        {
            var numberOfCharacters = ethString.Length / 2;
            var returnValue = new byte[numberOfCharacters];

            for (var i = 0; i < numberOfCharacters; i++)
            {
                var x = i * 2;
                var firstHexCharacter = ethString[x];
                var secondHexCharacter = ethString[x + 1];

                var hexStringBuilder = new StringBuilder();
                hexStringBuilder.Append(firstHexCharacter);
                hexStringBuilder.Append(secondHexCharacter);

                var hexString = hexStringBuilder.ToString();

                returnValue[i] = byte.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            }

            return returnValue;
        }

        public static string ToHex(this long number)
        {
            return number.ToString(Format);
        }

        public static string ToHex(this int number)
        {
            return number.ToString(Format);
        }

        public static byte[] ToHexBytes(this int number)
        {
            return Encoding.GetBytes(number.ToHex());
        }

        public static byte[] ToHexBytes(this long number)
        {
            return Encoding.GetBytes(number.ToHex());
        }

        public static byte[] ToEthBytes(this long number)
        {
            return Encoding.GetBytes($"0x{ToHexBytes(number)}");
        }

        public static byte[] ToEthBytes(this int number)
        {
            return Encoding.GetBytes($"0x{ToHexBytes(number)}");
        }
    }
}
