using Device.Net;
using Hid.Net.UWP;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public partial class LedgerTests
    {
        private Task GetLedger()
        {
            UWPHidDeviceFactory.Register();
            return GetLedgerBase();
        }
    }
}
