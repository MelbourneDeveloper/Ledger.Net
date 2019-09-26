using Device.Net;
using Hid.Net.UWP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UWPIntegrationTests : LedgerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            UWPHidDeviceFactory.Register(new DebugLogger(), new DebugTracer());
            StartBroker(null, new LedgerManagerFactory());
        }
    }
}
