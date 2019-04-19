using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using Ledger.Net.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    [TestClass]
    public partial class LedgerTests
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

        [TestMethod]
        public async Task DisplayTronAddress()
        {
            await GetLedger();

            uint coinNumber = 195;
            var isSegwit = false;
            var isChange = false;
            var index = 0;

            _LedgerManager.SetCoinNumber(coinNumber);

            //This address seems to match the default address in the Tron app
            var path = $"m/{(isSegwit ? 49 : 44)}'/{coinNumber}'/{(isChange ? 1 : 0)}'/{0}/{index}";
            var addressPath = AddressPathBase.Parse<BIP44AddressPath>(path);
            var address = await _LedgerManager.GetAddressAsync(addressPath, false, true);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task GetLiteCoinAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(2);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        /// <summary>
        /// Note this hasn't been confirmed to be returning the correct address in Ledger live yet
        /// </summary>
        [TestMethod]
        public async Task GetBitcoinCashAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(145);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task GetAddressAnyBitcoinApp()
        {
            await GetLedger();

            await _LedgerManager.SetCoinNumber();

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, false);
        }

        [TestMethod]
        public async Task DisplayAddress()
        {
            await GetLedger();

            var address = await _LedgerManager.GetAddressAsync(0, false, 0, true);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }


        [TestMethod]
        public async Task GetBitcoinPublicKey()
        {
            await GetLedger();

            var returnResponse = (GetPublicKeyResponseBase)await _LedgerManager.CallAndPrompt(_GetPublicKeyFunc,
            new CallAndPromptArgs<GetAddressArgs>
            {
                LedgerManager = _LedgerManager,
                MemberName = nameof(_GetPublicKeyFunc),
                Args = new GetAddressArgs(new BIP44AddressPath(true, 0, 0, false, 0), false)
            });

            Assert.IsTrue(!string.IsNullOrEmpty(returnResponse.PublicKey));
        }

        [TestMethod]
        public async Task DisplayEthereumPublicKey()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);
            var addressPath = Helpers.GetDerivationPathData(new BIP44AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));
            var publicKey = await _LedgerManager.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(true, false, addressPath));
            Assert.IsTrue(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [TestMethod]
        public async Task SignEthereumTransaction()
        {
            await GetLedger();
            _LedgerManager.SetCoinNumber(60);

            byte[] rlpEncodedTransactionData = { 227, 128, 132, 59, 154, 202, 0, 130, 82, 8, 148, 139, 6, 158, 207, 123, 242, 48, 225, 83, 184, 237, 144, 59, 171, 242, 68, 3, 204, 162, 3, 128, 128, 4, 128, 128 };

            var derivationData = Helpers.GetDerivationPathData(new BIP44AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));

            // Create base class like GetPublicKeyResponseBase and make the method more like GetAddressAsync

            var firstRequest = new EthereumAppSignatureRequest(true, derivationData.Concat(rlpEncodedTransactionData).ToArray());

            var response = await _LedgerManager.SendRequestAsync<EthereumAppSignatureResponse, EthereumAppSignatureRequest>(firstRequest);

            Assert.IsTrue(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.IsTrue(response.SignatureR?.Length == 32);
            Assert.IsTrue(response.SignatureS?.Length == 32);
        }

        [TestMethod]
        public async Task TestSignTronTransaction1()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L33
            var transactionRaw1 = "0a027d52220889fd90c45b71f24740e0bcb0f2be2c5a67080112630a2d747970" +
            "652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e7366" +
            "6572436f6e747261637412320a1541c8599111f29c1e1e061265b4af93ea1f27" +
            "4ad78a1215414f560eb4182ca53757f905609e226e96e8e1a80c18c0843d70d0" +
            "f5acf2be2c";

            await SignTronTransaction(transactionRaw1, "44'/195'/0'/0/0", true);
        }

        [TestMethod]
        public async Task TestSignTronTransaction2()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L45
            var transactionRaw2 = "0a02c56522086cd623dbe83075d8409089e88dbf2c5a67080112630a2d747970" +
                     "652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e7366" +
                     "6572436f6e747261637412320a1541c8599111f29c1e1e061265b4af93ea1f27" +
                     "4ad78a1215414f560eb4182ca53757f905609e226e96e8e1a80c1880897a70f3" +
                     "c3e48dbf2c";

            await SignTronTransaction(transactionRaw2, "44'/195'/0'/0/0", true);
        }

        [TestMethod]
        public async Task TestSignTronTransaction3()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L56
            var transactionRaw3 = "0a02e7c3220869e2abb19969f1e740f0bbd3fabf2c5a7c080212780a32747970" +
                     "652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e7366" +
                     "65724173736574436f6e747261637412420a1043727970746f436861696e546f" +
                     "6b656e121541c8599111f29c1e1e061265b4af93ea1f274ad78a1a15414f560e" +
                     "b4182ca53757f905609e226e96e8e1a80c200170b7f5cffabf2c";

            await SignTronTransaction(transactionRaw3, "44'/195'/0'/0/0", true);
        }

        /// <summary>
        /// I don't really know what this does. I think it locks your Tron wallet...
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestFreezeWARNING()
        {
            var transactionRaw = "0a025505220885d2f613d363093540c8c6cba49f2d5a53080c124f0a34747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e556e667265657a6542616c616e6365436f6e747261637412170a1541bfdc501d1ccc4a7167489c8e670e4954a44c914570ac8bc8a49f2d";

            await SignTronTransaction(transactionRaw, "44'/195'/0'/0/0", false);
        }

        /// <summary>
        /// I don't really know what this does. I think it locks your Tron wallet...
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestFreezeWARNING2()
        {
            var transactionRaw = "0a0254e32208d8055e5cf71ba13b40d8a9c5a49f2d5a730802126f0a32747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e736665724173736574436f6e747261637412390a0731303030323236121541bfdc501d1ccc4a7167489c8e670e4954a44c91451a1541035b5ce89b1bec9d82b4cf2e277075b36f77682d2001709be4c1a49f2d";

            await SignTronTransaction(transactionRaw, "44'/195'/0'/0/0", false);
        }
       
        private static TronTransactionModel GetTronTransactionModelFromResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string json;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            var model = JsonConvert.DeserializeObject<TronTransactionModel>(json);
            return model;
        }

        /// <summary>
        /// This unit test hangs
        /// https://github.com/MelbourneDeveloper/Ledger.Net/issues/13
        /// </summary>
        /// <returns></returns>
        [TestMethod]
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

            var derivationData = Helpers.GetDerivationPathData(new BIP44AddressPath(_LedgerManager.CurrentCoin.IsSegwit, _LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));

            // Create base class like GetPublicKeyResponseBase and make the method more like GetAddressAsync

            var firstRequest = new EthereumAppSignatureRequest(true, derivationData.Concat(rlpEncodedTransactionData).ToArray());

            var response = await _LedgerManager.SendRequestAsync<EthereumAppSignatureResponse, EthereumAppSignatureRequest>(firstRequest);

            Assert.IsTrue(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.IsTrue(response.SignatureR?.Length == 32);
            Assert.IsTrue(response.SignatureS?.Length == 32);
        }

        [TestMethod]
        public async Task GetEthereumAddress()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);
            var address = await _LedgerManager.GetAddressAsync(0, 0);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task GetEthereumAddressParsed()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            //Modern Path
            var path = AddressPathBase.Parse<CustomAddressPath>("m/44'/60'/0'/0/0");
            var address = await _LedgerManager.GetAddressAsync(path, false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Legacy Path
            path = AddressPathBase.Parse<CustomAddressPath>("m/44'/60'/0'/0");
            address = await _LedgerManager.GetAddressAsync(path, false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task GetEthereumAddressCustomPath()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            //Three elements
            var path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0 };
            var address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Four elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0, 1 };
            address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Two elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60) };
            address = await _LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

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
            Assert.IsTrue(exception != null);
        }

        [TestMethod]
        public async Task GetEthereumAddresses()
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(60);

            var addressManager = new AddressManager(_LedgerManager, new BIP44AddressPathFactory(false, 60));

            //Get 10 addresses with all the trimming
            const int numberOfAddresses = 3;
            const int numberOfAccounts = 2;
            var addresses = await addressManager.GetAddressesAsync(0, numberOfAddresses, numberOfAccounts, true, true);

            Assert.IsTrue(addresses != null);
            Assert.IsTrue(addresses.Accounts != null);
            Assert.IsTrue(addresses.Accounts.Count == numberOfAccounts);
            Assert.IsTrue(addresses.Accounts[0].Addresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[1].Addresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[0].ChangeAddresses.Count == numberOfAddresses);
            Assert.IsTrue(addresses.Accounts[1].ChangeAddresses.Count == numberOfAddresses);
        }

        #endregion

        #region Other 

        private async Task SignTronTransaction(string transactionRaw, string path, bool expectsResult)
        {
            await GetLedger();

            _LedgerManager.SetCoinNumber(195);

            var transactionData = new List<byte>();

            for (var i = 0; i < transactionRaw.Length; i += 2)
            {
                var byteInHex = transactionRaw.Substring(i, 2);
                transactionData.Add(Convert.ToByte(byteInHex, 16));
            }

            var derivationData = Helpers.GetDerivationPathData(AddressPathBase.Parse<BIP44AddressPath>(path));

            var firstRequest = new TronAppSignatureRequest(derivationData.Concat(transactionData).ToArray());

            var response = await _LedgerManager.SendRequestAsync<TronAppSignatureResponse, TronAppSignatureRequest>(firstRequest);

            var hexData = new StringBuilder();
            for (var i = 0; i < response.Data.Length; i++)
            {
                hexData.Append(response.Data[i].ToString("X"));
            }
            Console.WriteLine(hexData);

            Assert.IsTrue(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.IsTrue(!expectsResult ||  response.Data?.Length > 2, "No data was returned");           
        }

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

        private async Task GetLedgerBase(ErrorPromptDelegate errorPrompt = null)
        {
            var ledgerManagerBroker = new LedgerManagerBroker(3000, null, Prompt);
            _LedgerManager = await ledgerManagerBroker.WaitForFirstDeviceAsync();
        }

        #endregion
    }
}
