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
        private static readonly MockLedgerDeviceFactory MockLedgerDeviceFactory = new MockLedgerDeviceFactory() { };
        public static MockLedgerManagerFactory MockLedgerManagerFactory;
        #endregion

        static UnitTests()
        {
            MockLedgerManagerFactory = new MockLedgerManagerFactory(MockPrompt);
            DeviceManager.Current.DeviceFactories.Add(MockLedgerDeviceFactory);
            StartBroker(MockPrompt, MockLedgerManagerFactory);
        }

        #region Tests
        [TestMethod]
        public async Task TestBitcoinDashboardTooManyPrompts()
        {
            var lastState = MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState;
            Exception lastException = null;
            try
            {
                MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState = CurrentState.Dashboard;

                var address = await LedgerManager.GetAddressAsync(0, 0);

                Assert.IsTrue(!string.IsNullOrEmpty(address));
            }
            catch (TooManyPromptsException tex)
            {
                lastException = tex;
            }
            Assert.IsTrue(lastException is TooManyPromptsException);
            MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState = lastState;
        }
        #endregion

        #region Private Methods
        private static async Task MockPrompt(int? returnCode, Exception exception, string member)
        {
            if (MockLedgerManagerFactory.MockLedgerManagerTransport.CurrentState == CurrentState.Dashboard)
            {
                //TODO: is this the right code for dashboard?
                Assert.IsTrue(returnCode.HasValue && returnCode == Constants.SecurityNotValidStatusCode);
            }

            if (exception != null)
            {
                throw exception;
            }
        }
        #endregion
    }
}
