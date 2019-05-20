using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usb.Net.Windows;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class WindowsUnitTests : LedgerTests
    {
        protected override ILedgerManagerFactory GetLedgerManagerFactory()
        {
            return new LedgerManagerFactory();
        }

        [TestInitialize]
        public void Initialize()
        {
            WindowsHidDeviceFactory.Register();
            WindowsUsbDeviceFactory.Register();
            StartBroker();
        }
    }
}
