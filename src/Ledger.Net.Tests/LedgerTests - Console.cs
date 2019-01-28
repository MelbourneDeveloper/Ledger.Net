using Hid.Net.Windows;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Ledger.Net.Tests
{
    public  partial class LedgerTests
    {
        private Task GetLedger()
        {
            WindowsHidDeviceFactory.Register();
            WindowsUsbDeviceFactory.Register();
            return GetLedgerBase();
        }
    }
}
