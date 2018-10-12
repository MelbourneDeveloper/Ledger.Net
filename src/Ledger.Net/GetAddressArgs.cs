using Hardwarewallets.Net.Model;

namespace Ledger.Net
{
    public class GetAddressArgs
    {
        public IAddressPath AddressPath { get; set; }
        public bool ShowDisplay { get; set; }

        public GetAddressArgs(IAddressPath addressPath, bool showDisplay)
        {
            AddressPath = addressPath;
            ShowDisplay = showDisplay;
        }
    }

}
