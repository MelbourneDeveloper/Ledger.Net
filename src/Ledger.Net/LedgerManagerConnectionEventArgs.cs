using System;

namespace Ledger.Net
{
    public class LedgerManagerConnectionEventArgs : EventArgs
    {
        public LedgerManager LedgerManager { get; }

        public LedgerManagerConnectionEventArgs(LedgerManager ledgerManager)
        {
            LedgerManager = ledgerManager;
        }

    }
}