﻿using Device.Net;

namespace Ledger.Net
{
    public class LedgerManagerFactory : IManagesLedgerFactory
    {
        public IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new LedgerManager(new LedgerManagerTransport(ledgerHidDevice), coinUtility, errorPrompt);
        }
    }
}
