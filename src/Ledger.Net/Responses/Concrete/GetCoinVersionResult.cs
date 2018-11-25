using System.Linq;
using System.Text;

namespace Ledger.Net.Responses
{
	public class GetCoinVersionResponse: ResponseBase
	{
		#region Constants
		private const int CoinLengthPos = 5;
		private const int SpacerLength = 2;
		#endregion

		#region Public Properties
		public string CoinName { get; }
		public string ShortCoinName { get; }
		#endregion

		#region Constructor
		public GetCoinVersionResponse(byte[] data) : base(data)
		{
            if (!IsSuccess)
            {
                return;
            }

            var coinLength = data[CoinLengthPos];
			var shortCoinNameStartPos = (CoinLengthPos + SpacerLength) + coinLength;
			var shortCoinLength = data[shortCoinNameStartPos - 1];

			var responseList = data.ToList();

			var coinNameData = responseList.GetRange(6, coinLength).ToArray();
			var shortCoinNameData = responseList.GetRange(shortCoinNameStartPos, shortCoinLength).ToArray();

			CoinName = Encoding.ASCII.GetString(coinNameData);
			ShortCoinName = Encoding.ASCII.GetString(shortCoinNameData);
		}
		#endregion
	}
}