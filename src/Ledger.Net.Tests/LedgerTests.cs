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
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public abstract class LedgerTests
    {
        #region Private Fields
        protected LedgerManagerBroker _LedgerManagerBroker;
        private LedgerManager LedgerManager
        {
            get
            {
                var i = 0;

                while (_LedgerManagerBroker.LedgerManagers.Count == 0)
                {
                    Thread.Sleep(100);
                    i++;
                    if (i > 200) throw new Exception("Waited too long");
                }

                return (LedgerManager)_LedgerManagerBroker.LedgerManagers.First();
            }
        }

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
                    //TODO: don't use the RequestHandler directly.
                    response = await lm.RequestHandler.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(s.Args.ShowDisplay, false, data));
                    break;
                case App.Bitcoin:
                    //TODO: Should we use the Coin's IsSegwit here?
                    //TODO: don't use the RequestHandler directly.
                    response = await lm.RequestHandler.SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(s.Args.ShowDisplay, BitcoinAddressType.Segwit, data));
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
        public async Task TestTronDisplayAddress()
        {
            uint coinNumber = 195;
            var isSegwit = false;
            var isChange = false;
            var index = 0;

            LedgerManager.SetCoinNumber(coinNumber);

            //This address seems to match the default address in the Tron app
            var path = $"m/{(isSegwit ? 49 : 44)}'/{coinNumber}'/{(isChange ? 1 : 0)}'/{0}/{index}";
            var addressPath = AddressPathBase.Parse<BIP44AddressPath>(path);
            var address = await LedgerManager.GetAddressAsync(addressPath, false, true);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task TestLiteCoinGetAddress()
        {
            await GetAddress(2);
        }

        /// <summary>
        /// Note this hasn't been confirmed to be returning the correct address in Ledger live yet
        /// </summary>
        [TestMethod]
        public async Task TestBitcoinCashGetAddress()
        {
            await GetAddress(145);
        }

        [TestMethod]
        public async Task TestBitcoinGetAddressAnyApp()
        {

            await LedgerManager.SetCoinNumber();

            var address = await LedgerManager.GetAddressAsync(0, false, 0, false);
        }

        [TestMethod]
        public async Task TestBitcoinDisplayAddress()
        {

            var address = await LedgerManager.GetAddressAsync(0, false, 0, true);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }


        [TestMethod]
        public async Task TestBitcoinGetPublicKey()
        {

            var returnResponse = (GetPublicKeyResponseBase)await LedgerManager.CallAndPrompt(_GetPublicKeyFunc,
            new CallAndPromptArgs<GetAddressArgs>
            {
                LedgerManager = LedgerManager,
                MemberName = nameof(_GetPublicKeyFunc),
                Args = new GetAddressArgs(new BIP44AddressPath(true, 0, 0, false, 0), false)
            });

            Assert.IsTrue(!string.IsNullOrEmpty(returnResponse.PublicKey));
        }

        [TestMethod]
        public async Task TestEthereumDisplayPublicKey()
        {
            LedgerManager.SetCoinNumber(60);
            var addressPath = Helpers.GetDerivationPathData(new BIP44AddressPath(LedgerManager.CurrentCoin.IsSegwit, LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));
            //TODO: don't use the RequestHandler directly.
            var publicKey = await LedgerManager.RequestHandler.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(true, false, addressPath));
            Assert.IsTrue(!string.IsNullOrEmpty(publicKey.PublicKey));
        }

        [TestMethod]
        public async Task TestEthereumSignTransaction()
        {
            byte[] rlpEncodedTransactionData = { 227, 128, 132, 59, 154, 202, 0, 130, 82, 8, 148, 139, 6, 158, 207, 123, 242, 48, 225, 83, 184, 237, 144, 59, 171, 242, 68, 3, 204, 162, 3, 128, 128, 4, 128, 128 };

            await SignEtheremAsync(rlpEncodedTransactionData, true);
        }

        /// <summary>
        /// This unit test fails
        /// https://github.com/MelbourneDeveloper/Ledger.Net/issues/13
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestEthereumSignTransactionChunked()
        {
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

            await SignEtheremAsync(rlpEncodedTransactionData, false);
        }

        private async Task SignEtheremAsync(byte[] rlpEncodedTransactionData, bool isTransaction)
        {
            LedgerManager.SetCoinNumber(60);

            var derivationData = Helpers.GetDerivationPathData(new BIP44AddressPath(LedgerManager.CurrentCoin.IsSegwit, LedgerManager.CurrentCoin.CoinNumber, 0, false, 0));

            var concatenatedData = derivationData.Concat(rlpEncodedTransactionData).ToArray();

            Console.WriteLine($"Input data: {HexDataToString(concatenatedData)}");

            var firstRequest = new EthereumAppSignatureRequest(isTransaction, concatenatedData);

            //TODO: don't use the RequestHandler directly.
            var response = await LedgerManager.RequestHandler.SendRequestAsync<EthereumAppSignatureResponse, EthereumAppSignatureRequest>(firstRequest);

            Assert.IsTrue(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.IsTrue(response.SignatureR?.Length == 32);
            Assert.IsTrue(response.SignatureS?.Length == 32);

            Console.WriteLine($"R:{HexDataToString(response.SignatureR)}\r\nS:{HexDataToString(response.SignatureS)}\r\nV:{response.SignatureV}");
        }

        [TestMethod]
        public async Task TestTronSignTransactionBig()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/aaf10b341c47cc8cb22ba22ba189bc91fa0355aa/examples/signTransactionExtension.py#L18

            var transactionRaw1 = "0a02162e220897e339bb4351670240e8cdc9bca62d5ac602080412c1020a30747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e566f74655769746e657373436f6e7472616374128c020a1541bfdc501d1ccc4a7167489c8e670e4954a44c9145121b0a154184399fc6a98edc11a6efb146e86a3e153d0a093310a08d0612190a15414d1ef8673f916debb7e2515a8f3ecaf2611034aa100112190a1541496e85711fa3b7ba5a093af635269a67230ac2c1100112190a1541b3eec71481e8864f0fc1f601b836b74c40548287100112190a1541f29f57614a6b201729473c837e1d2879e9f90b8e100112190a1541d25855804e4e65de904faf3ac74b0bdfc53fac76100112190a15418a445facc2aa94d72292ebbcb2a611e9fd8a6c6e100112190a1541c189fa6fc9ed7a3580c3fe291915d5c6a6259be7100112190a1541bac7378c4265ad2739772337682183b8864f517a100170ab95c6bca62d";
            await SignTronTransaction(transactionRaw1, "44'/195'/0'/0/0");
        }

        [TestMethod]
        public async Task TestTronSignTransactionBig2()
        {
            var transactionRaw1 = "0a022b652208745915a7ae8ae98d409097b1c4a62d5a66082c12620a38747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e45786368616e67655472616e73616374696f6e436f6e747261637412260a1541bfdc501d1ccc4a7167489c8e670e4954a44c914510191a0731303030323236200a2801709edaadc4a62d08191207313030303232361a1454524f4e4575726f7065526577617264436f696e20002a015f320354525838064247304502210092bcbaeb668c8cc5a89db2d5ce901946a4d7f9fab02b0e119d44cf818c36b4e802201ac8702d5412177d622c82737c46e25f9fdaae35f4d3c4ea807ef30b8ae00bf2";
            await SignTronTransaction(transactionRaw1, "44'/195'/0'/0/0");
        }

        [TestMethod]
        public async Task TestTronSignTransaction1()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L33
            //var transactionRaw1 = "0a02edd92208ac1579fd507b8c8b40b0a2d3ada62d5af301080412ee010a30747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e566f74655769746e657373436f6e747261637412b9010a15417773ff0ebd2d2c85761db01ae2b00c417bf1539312190a154167e39013be3cdd38";

            var transactionRaw1 = "0a02ee332208c90c7f40dfbdc51540e0dfe3ada62d5a860108041281010a30747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e566f74655769746e657373436f6e7472616374124d0a15417773ff0ebd2d2c85761db01ae2b00c417bf1539312190a154167e39013be3cdd3814bed152d7439fb5b6791409100212190a1541c189fa6fc9ed7a3580c3fe291915d5c6a6259be710027089a2e0ada62d";

            await SignTronTransaction(transactionRaw1, "44'/195'/0'/0/0");
        }

        [TestMethod]
        public async Task TestTronSignTransaction2()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L45
            var transactionRaw2 = "0a02c56522086cd623dbe83075d8409089e88dbf2c5a67080112630a2d747970" +
                     "652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e7366" +
                     "6572436f6e747261637412320a1541c8599111f29c1e1e061265b4af93ea1f27" +
                     "4ad78a1215414f560eb4182ca53757f905609e226e96e8e1a80c1880897a70f3" +
                     "c3e48dbf2c";

            await SignTronTransaction(transactionRaw2, "44'/195'/0'/0/0");
        }

        [TestMethod]
        public async Task TestTronSignTransaction3()
        {
            //Data from python sample
            //https://github.com/fbsobreira/trx-ledger/blob/b274fcdc19b09c20485fefa534aeba878ae525b6/test_signTransaction.py#L56
            var transactionRaw3 = "0a02e7c3220869e2abb19969f1e740f0bbd3fabf2c5a7c080212780a32747970" +
                     "652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e5472616e7366" +
                     "65724173736574436f6e747261637412420a1043727970746f436861696e546f" +
                     "6b656e121541c8599111f29c1e1e061265b4af93ea1f274ad78a1a15414f560e" +
                     "b4182ca53757f905609e226e96e8e1a80c200170b7f5cffabf2c";

            await SignTronTransaction(transactionRaw3, "44'/195'/0'/0/0");
        }

        [TestMethod]
        public async Task TestTronFreezeBalanceContract()
        {
            ///Freeze Balance
            ///Freezes an amount of TRX. Will give bandwidth OR Energy and TRON Power (voting rights) to the owner of the frozen tokens. 
            ///Optionally, can freeze TRX to grant Energy or Bandwidth to other users. Balance amount in the denomination of Sun.
            ///https://developers.tron.network/reference#walletfreezebalance-1
            ///Data from https://github.com/CTJaeger
            var transactionRaw3 = "0a02b76d2208ca2fdf1dc2eda61040f8a4b9a0a32d5a58080b12540a32747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e467265657a6542616c616e6365436f6e7472616374121e0a1541bfdc501d1ccc4a7167489c8e670e4954a44c914510c096b102180370dfdeb5a0a32d";

            await SignTronTransaction(transactionRaw3, "44'/195'/0'/0/0", 134);
        }

        [TestMethod]
        public async Task TestTronUnFreezeBalanceContract()
        {
            ///Unfreeze Balance
            ///Unfreeze TRX that has passed the minimum freeze duration. Unfreezing will remove bandwidth and TRON Power. Returns unfrozen TRX transaction.
            ///https://developers.tron.network/reference#walletunfreezebalance-1
            ///Data from https://github.com/CTJaeger
            var transactionRaw4 = "0a02b7782208d3016ebea5e1611740e0a6bba0a32d5a53080c124f0a34747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e556e667265657a6542616c616e6365436f6e747261637412170a1541bfdc501d1ccc4a7167489c8e670e4954a44c914570b4e4b7a0a32d";

            await SignTronTransaction(transactionRaw4, "44'/195'/0'/0/0", 134);
        }

        [TestMethod]
        public async Task TestTronVoteWitnessContract()
        {
            ///Vote Witness Account
            ///Vote for Super Representatives or Candidates
            ///https://developers.tron.network/reference#walletvotewitnessaccount-1
            ///Data from https://github.com/CTJaeger
            var transactionRaw5 = "0a02b77d2208634e3da9bdfa61ef40f89bbca0a32d5a880108041283010a30747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e566f74655769746e657373436f6e7472616374124f0a1541bfdc501d1ccc4a7167489c8e670e4954a44c9145121b0a154184399fc6a98edc11a6efb146e86a3e153d0a093310c4b30612190a154184399fc6a98edc11a6efb146e86a3e153d0a0933100570e0d5b8a0a32d";

            await SignTronTransaction(transactionRaw5, "44'/195'/0'/0/0", 134);
        }

        [TestMethod]
        public async Task TestTronExchangeContract()
        {
            ///Exchange Transaction
            ///This API call performs a trade. This is essentially the "buy" and "sell" API calls for decentralized exchanges, rolled into one.
            ///https://developers.tron.network/reference#walletexchangetransaction
            ///Data from https://github.com/CTJaeger
            var transactionRaw6 = "0a02b7832208ca07886003b5260940c8a8bda0a32d5a63082c125f0a38747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e45786368616e67655472616e73616374696f6e436f6e747261637412230a1541bfdc501d1ccc4a7167489c8e670e4954a44c914510191a015f20908e8101280a70e8e8b9a0a32d";

            await SignTronTransaction(transactionRaw6, "44'/195'/0'/0/0", 134);
        }

        [TestMethod]
        public async Task TestEthereumGetAddress()
        {
            LedgerManager.SetCoinNumber(60);
            var address = await LedgerManager.GetAddressAsync(0, 0);

            Console.WriteLine(address);

            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task TestEthereumGetAddressParsed()
        {
            LedgerManager.SetCoinNumber(60);

            //Modern Path
            var path = AddressPathBase.Parse<CustomAddressPath>("m/44'/60'/0'/0/0");
            var address = await LedgerManager.GetAddressAsync(path, false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Legacy Path
            path = AddressPathBase.Parse<CustomAddressPath>("m/44'/60'/0'/0");
            address = await LedgerManager.GetAddressAsync(path, false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        [TestMethod]
        public async Task TestEthereumGetAddressCustomPath()
        {
            LedgerManager.SetCoinNumber(60);

            //Three elements
            var path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0 };
            var address = await LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Four elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60), 0, 1 };
            address = await LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            //Two elements
            path = new uint[] { AddressUtilities.HardenNumber(44), AddressUtilities.HardenNumber(60) };
            address = await LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            Assert.IsTrue(!string.IsNullOrEmpty(address));

            LedgerManager.ErrorPrompt = ThrowErrorInsteadOfPrompt;

            Exception exception = null; ;
            try
            {
                //The ethereum app doesn't like this Purpose (49)
                path = new uint[] { AddressUtilities.HardenNumber(49), AddressUtilities.HardenNumber(60), AddressUtilities.HardenNumber(0), 0, 0 };
                address = await LedgerManager.GetAddressAsync(new CustomAddressPath(path), false, false);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsTrue(exception != null);
        }

        [TestMethod]
        public async Task TestEthereumGetAddresses()
        {
            LedgerManager.SetCoinNumber(60);

            var addressManager = new AddressManager(LedgerManager, new BIP44AddressPathFactory(false, 60));

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

        private async Task GetAddress(uint coinNumber)
        {
            LedgerManager.SetCoinNumber(coinNumber);
            var address = await LedgerManager.GetAddressAsync(0, 0);
            Assert.IsTrue(!string.IsNullOrEmpty(address));
        }

        private async Task SignTronTransaction(string transactionRaw, string path, int? expectedDataLength = null)
        {
            LedgerManager.SetCoinNumber(195);

            var transactionData = new List<byte>();

            for (var i = 0; i < transactionRaw.Length; i += 2)
            {
                var byteInHex = transactionRaw.Substring(i, 2);
                transactionData.Add(Convert.ToByte(byteInHex, 16));
            }

            var derivationData = Helpers.GetDerivationPathData(AddressPathBase.Parse<BIP44AddressPath>(path));

            var firstRequest = new TronAppSignatureRequest(derivationData.Concat(transactionData).ToArray());

            //TODO: don't use the RequestHandler directly.
            var response = await LedgerManager.RequestHandler.SendRequestAsync<TronAppSignatureResponse, TronAppSignatureRequest>(firstRequest);

            var data = response.Data;

            var hexAsString = HexDataToString(data);

            Console.WriteLine(hexAsString);

            Console.WriteLine($"Length: {hexAsString.Length}");

            Assert.IsTrue(response.IsSuccess, $"The response failed with a status of: {response.StatusMessage} ({response.ReturnCode})");

            Assert.IsTrue(!expectedDataLength.HasValue || hexAsString.Length == expectedDataLength, $"Expected legnth {expectedDataLength}. Actual: {hexAsString.Length}");
        }

        private static string HexDataToString(byte[] data)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));
            }
            var hexAsString = sb.ToString();
            return hexAsString;
        }

        private async Task ThrowErrorInsteadOfPrompt(int? returnCode, Exception exception, string member)
        {
            throw new Exception("Ouch!");
        }

        protected async Task Prompt(int? returnCode, Exception exception, string member)
        {
            if (returnCode.HasValue)
            {
                switch (returnCode.Value)
                {
                    case Constants.IncorrectLengthStatusCode:
                        Debug.WriteLine($"Please ensure the app { LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    case Constants.SecurityNotValidStatusCode:
                        Debug.WriteLine($"It appears that your Ledger pin has not been entered, or no app is open. Please ensure the app  {LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    case Constants.InstructionNotSupportedStatusCode:
                        Debug.WriteLine($"The current app is incorrect. Please ensure the app for {LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                    default:
                        Debug.WriteLine($"Something went wrong. Please ensure the app  {LedgerManager.CurrentCoin.App} is open on the Ledger, and press OK");
                        break;
                }
            }
            else
            {
                if (exception is IOException)
                {
                    await Task.Delay(3000);
                    //TODO: don't use the RequestHandler directly.
                    var ledgerManagerTransport = LedgerManager.RequestHandler as LedgerManagerTransport;
                    await ledgerManagerTransport.LedgerHidDevice.InitializeAsync();
                }
            }

            await Task.Delay(5000);
        }

        protected void StartBroker(ErrorPromptDelegate errorPrompt = null)
        {
            _LedgerManagerBroker = new LedgerManagerBroker(3000, null, Prompt, GetLedgerManagerFactory());
            _LedgerManagerBroker.Start();
        }

        protected abstract ILedgerManagerFactory GetLedgerManagerFactory();

        #endregion
    }
}
