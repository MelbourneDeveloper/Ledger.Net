using System;

namespace Ledger.Net
{
    public class LedgerManagerConnectionEventArgs : EventArgs
    {
        public IManagesLedger LedgerManager { get; }

        public LedgerManagerConnectionEventArgs(IManagesLedger ledgerManager)
        {
            LedgerManager = ledgerManager;
        }

    }
}