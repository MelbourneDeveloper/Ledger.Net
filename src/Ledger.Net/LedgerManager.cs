using Hid.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManager
    {
        #region Fields
        private readonly IHidDevice _LedgerHidDevice;
        protected SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
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
            return null;
        }

        public async Task<TResponse> SendMessageAsync<TResponse, TWrite>(TWrite message)
           where TResponse : ResponseBase
           where TWrite : RequestBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {

            }
            finally
            {
                _SemaphoreSlim.Release();
            }

            return default(TResponse);
        }
        #endregion
    }
}
