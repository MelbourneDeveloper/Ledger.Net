using Device.Net;

namespace Ledger.Net.Tests
{
    public class MockLedgerManagerFactory : ILedgerManagerFactory
    {
        private readonly ErrorPromptDelegate _PromptDelegate;

        public static MockLedgerManagerTransport MockLedgerManagerTransport = new MockLedgerManagerTransport();

        public MockLedgerManagerFactory(ErrorPromptDelegate promptDelegate)
        {
            _PromptDelegate = promptDelegate;
        }

        public IManagesLedger GetNewLedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            return new LedgerManager(MockLedgerManagerTransport, null, _PromptDelegate);
        }
    }
}
