using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public class MockLedgerDeviceFactory : IDeviceFactory
    {
        public DeviceType DeviceType => throw new NotImplementedException();

        public List<string> DeviceIds { get; } = new List<string>();

        internal static void Register()
        {
            DeviceManager.Current.DeviceFactories.Add(new MockLedgerDeviceFactory());
        }

        public Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return Task.FromResult(DeviceIds.Select(d => new ConnectedDeviceDefinition(d)));
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            return null;
        }
    }
}
