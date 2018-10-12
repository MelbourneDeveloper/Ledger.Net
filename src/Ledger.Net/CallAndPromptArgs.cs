namespace Ledger.Net
{
    public class CallAndPromptArgs<T>
    {
        public string MemberName { get; set; }
        public T Args { get; set; }
        public LedgerManager LedgerManager { get; set; }
    }
}
