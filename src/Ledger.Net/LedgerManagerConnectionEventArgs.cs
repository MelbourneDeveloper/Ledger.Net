using System;

namespace Ledger.Net
{
    public class LedgerManagerConnectionEventArgs : EventArgs
    {
        public ILedgerManager LedgerManager { get; }

        public LedgerManagerConnectionEventArgs(ILedgerManager ledgerManager)
        {
            LedgerManager = ledgerManager;
        }

    }
}