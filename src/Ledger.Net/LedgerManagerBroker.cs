using Device.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManagerBroker : IDisposable
    {
        #region Protected Abstract Properties
        protected List<FilterDeviceDefinition> DeviceDefinitions { get; } = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x2c97, ProductId=0x0004  }
       };

        #endregion

        #region Fields
        private DeviceListener _DeviceListener;
        private SemaphoreSlim _Lock = new SemaphoreSlim(1, 1);
        private TaskCompletionSource<IManagesLedger> _FirstLedgerTaskCompletionSource = new TaskCompletionSource<IManagesLedger>();
        private bool disposed;
        #endregion

        #region Events
        /// <summary>
        /// Occurs after the LedgerManagerBroker notices that a device hasbeen connected, and initialized
        /// </summary>
        public event EventHandler<LedgerManagerConnectionEventArgs> LedgerInitialized;

        /// <summary>
        /// Occurs after the LedgerManagerBroker notices that the device has been disconnected, but before the LedgerManager is disposed
        /// </summary>
        public event EventHandler<LedgerManagerConnectionEventArgs> LedgerDisconnected;
        #endregion

        #region Public Properties
        public ReadOnlyCollection<IManagesLedger> LedgerManagers { get; private set; } = new ReadOnlyCollection<IManagesLedger>(new List<IManagesLedger>());
        public ICoinUtility CoinUtility { get; }
        public int? PollInterval { get; }
        public ErrorPromptDelegate ErrorPromptDelegate { get; }
        public ILedgerManagerFactory LedgerManagerFactory { get; }
        #endregion

        #region Constructor
        public LedgerManagerBroker(int? pollInterval, ICoinUtility coinUtility, ErrorPromptDelegate errorPromptDelegate, ILedgerManagerFactory ledgerManagerFactory)
        {
            CoinUtility = coinUtility;
            PollInterval = pollInterval;
            ErrorPromptDelegate = errorPromptDelegate;
            LedgerManagerFactory = ledgerManagerFactory;
        }
        #endregion

        #region Event Handlers
        private async void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            try
            {
                await _Lock.WaitAsync();

                var LedgerManager = LedgerManagers.FirstOrDefault(t =>
                {
                    var ledgerManagerTransport = t.RequestHandler as LedgerManagerTransport;
                    return ReferenceEquals(ledgerManagerTransport?.LedgerHidDevice, e.Device);
                });

                if (LedgerManager == null)
                {
                    LedgerManager = LedgerManagerFactory.GetNewLedgerManager(e.Device, CoinUtility, ErrorPromptDelegate);

                    var tempList = new List<IManagesLedger>(LedgerManagers)
                    {
                        LedgerManager
                    };

                    LedgerManagers = new ReadOnlyCollection<IManagesLedger>(tempList);

                    if (_FirstLedgerTaskCompletionSource.Task.Status == TaskStatus.WaitingForActivation) _FirstLedgerTaskCompletionSource.SetResult(LedgerManager);

                    LedgerInitialized?.Invoke(this, new LedgerManagerConnectionEventArgs(LedgerManager));
                }
            }
            finally
            {
                _Lock.Release();
            }
        }

        private async void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            try
            {
                await _Lock.WaitAsync();

                var LedgerManager = LedgerManagers.FirstOrDefault(t =>
                {
                    var ledgerManagerTransport = t.RequestHandler as LedgerManagerTransport;
                    return ReferenceEquals(ledgerManagerTransport?.LedgerHidDevice, e.Device);
                });

                if (LedgerManager != null)
                {
                    LedgerDisconnected?.Invoke(this, new LedgerManagerConnectionEventArgs(LedgerManager));

                    LedgerManager.Dispose();

                    var tempList = new List<IManagesLedger>(LedgerManagers);

                    tempList.Remove(LedgerManager);

                    LedgerManagers = new ReadOnlyCollection<IManagesLedger>(tempList);
                }
            }
            finally
            {
                _Lock.Release();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the device listener that manages the connection and disconnection of devices
        /// </summary>
        public void Start()
        {
            Start(false);
        }

        /// <summary>
        /// Starts the device listener that manages the connection and disconnection of devices
        /// </summary>
        public void Start(bool restart)
        {
            if (restart && _DeviceListener != null)
            {
                LedgerManagers = new ReadOnlyCollection<IManagesLedger>(new List<IManagesLedger>());
                _DeviceListener.DeviceDisconnected -= DevicePoller_DeviceDisconnected;
                _DeviceListener.DeviceInitialized -= DevicePoller_DeviceInitialized;
                _DeviceListener.Dispose();
                _DeviceListener = null;
            }

            if (_DeviceListener == null)
            {
                _DeviceListener = new DeviceListener(DeviceDefinitions, PollInterval)
                {
                    Logger = new DebugLogger()
                };

                _DeviceListener.DeviceDisconnected += DevicePoller_DeviceDisconnected;
                _DeviceListener.DeviceInitialized += DevicePoller_DeviceInitialized;
                _DeviceListener.Start();
            }
        }

        public void Stop()
        {
            _DeviceListener?.Stop();
        }

        /// <summary>
        /// Check to see if there are any devices connected
        /// </summary>
        public async Task CheckForDevicesAsync()
        {
            try
            {
                await _DeviceListener.CheckForDevicesAsync();
            }
#pragma warning disable CA1031 
            catch
            {
            }
#pragma warning restore CA1031 
        }

        /// <summary>
        /// Starts the device listener and waits for the first connected Ledger to be initialized
        /// </summary>
        /// <returns></returns>
        public async Task<IManagesLedger> WaitForFirstDeviceAsync()
        {
            if (_DeviceListener == null) Start();
            await _DeviceListener.CheckForDevicesAsync();
            return await _FirstLedgerTaskCompletionSource.Task;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _Lock.Dispose();
            _DeviceListener.Stop();
            _DeviceListener.Dispose();

            foreach (var LedgerManager in LedgerManagers)
            {
                LedgerManager.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        ~LedgerManagerBroker()
        {
            Dispose();
        }
        #endregion
    }
}



