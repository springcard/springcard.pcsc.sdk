# PcscDiag2

PC/SC Diagnostic and test Tool for .NET.

**This project is part of the SpringCard SDK for PC/SC.**

**Check the GitHub repository on https://github.com/springcard/springcard.pcsc.sdk for updates.**

## **Abstract**

**PscDiag2** is an handy tool to check the installation of the PC/SC readers and drivers.

It is also suitable to perform 'quick and dirty' tests with any smartcard in no time: connect to the card by double-clicking its ATR, write your Command APDU, click *SCardTransmit*, and get the card's Response APDU immediately.

The tool may also be used to send direct commands to the reader through *SCardControl*.

Right-click a reader and select *Info* to get information regarding the firmware version.

## Pre-requisites

**PscDiag2** targets the .NET Framework, version 4.6.2 or newer.

## Building the tool

Open `PcscDiag2.sln` with Microsoft Visual Studio 2017.

Required class libraries are provided the `binaries/dotnet` directory (starting from the root of the SDK).

The output directory is defined to `_output/PcscDiag2` (starting from the root of the SDK). Adjust to your own requirements or habits.

## Warning

THIS IS AN UNSUPPORTED SOFTWARE. USE IT A YOUR OWN RISK.

## License

SpringCard's SDK are available free of charge.

The license allows you to use the featured software (binary or source) freely, provided that the software or any derivative works is used only in link with genuine SpringCard products

Please read LICENSE.TXT for details.

## Changelog

#### v19.9 17/09/2019

Rebuild with latest libraries. Now targetting .NET Framework v4.6.2 (previously: v4.0).