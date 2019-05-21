using Device.Net;
using Ledger.Net.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UnitTests : LedgerTests
    {
        #region Fields
        private readonly MockLedgerDeviceFactory MockLedgerDeviceFactory = new MockLedgerDeviceFactory() { };
        public static MockLedgerManagerFactory MockLedgerManagerFactory;
        #endregion

        #region Initialization
        [TestInitialize]
        public void Initialize()
        {
            MockLedgerManagerFactory = new MockLedgerManagerFactory(MockPrompt);
            DeviceManager.Current.DeviceFactories.Add(MockLedgerDeviceFactory);
            StartBroker();
        }
        #endregion

        #region Protected Overrides
        protected override ILedgerManagerFactory GetLedgerManagerFactory()
        {
            return MockLedgerManagerFactory;
        }
        #endregion

        #region Tests
        [TestMethod]
        public async Task TestBitcoinDashboardTooManyPrompts()
        {
            Exception lastException = null;
            try
            {
                MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState = CurrentState.Dashboard;

                var address = await LedgerManager.GetAddressAsync(0, 0);

                Assert.IsTrue(!string.IsNullOrEmpty(address));
            }
            catch(TooManyPromptsException tex)
            {
                lastException = tex;
            }
            Assert.IsTrue(lastException is TooManyPromptsException);
        }
        #endregion

        #region Private Methods
        private async Task MockPrompt(int? returnCode, Exception exception, string member)
        {
            if(MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState == CurrentState.Dashboard)
            {
                //TODO: is this the right code for dashboard?
                Assert.IsTrue(returnCode.HasValue && returnCode == Constants.SecurityNotValidStatusCode);
            }
        }
        #endregion
    }
}
