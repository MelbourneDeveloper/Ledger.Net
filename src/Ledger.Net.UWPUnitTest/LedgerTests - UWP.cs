using Hid.Net;
using Hid.Net.UWP;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public partial class LedgerTests
    {
        private static Task<IHidDevice> GetLedgerDevice()
        {
            var taskCompletionSource = new TaskCompletionSource<IHidDevice>();
            var uwpHidDevice = new UWPHidDevice();
            var uwpHidDevicePoller = new UWPHidDevicePoller(1, 0x2c97, uwpHidDevice);
            uwpHidDevice.Connected += (a, b) => taskCompletionSource.SetResult(uwpHidDevice);
            return taskCompletionSource.Task;
        }
    }
}
