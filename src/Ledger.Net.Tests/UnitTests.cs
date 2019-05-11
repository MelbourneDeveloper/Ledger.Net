using Device.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UnitTests : LedgerTests
    {
        MockLedgerDeviceFactory MockLedgerDeviceFactory = new MockLedgerDeviceFactory() { DeviceIds = { "test" } };

        protected override Task GetLedger()
        {
            DeviceManager.Current.DeviceFactories.Add(MockLedgerDeviceFactory);
            MockLedgerDeviceFactory.Register();
            return GetLedgerBase();
        }
    }
}
