# Ledger.Net
Cross Platform C# Library for the Ledger Cryptocurrency Hardwarewallet

Join us on Slack:
https://hardwarewallets.slack.com

Twitter:
https://twitter.com/HardfolioApp

Blog:
https://christianfindlay.wordpress.com

Currently supports:
* .NET Framework
* .NET Core
* Android
* UWP 

## Quick Start

- Clone the repo and open the solution
- Connect Ledger and enter pin
- Open Bitcoin app on the device
- Run the GetAddress unit test
- Repeat for Ethereum

For any instructions that are not implemented you will need to create a RequestBase, and ResponseBase class. Then, you will need to call SendRequestAsync. See the see also section.

## NuGet

Install-Package Ledger.Net

## [Hardwarewallets.Net](https://github.com/MelbourneDeveloper/Hardwarewallets.Net)

This library is part of the Hardwarewallets.Net suite of libraries. It is an ambitious project aimed toward putting a set of common C# interfaces across all hardwarewallets

## Contribution

I welcome feedback, and pull requests. If there's something that you need to change in the library, please log an issue, and explain the problem. If you have a proposed solution, please write it up and explain why you think it is the answer to the problem. The best way to highlight a bug is to submit a pull request with a unit test that fails so I can clearly see what the problem is in the first place.

### Pull Requests

Please break pull requests up in to their smallest possible parts. If you have a small feature of refactor that other code depends on, try submitting that first. Please try to reference an issue so that I understand the context of the pull request. If there is no issue, I don't know what the code is about. If you need help, please jump on Slack here: https://hardwarewallets.slack.com

## Donate

All Hardwarewallets.Net libraries are open source and free. Your donations will contribute to making sure that these libraries keep up with the latest hardwarewallet firmware, functions are implemented, and the quality is maintained.

Bitcoin: 33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U

Ethereum: 0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e

Litecoin: MVAbLaNPq7meGXvZMU4TwypUsDEuU6stpY

## Store App Production Usage (Not Yet In Production)

This app currently only Supports Trezor (https://github.com/MelbourneDeveloper/Trezor.Net) but it will soon support Ledger with this library.

https://play.google.com/store/apps/details?id=com.Hardfolio (Android)

https://www.microsoft.com/en-au/p/hardfolio/9p8xx70n5d2j (UWP)

## Hid.Net

Ledger.Net communicates with the device via the Hid.Net library. You can see the repo for this library here:

https://github.com/MelbourneDeveloper/Hid.Net

## See Also

[Hardwarewallets.Net](https://github.com/MelbourneDeveloper/Hardwarewallets.Net) - Base level Hardwarewallet Library

[Trezor.Net](https://github.com/MelbourneDeveloper/Trezor.Net) - Trezor Hardwarewallet Library

[KeepKey.Net](https://github.com/MelbourneDeveloper/KeepKey.Net) - KeepKey Hardwarewallet Library

This library has more instructions implemented out of the box.

https://github.com/LedgerHQ/ledger-dotnet-api

 These are the actual 3rd party apps on the Ledger, their codebase, and samples. These can be used to reverse engineer the Request/Response messages.

https://github.com/LedgerHQ/blue-app-btc

https://github.com/LedgerHQ/blue-app-eth

