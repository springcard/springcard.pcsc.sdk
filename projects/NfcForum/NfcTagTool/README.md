# NfcTagTool

.NET utility to read and write NFC Forum Tags.

**This project is part of the SpringCard SDK for PC/SC.**

**Check the GitHub repository on https://github.com/springcard/springcard.pcsc.sdk for updates.**

## **Abstract**

**NfcTagTool** demonstrates the use of `SpringCard.PCSC.NfcForum` class library to manipulate NFC Forum data (NDEF, RTD) and to read/write NFC Forum Tags.

It is designed, and tested, with non-secure, memory-only Tags in mind, namely NFC Forum Type 2 Tags (NXP NTAG or Mifare UL, Infineon my-d NFC etc) or NFC Forum Type 5 Tags (NXP ICODE-SLI, Texas Instrument TagIT etc).

Reading NFC Forum Type 4 Tags is OK, but only Desfire-based Tags could be formatted and written.

NFC Forum Type 1 Tags (Innovision / Broadcomm Topaz etc) and NFC Forum Type 3 Tags (Sony FeliCa Lite) shoud supported but have not been tested recently.

## Pre-requisites

**NfcTagTool** targets the .NET Framework, version 4.6.2 or newer.

## Building the tool

Open `NfcTagTool.sln` with Microsoft Visual Studio 2017.

Required class libraries are provided the `binaries/dotnet` directory (starting from the root of the SDK).

The output directory is defined to `_output/NfcForum/NfcTagTool` (starting from the root of the SDK). Adjust to your own requirements or habits.

## Warning

THIS IS AN UNSUPPORTED SOFTWARE. USE IT A YOUR OWN RISK.

Notably, using this software to write certain cards/tags/labels is likely to lock them down and make it permanently unusable. You've been warned.

## License

SpringCard's SDK are available free of charge.

The license allows you to use the featured software (binary or source) freely, provided that the software or any derivative works is used only in link with genuine SpringCard products

Please read LICENSE.TXT for details.

## Changelog

#### v19.9 17/09/2019

To avoid confusion, name changed to NfcTagTool (formerly: NfcTool).

Rebuild with latest libraries. Now targetting .NET Framework v4.6.2 (previously: v4.0).