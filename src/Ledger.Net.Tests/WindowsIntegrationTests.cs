using Device.Net;
using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usb.Net.Windows;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class WindowsIntegrationTests : LedgerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            WindowsHidDeviceFactory.Register(new DebugLogger(), new DebugTracer());
            WindowsUsbDeviceFactory.Register(new DebugLogger(), new DebugTracer());
            StartBroker(null, new LedgerManagerFactory());
        }
    }
}
