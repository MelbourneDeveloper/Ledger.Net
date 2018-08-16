namespace Ledger.Net
{
    public class CoinInfo : ICoinInfo
    {
        public App App { get; }
        public string ShortName { get; }
        public string FullName { get; }
        public uint CoinNumber { get; }
        public bool IsSegwit { get; }

        public CoinInfo(App app, string shortName, string fullName, uint coinNumber, bool isSegwit)
        {
            App = app;
            ShortName = shortName;
            FullName = fullName;
            CoinNumber = coinNumber;
            IsSegwit = isSegwit;
        }
    }
}
