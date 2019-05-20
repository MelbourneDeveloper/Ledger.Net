namespace Ledger.Net
{
    public class CallAndPromptArgs<T>
    {
        public string MemberName { get; set; }
        public T Args { get; set; }
        public IManagesLedger LedgerManager { get; set; }
    }
}
