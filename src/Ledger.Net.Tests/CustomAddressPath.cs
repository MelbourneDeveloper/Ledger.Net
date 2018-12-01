using Hardwarewallets.Net.AddressManagement;

namespace Ledger.Net.Tests
{
    public class CustomAddressPath : AddressPathBase
    {
        private readonly uint[] path;

        public CustomAddressPath()
        {

        }

        public CustomAddressPath(uint[] path)
        {
            foreach (var value in path)
            {
                AddressPathElements.Add(new AddressPathElement { Value = value });
            }
        }
    }
}
