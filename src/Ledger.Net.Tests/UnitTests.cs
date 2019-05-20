using Device.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UnitTests : LedgerTests
    {
        private readonly MockLedgerDeviceFactory MockLedgerDeviceFactory = new MockLedgerDeviceFactory() { DeviceIds = { "test" } };

        [TestInitialize]
        public void GetLedger()
        {
            DeviceManager.Current.DeviceFactories.Add(MockLedgerDeviceFactory);
            _LedgerManagerBroker = new LedgerManagerBroker(3000, null, Prompt, new MockLedgerManagerFactory());
            StartBroker();
        }
    }
}
