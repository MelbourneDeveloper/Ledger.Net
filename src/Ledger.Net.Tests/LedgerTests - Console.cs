using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Ledger.Net.Tests
{
    public class LedgerTestss : LedgerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            WindowsHidDeviceFactory.Register();
            WindowsUsbDeviceFactory.Register();
            StartBroker();
        }
    }
}
