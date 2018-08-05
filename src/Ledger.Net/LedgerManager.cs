using Hid.Net;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManager
    {
        #region Fields
        private readonly IHidDevice _LedgerHidDevice;
        #endregion

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice)
        {
            _LedgerHidDevice = ledgerHidDevice;
        }
        #endregion

        #region Public Methods
        public async Task<string> GetAddressAsync(string coinShortcut, uint coinNumber, bool isChange, uint index, bool showDisplay, AddressType addressType)
        {

        }
        #endregion
    }
}
