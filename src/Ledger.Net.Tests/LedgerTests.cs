using Hid.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ledger.Net.Tests
{
    public class LedgerTests
    {
        public static VendorProductIds[] WellKnownLedgerWallets = new VendorProductIds[]
        {
            new VendorProductIds(0x2c97),
            new VendorProductIds(0x2581, 0x3b7c)
        };
        private static readonly UsageSpecification[] _UsageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };

        [Fact]
        public async Task GetAddressAnyBitcoinApp()
        {
            var ledgerManager = await GetLedger();

            await ledgerManager.SetCoinNumber();

            var address = await ledgerManager.GetAddressAsync(0, false, 0, false);
        }

        [Fact]
        public async Task GetAddress()
        {
            var ledgerManager = await GetLedger();

            var address = await ledgerManager.GetAddressAsync(0, false, 0, false);
            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }


        [Fact]
        public async Task GetBitcoinPublicKey()
        {
            var ledgerManager = await GetLedger();
            var addressPath = Helpers.GetDerivationPathData(ledgerManager.CurrentCoin.App, ledgerManager.CurrentCoin.CoinNumber, 0, 0, false, ledgerManager.CurrentCoin.IsSegwit);
            var publicKey = await ledgerManager.SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest (true, BitcoinAddressType.Legacy, addressPath));
            Assert.True(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [Fact]
        public async Task GetEthereumPublicKey()
        {
            var ledgerManager = await GetLedger();
            ledgerManager.SetCoinNumber(60);
            var addressPath = Helpers.GetDerivationPathData(ledgerManager.CurrentCoin.App, ledgerManager.CurrentCoin.CoinNumber, 0, 0, false, ledgerManager.CurrentCoin.IsSegwit);
            var publicKey = await ledgerManager.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(true, false, addressPath));
            Assert.True(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [Fact]
        public async Task SignEthereumTransaction()
        {
            var ledgerManager = await GetLedger();
            ledgerManager.SetCoinNumber(60);

            byte[] rlpEncodedTransactionData = { 227, 128, 132, 59, 154, 202, 0, 130, 82, 8, 148, 139, 6, 158, 207, 123, 242, 48, 225, 83, 184, 237, 144, 59, 171, 242, 68, 3, 204, 162, 3, 128, 128, 4, 128, 128 };

            var derivationData = Helpers.GetDerivationPathData(ledgerManager.CurrentCoin.App, ledgerManager.CurrentCoin.CoinNumber, 0, 0, false, ledgerManager.CurrentCoin.IsSegwit);

            // Create base class like GetPublicKeyResponseBase and make the method more like GetAddressAsync
            var firstRequest = new EthereumAppSignTransactionRequest(derivationData.Concat(rlpEncodedTransactionData).ToArray());

            var response = await ledgerManager.SendRequestAsync<EthereumAppSignTransactionResponse, EthereumAppSignTransactionRequest>(firstRequest);

            Assert.True(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.True(response.SignatureR?.Length == 32);
            Assert.True(response.SignatureS?.Length == 32);
        }

        [Fact]
        public async Task GetEthereumAddress()
        {
            var ledgerManager = await GetLedger();

            ledgerManager.SetCoinNumber(60);
            var address = await ledgerManager.GetAddressAsync(0, 0);

            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }

        private static async Task<LedgerManager> GetLedger()
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in WellKnownLedgerWallets)
            {
                if (ids.ProductId == null)
                {
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
                }
                else
                {
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
                }
            }

            var retVal = devices
                .FirstOrDefault(d =>
                _UsageSpecification == null ||
                _UsageSpecification.Length == 0 ||
                _UsageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            var ledgerHidDevice = new WindowsHidDevice(retVal);
            await ledgerHidDevice.InitializeAsync();
            var ledgerManager = new LedgerManager(ledgerHidDevice);
            return ledgerManager;
        }
    }

    public class VendorProductIds
    {
        public VendorProductIds(int vendorId)
        {
            VendorId = vendorId;
        }
        public VendorProductIds(int vendorId, int? productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }
        public int VendorId
        {
            get;
        }
        public int? ProductId
        {
            get;
        }
    }

    public class UsageSpecification
    {
        public UsageSpecification(ushort usagePage, ushort usage)
        {
            UsagePage = usagePage;
            Usage = usage;
        }

        public ushort Usage
        {
            get;
        }
        public ushort UsagePage
        {
            get;
        }
    }
}
