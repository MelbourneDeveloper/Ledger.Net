using Device.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UnitTests : LedgerTests
    {
        private readonly MockLedgerDeviceFactory MockLedgerDeviceFactory = new MockLedgerDeviceFactory() {  };

        [TestInitialize]
        public void Initialize()
        {
            DeviceManager.Current.DeviceFactories.Add(MockLedgerDeviceFactory);
            StartBroker();
        }

        protected override ILedgerManagerFactory GetLedgerManagerFactory()
        {
            return new MockLedgerManagerFactory();
        }
    }
}
