using Hid.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ledger.Net.Tests
{
    public class LedgerTests
    {
        private static LedgerManager _LedgerManager;

        private readonly Func<CallAndPromptArgs<GetAddressArgs>, Task<GetPublicKeyResponseBase>> _GetPublicKeyFunc = new Func<CallAndPromptArgs<GetAddressArgs>, Task<GetPublicKeyResponseBase>>(async (s) =>
        {
            var lm = s.LedgerManager;

            var data = Helpers.GetDerivationPathData(lm.CurrentCoin.App, lm.CurrentCoin.CoinNumber, s.Args.Account, s.Args.Index, s.Args.IsChange, s.LedgerManager.CurrentCoin.IsSegwit);

            GetPublicKeyResponseBase response;

            switch (lm.CurrentCoin.App)
            {
                case App.Ethereum:
                    response = await lm.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(s.Args.ShowDisplay, false, data));
                    break;
                case App.Bitcoin:
                    //TODO: Should we use the Coin's IsSegwit here?
                    response = await lm.SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(s.Args.ShowDisplay, BitcoinAddressType.Segwit, data));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return response;
        });

        public static VendorProductIds[] WellKnownLedgerWallets = new VendorProductIds[]
        {
            new VendorProductIds(0x2c97),
            new VendorProductIds(0x2581, 0x3b7c)
        };
        private static readonly UsageSpecification[] _UsageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };

        [Fact]
        public async Task GetAddressAnyBitcoinApp()
        {
            await GetLedger();

            await _LedgerManager.SetCoinNumber();

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, false);
        }

        private async Task Prompt(int? returnCode, Exception exception, string member)
        {
            if (returnCode.HasValue)
            {
                switch (returnCode.Value)
                {
                    case Constants.IncorrectLengthStatusCode:
                        Debug.WriteLine($"Please ensure the app { _LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    case Constants.SecurityNotValidStatusCode:
                        Debug.WriteLine($"It appears that your Ledger pin has not been entered, or no app is open. Please ensure the app  {_LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    case Constants.InstructionNotSupportedStatusCode:
                        Debug.WriteLine($"The current app is incorrect. Please ensure the app for {_LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    default:
                        Debug.WriteLine($"Something went wrong. Please ensure the app  {_LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                }
            }
            else
            {
                if (exception is IOException)
                {
                    await Task.Delay(3000);
                    await _LedgerManager.LedgerHidDevice.InitializeAsync();
                }
            }

            await Task.Delay(5000);
        }

        [Fact]
        public async Task GetAddress()
        {
            await GetLedger(Prompt);

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, false);

            Assert.True(!string.IsNullOrEmpty(address));
        }


        [Fact]
        public async Task DisplayBitcoinPublicKey()
        {
            await GetLedger(Prompt);

            var returnResponse = (GetPublicKeyResponseBase)await _LedgerManager.CallAndPrompt(_GetPublicKeyFunc,
            new CallAndPromptArgs<GetAddressArgs>
            {
                LedgerManager = _LedgerManager,
                MemberName = nameof(_GetPublicKeyFunc),
                Args = new GetAddressArgs(0, 0, false, true)
            });

            Assert.True(!string.IsNullOrEmpty(returnResponse.PublicKey));
        }

        [Fact]
        public async Task GetEthereumPublicKey()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);
            var addressPath = Helpers.GetDerivationPathData(_LedgerManager.CurrentCoin.App, _LedgerManager.CurrentCoin.CoinNumber, 0, 0, false, _LedgerManager.CurrentCoin.IsSegwit);
            var publicKey = await _LedgerManager.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(true, false, addressPath));
            Assert.True(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [Fact]
        public async Task SignEthereumTransaction()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);

            byte[] rlpEncodedTransactionData = { 227, 128, 132, 59, 154, 202, 0, 130, 82, 8, 148, 139, 6, 158, 207, 123, 242, 48, 225, 83, 184, 237, 144, 59, 171, 242, 68, 3, 204, 162, 3, 128, 128, 4, 128, 128 };

            var derivationData = Helpers.GetDerivationPathData(_LedgerManager.CurrentCoin.App, _LedgerManager.CurrentCoin.CoinNumber, 0, 0, false, _LedgerManager.CurrentCoin.IsSegwit);

            // Create base class like GetPublicKeyResponseBase and make the method more like GetAddressAsync

            var firstRequest = new EthereumAppSignatureRequest(true, derivationData.Concat(rlpEncodedTransactionData).ToArray());

            var response = await _LedgerManager.SendRequestAsync<EthereumAppSignatureResponse, EthereumAppSignatureRequest>(firstRequest);

            Assert.True(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.True(response.SignatureR?.Length == 32);
            Assert.True(response.SignatureS?.Length == 32);
        }

        [Fact]
        public async Task GetEthereumAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }

        private async Task GetLedger(ErrorPromptDelegate errorPrompt = null)
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
            _LedgerManager = new LedgerManager(ledgerHidDevice, null, Prompt);
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
