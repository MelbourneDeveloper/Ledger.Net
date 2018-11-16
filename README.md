# Ledger.Net
Cross Platform C# Library For Ledger Hardwarewallet

Join us on Slack:
https://hardwarewallets.slack.com

Currently supports:
* Android
* Windows
* UWP (Except the UWP Package Manifest has a bug that won't allow the Ledger Vendor Id)

## Quick Start

- Connect Ledger and enter pin
- Open Bitcoin app
- Clone the repo and run the GetAddress unit test
- Repeat for Ethereum

For any instructions that are not implemented you will need to create a RequestBase, and ResponseBase class. Then, you will need to call SendRequestAsync. See the see also section.

**Would love to see some samples for UWP or Android**

## Store App Production Usage (Not Yet In Production)

This app currently only Supports Trezor (https://github.com/MelbourneDeveloper/Trezor.Net) but it will soon support Ledger with this library.

https://play.google.com/store/apps/details?id=com.Hardfolio (Android)

https://www.microsoft.com/en-au/p/hardfolio/9p8xx70n5d2j (UWP)

## NuGet
Install-Package Ledger.Net

## Contribution

I welcome feedback, and pull requests. If there's something that you need to change in the library, please log an issue, and explain the problem. If you have a proposed solution, please write it up and explain why you think it is the answer to the problem. 

# Pull Requests

Please break pull requests up in to their smallest possible parts. If you have a small feature of refactor that other code depends on, try submitting that first. Please try to reference an issue so that I understand the context of the pull request. If there is no issue, I don't know what the code is about. If you need help, please jump on Slack here: https://hardwarewallets.slack.com

## Donate

Bitcoin: 33LrG1p81kdzNUHoCnsYGj6EHRprTKWu3U

Ethereum: 0x7ba0ea9975ac0efb5319886a287dcf5eecd3038e

Litecoin: MVAbLaNPq7meGXvZMU4TwypUsDEuU6stpY

## See Also

This library has more instructions implemented out of the box.

https://github.com/LedgerHQ/ledger-dotnet-api

 These are the actual 3rd party apps on the Ledger, their codebase, and samples. These can be used to reverse engineer the Request/Response messages.

https://github.com/LedgerHQ/blue-app-btc

https://github.com/LedgerHQ/blue-app-eth

