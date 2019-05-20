﻿using Hid.Net.UWP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ledger.Net.Tests
{
    [TestClass]
    public class UWPLedgerTests : LedgerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            UWPHidDeviceFactory.Register();
            StartBroker();
        }

        protected override ILedgerManagerFactory GetLedgerManagerFactory()
        {
            return new LedgerManagerFactory();
        }
    }
}
