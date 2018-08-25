namespace Ledger.Net
{
    public interface ICoinUtility
    {
        CoinInfo GetCoinInfo(uint coinNumber);
        CoinInfo GetCoinInfo(string coinShortName);
    }
}
