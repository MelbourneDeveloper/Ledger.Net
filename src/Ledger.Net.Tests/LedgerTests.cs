using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using Hardwarewallets.Net.Model;
using Hid.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using NBitcoin;
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
        #region Private Fields
        private LedgerManager _LedgerManager;

        /// <summary>
        /// This func is not necessary, but it is an example of how to make a call so that the user can be prompted with UI prompts based on the current state of the Ledger device
        /// </summary>
        private readonly Func<CallAndPromptArgs<GetAddressArgs>, Task<GetPublicKeyResponseBase>> _GetPublicKeyFunc = new Func<CallAndPromptArgs<GetAddressArgs>, Task<GetPublicKeyResponseBase>>(async (s) =>
        {
            var lm = s.LedgerManager;

            var data = Helpers.GetDerivationPathData(s.Args.AddressPath);

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

        public static VendorProductIds[] WellKnownLedgerWallets = new VendorProductIds[] { new VendorProductIds(0x2c97), new VendorProductIds(0x2581, 0x3b7c) };
        private static readonly UsageSpecification[] _UsageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };
        #endregion

        #region Tests

        [Fact]
        public async Task GetLiteCoinAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(2);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            Assert.True(!string.IsNullOrEmpty(address));
        }

        /// <summary>
        /// Note this hasn't been confirmed to be returning the correct address in Ledger live yet
        /// </summary>
        [Fact]
        public async Task GetBitcoinCashAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(145);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            Assert.True(!string.IsNullOrEmpty(address));
        }

        [Fact]
        public async Task GetAddressAnyBitcoinApp()
        {
            await GetLedger();

            await _LedgerManager.SetCoinNumber();

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, false);
        }

        [Fact]
        public async Task DisplayAddress()
        {
            await GetLedger(Prompt);

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, true);

            Assert.True(!string.IsNullOrEmpty(address));
        }


        [Fact]
        public async Task GetBitcoinPublicKey()
        {
            await GetLedger(Prompt);

            var returnResponse = (GetPublicKeyResponseBase)await _LedgerManager.CallAndPrompt(_GetPublicKeyFunc,
            new CallAndPromptArgs<GetAddressArgs>
            {
                LedgerManager = _LedgerManager,
                MemberName = nameof(_GetPublicKeyFunc),
                Args = new GetAddressArgs(new AddressPath(true, 0, 0, false, 0), false)
            });

            Assert.True(!string.IsNullOrEmpty(returnResponse.PublicKey));
        }

        [Fact]
        public async Task GetEthereumPublicKey()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);
            var addressPath = Helpers.GetDerivationPathData(new AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));
            var publicKey = await _LedgerManager.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(true, false, addressPath));
            Assert.True(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [Fact]
        public async Task SignEthereumTransaction()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);

            byte[] rlpEncodedTransactionData = { 227, 128, 132, 59, 154, 202, 0, 130, 82, 8, 148, 139, 6, 158, 207, 123, 242, 48, 225, 83, 184, 237, 144, 59, 171, 242, 68, 3, 204, 162, 3, 128, 128, 4, 128, 128 };

            var derivationData = Helpers.GetDerivationPathData(new AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));

            // Create base class like GetPublicKeyResponseBase and make the method more like GetAddressAsync

            var firstRequest = new EthereumAppSignatureRequest(true, derivationData.Concat(rlpEncodedTransactionData).ToArray());

            var response = await _LedgerManager.SendRequestAsync<EthereumAppSignatureResponse, EthereumAppSignatureRequest>(firstRequest);

            Assert.True(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.True(response.SignatureR?.Length == 32);
            Assert.True(response.SignatureS?.Length == 32);
        }

        /// <summary>
        /// This unit test hangs
        /// https://github.com/MelbourneDeveloper/Ledger.Net/issues/13
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SignChunkedEthereumTransaction()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);

            byte[] rlpEncodedTransactionData = {
                0xf9, 0x01, 0x27, 0x11, 0x84, 0x3b, 0x9a, 0xca,
                0x00, 0x83, 0x2d, 0xc6, 0xc0, 0x94, 0xb5, 0x0c,
                0xda, 0x8b, 0xf7, 0xf4, 0x09, 0xac, 0xae, 0x4d,
                0x46, 0xd4, 0x07, 0x3b, 0xab, 0xcc, 0x91, 0xa4,
                0xd2, 0xcc, 0x80, 0xb9, 0x01, 0x04, 0xf2, 0xf3,
                0x57, 0x85, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0e, 0x30, 0x31, 0x30, 0x32, 0x30, 0x33,
                0x30, 0x34, 0x30, 0x35, 0x30, 0x36, 0x30, 0x43,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00};

            var derivationData = Helpers.GetDerivationPathData(new AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));

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

            Assert.True(!string.IsNullOrEmpty(address));
        }

        [Fact]
        public async Task GetEthereumAddressParsed()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            //Modern Path
            var path = KeyPath.Parse("m/44'/60'/0'/0").Derive(0);
            var addressPath = new KeyPathAddressPath(path);
            var address = await _LedgerManager.GetAddressAsync(addressPath, false, false);
            Assert.True(!string.IsNullOrEmpty(address));

            //Legacy Path
            path = KeyPath.Parse("m/44'/60'/0'").Derive(0);
            addressPath = new KeyPathAddressPath(path);
            address = await _LedgerManager.GetAddressAsync(addressPath, false, false);
            Assert.True(!string.IsNullOrEmpty(address));
        }

        [Fact]
        public async Task GetEthereumAddressCustomPath()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            //Three elements
            var path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0 };
            var address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.True(!string.IsNullOrEmpty(address));

            //Four elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0, 1 };
            address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.True(!string.IsNullOrEmpty(address));

            //Two elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60) };
            address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.True(!string.IsNullOrEmpty(address));

            _LedgerManager.ErrorPrompt = ThrowErrorInsteadOfPrompt;

            Exception exception = null; ;
            try
            {
                //The ethereum app doesn't like this Purpose (49)
                path = new uint[] { AddressUtilities.HardenNumber(49), AddressUtilities.HardenNumber(60), AddressUtilities.HardenNumber(0), 0, 0 };
                address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.True(exception != null);
        }

        [Fact]
        public async Task GetEthereumAddresses()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            var addressManager = new AddressManager(_LedgerManager, new AddressPathFactory(false, 60));

            //Get 10 addresses with all the trimming
            const int numberOfAddresses = 3;
            const int numberOfAccounts = 2;
            var addresses = await addressManager.GetAddressesAsync(0, numberOfAddresses, numberOfAccounts, true, true);

            Assert.True(addresses != null);
            Assert.True(addresses.Accounts != null);
            Assert.True(addresses.Accounts.Count == numberOfAccounts);
            Assert.True(addresses.Accounts[0].Addresses.Count == numberOfAddresses);
            Assert.True(addresses.Accounts[1].Addresses.Count == numberOfAddresses);
            Assert.True(addresses.Accounts[0].ChangeAddresses.Count == numberOfAddresses);
            Assert.True(addresses.Accounts[1].ChangeAddresses.Count == numberOfAddresses);
        }

        #endregion

        #region Other 

        private async Task ThrowErrorInsteadOfPrompt(int? returnCode, Exception exception, string member)
        {
            throw new Exception("Ouch!");
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
        #endregion
    }

    public class CustomAddressPath : IAddressPath
    {
        public uint Purpose => throw new NotImplementedException();

        public uint CoinType => throw new NotImplementedException();

        public uint Account => throw new NotImplementedException();

        public uint Change => throw new NotImplementedException();

        public uint AddressIndex => throw new NotImplementedException();

        public uint[] Path { get; }

        public CustomAddressPath(uint[] path)
        {
            Path = path;
        }

        public uint[] ToHardenedArray()
        {
            return Path;
        }

        public uint[] ToUnhardenedArray()
        {
            throw new NotImplementedException();
        }
    }
}
