using Ledger.Net.Requests.Helpers;

namespace Ledger.Net.Requests
{
    public abstract class GetPublicKeyRequestBase : RequestBase
    {
        protected GetPublicKeyRequestBase(uint coinNumber, uint account, uint index, bool isChange, bool isSegwit) : base(PublicKeyHelpers.GetDerivationPathData(coinNumber, account, index, isChange, isSegwit))
        {
        }
    }
}
