# Ledger.Net
Cross Platform C# Library for the Ledger Cryptocurrency Hardwarewallet

**Ledger Nano X Support is here in Version 4.0.0!**

![a](https://cdn.shopify.com/s/files/1/2974/4858/products/ledger-nano-x-stand-up_grande_7a016731-824a-4d00-acec-40acfdfed9dc.png?v=1545313453)

Currently supports: .NET Framework, .NET Core, Android, UWP , See [MacOS and Linux Support](https://github.com/MelbourneDeveloper/Device.Net/wiki/Linux-and-MacOS-Support)

[Would you like to contribute?](https://christianfindlay.com/2019/04/28/calling-all-c-crypto-developers/)

## Quick Start

- Clone the repo and open the solution
- Connect Ledger and enter pin
- Open Bitcoin app on the device
- Run the GetBitcoinPublicKey unit test
- Repeat for Ethereum and Tron etc.

For any instructions that are not implemented you will need to create a RequestBase, and ResponseBase class. Then, you will need to call SendRequestAsync or CallAndPrompt.

NuGet: Install-Package Ledger.Net

[Example](https://github.com/MelbourneDeveloper/Ledger.Net/blob/7b166489eb227ffe56eeb765ba6108d4573ebedc/src/Ledger.Net.Tests/LedgerTests.cs#L125):
```cs
public async Task DisplayAddress()
{
    WindowsHidDeviceFactory.Register(new DebugLogger(), new DebugTracer());
    var ledgerManagerBroker = new LedgerManagerBroker(3000, null, null, new LedgerManagerFactory() );
    var ledgerManager = (IAddressDeriver) await ledgerManagerBroker.WaitForFirstDeviceAsync();
    var path = $"m/49'/0'/0'/0/0";
    var addressPath = AddressPathBase.Parse<BIP44AddressPath>(path);
    var address = await ledgerManager.GetAddressAsync(addressPath, false, true);
}
```
## Contact

- Join us on [Discord](https://discord.gg/ZcvXARm)
- PM me on [Twitter](https://twitter.com/cfdevelop)
- Blog: https://christianfindlay.com/

## [Contribution](https://github.com/MelbourneDeveloper/Ledger.Net/blob/master/CONTRIBUTING.md)

The community needs your help! Unit tests, integration tests, more app integrations and bug fixes please! Check out the Issues section.

## Donate

All my libraries are open source and free. Your donations will contribute to making sure that these libraries keep up with the latest firmware, functions are implemented, and the quality is maintained.

| Coin           | Address |
| -------------  |:-------------:|
| Bitcoin        | [33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U](https://www.blockchain.com/btc/address/33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U) |
| Ethereum       | [0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e](https://etherdonation.com/d?to=0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e) |

## Based On

| Library           | Description |
| -------------  |:-------------:|
| [Hardwarewallets.Net](https://github.com/MelbourneDeveloper/Hardwarewallets.Net) | This library is part of the Hardwarewallets.Net suite. It is aimed toward putting a set of common C# interfaces, and utilities that will work with all hardwarewallets. |
| [Hid.Net, Usb.Net](https://github.com/MelbourneDeveloper/Device.Net)             | Ledger.Net communicates with the devices via the Hid.Net and Usb.Net libraries. You can see the repo for this library here. |

## See Also

| Library           | Description |
| -------------  |:-------------:|
| [Trezor.Net](https://github.com/MelbourneDeveloper/Trezor.Net)                   | Trezor Hardwarewallet Library
| [KeepKey.Net](https://github.com/MelbourneDeveloper/KeepKey.Net)                 | KeepKey Hardwarewallet Library
| [Ledger .NET API](https://github.com/LedgerHQ/ledger-dotnet-api)                 | A similar library |
| [Ledger Bitcoin App](https://github.com/LedgerHQ/blue-app-btc)                   | Bitcoin wallet application for Ledger Blue and Nano S |
| [Ledger Ethereum App](https://github.com/LedgerHQ/blue-app-eth)                  | Ethereum wallet application for Ledger Blue and Nano S |

