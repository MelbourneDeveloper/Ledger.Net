namespace Ledger.Net
{
    public class LedgerManagerConnectionEventArgs
    {
        public LedgerManager LedgerManager { get; }

        public LedgerManagerConnectionEventArgs(LedgerManager ledgerManager)
        {
            LedgerManager = ledgerManager;
        }

    }
}