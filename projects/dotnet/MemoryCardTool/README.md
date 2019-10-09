# MemoryCardTool

Display and allow to edit the content of contactless memory cards, NFC tags or RFID labels.

**This project is part of the SpringCard SDK for PC/SC.**

**Check the GitHub repository on https://github.com/springcard/springcard.pcsc.sdk for updates.**

## **Abstract**

**MemoryCard Tool** demonstrates the use of a SpringCard NFC/RFID HF PC/SC reader with contactless memory cards.

It uses simple READ BINARY and UPDATE BINARY APDUs to read/write virtually any contactless card, provided that it is supported by the SpringCard device.

Obviously, only non-secure cards are supported by this mean, with the exception of Mifare Classic cards, where the application is able to get authenticated before reading/writing, thanks to the reader's CRYPTO1 unit.

## Pre-requisites

**MemoryCardTool** targets the .NET Framework, version 4.6.2 or newer.

## Building the tool

Open `MemoryCardTool.sln` with Microsoft Visual Studio 2017.

Required class libraries are provided the `binaries/dotnet` directory (starting from the root of the SDK).

The output directory is defined to `_output/MemoryCardTool` (starting from the root of the SDK). Adjust to your own requirements or habits.

## Warning

THIS IS AN UNSUPPORTED SOFTWARE. USE IT A YOUR OWN RISK.

Notably, using this software to write certain cards/tags/labels is likely to lock them down and make it permanently unusable. You've been warned.

## License

SpringCard's SDK are available free of charge.

The license allows you to use the featured software (binary or source) freely, provided that the software or any derivative works is used only in link with genuine SpringCard products

Please read LICENSE.TXT for details.

## Changelog

#### v19.9 17/09/2019

Rebuild with latest libraries. Now targetting .NET Framework v4.6.2 (previously: v4.0).