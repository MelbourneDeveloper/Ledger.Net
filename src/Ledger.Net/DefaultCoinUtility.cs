using System;

namespace Ledger.Net
{
    public class DefaultCoinUtility : ICoinUtility
    {
        public readonly static CoinInfo Bitcoin = new CoinInfo(App.Bitcoin, "BTC", "Bitcoin", 0, true);
        //Segwit?
        public readonly static CoinInfo Litecoin = new CoinInfo(App.Bitcoin, "LTC", "Litecoin", 2, true);
        public readonly static CoinInfo Ethereum = new CoinInfo(App.Ethereum, "ETH", "Ethereum", 60, false);
        public readonly static CoinInfo EthereumClassic = new CoinInfo(App.Ethereum, "ETC", "Ethereum Classic", 61, false);
        public readonly static CoinInfo BitcoinGold = new CoinInfo(App.BitcoinGold, "BTG", "Bitcoin Gold", 156, false);
        public readonly static CoinInfo BitcoinCash = new CoinInfo(App.Bitcoin, "BCH", "Bitcoin Gold", 145, false);

        public CoinInfo GetCoinInfo(uint coinNumber)
        {
            switch (coinNumber)
            {
                case 0:
                    return Bitcoin;
                case 2:
                    return Litecoin;
                case 60:
                    return Ethereum;
                case 61:
                    return EthereumClassic;
                case 145:
                    return BitcoinCash;
                case 156:
                    return BitcoinGold;
                default:
                    throw new NotImplementedException("Coin not implemented. You can implement your own ICoinUtility for other coins and set CoinUtility in the LedgerManager constructor.");
            }
        }

        public CoinInfo GetCoinInfo(string coinShortName)
        {
            switch (coinShortName)
            {
                case "BTC":
                    return Bitcoin;
                case "LTC":
                    //Segwit?
                    return Litecoin;
                case "ETH":
                    return Ethereum;
                case "ETC":
                    return EthereumClassic;
                case "BTG":
                    return BitcoinGold;
                default:
                    throw new NotImplementedException("Coin not implemented. You can implement your own ICoinUtility for other coins and set CoinUtility in the LedgerManager constructor.");
            }
        }
    }
}
