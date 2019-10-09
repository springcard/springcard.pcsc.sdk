# NfcBeam

.NET utility to push and receive NFC Forum NDEF message using the Peer-to-Peer stack (SNEP over LLCP). Currently only initiator role is supported.

**This project is part of the SpringCard SDK for PC/SC.**

**Check the GitHub repository on https://github.com/springcard/springcard.pcsc.sdk for updates.**

## **Abstract**

**NfcBeam** demonstrates the use of `SpringCard.PCSC.NfcForum` class library to implement a basic SNEP server other a LLCP stack running through PC/SC *SCardTransmit* calls.

Due to this intrication with *SCardTransmit*, the application is limited to having the **SpringCard** device running as a P2P initiator (not target).

## Pre-requisites

**NfcBeam** targets the .NET Framework, version 4.6.2 or newer.

## Building the tool

Open `NfcBeam.sln` with Microsoft Visual Studio 2017.

Required class libraries are provided the `binaries/dotnet` directory (starting from the root of the SDK).

The output directory is defined to `_output/NfcForum/NfcBeam` (starting from the root of the SDK). Adjust to your own requirements or habits.

## Warning

THIS IS AN UNSUPPORTED SOFTWARE. USE IT A YOUR OWN RISK.

## License

SpringCard's SDK are available free of charge.

The license allows you to use the featured software (binary or source) freely, provided that the software or any derivative works is used only in link with genuine SpringCard products

Please read LICENSE.TXT for details.

## Changelog

#### v19.9 17/09/2019

Rebuild with latest libraries. Now targetting .NET Framework v4.6.2 (previously: v4.0).

Fixed the concurrence with other applications when trying to connect to the peer (now retrying if another application has been faster).