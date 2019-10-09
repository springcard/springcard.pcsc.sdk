# SpringCard PC/SC SDK

## About this SDK

The PC/SC SDK is developed by SpringCard and is available for free to all Springcard’s customers, and helps to use with SpringCard’s PC/SC products.

## Legal disclaimer

THE SDK IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

## Abstract

### What is PC/SC?

PC/SC is the de-facto standard to interface Personal Computers with SmartCards.

PC/SC is implemented by Winscard.dll on Windows and PCSC-lite on Linux and Mac OS.

### Supported devices

Any SpringCard PC/SC coupler may be used with this SDK.

The SDK is tested with, Prox’n’Roll PC/SC HSP, a CrazyWriter-HSP and PUCK.

This page contains a complete list of SpringCard’s PC/SC couplers:

[www.springcard.com/en/products.html](http://www.springcard.com/en/products.html)

This other page describes how to upgrade SpringCard PC/SC couplers:

http://tech.springcard.com/firmware-upgrade/

### Supported platforms

This SDK targets Windows, Linux and Mac OS.

Most examples in this SDK are heavily tested on Windows only, anyway, the C samples should compile and link for the other platforms too.

All the C# and Visual Basic samples run over the .NET Framework v4.6.2, either on Windows or on top of Mono for Linux and Mac OS.

***Notes***

- See [github.com/springcard/android-pcsclike/](https://github.com/springcard/android-pcsclike/) for a PC/SC-Like implementation on Android
- See [github.com/springcard/ios-pcsclike/](https://github.com/springcard/ios-pcsclike/) for a PC/SC-Like implementation on Android

### Physical organisation of the SDK

This SDK is available either as a ZIP file or through GitHub: [github.com/springcard/springcard.pcsc.sdk](https://github.com/springcard/springcard.pcsc.sdk)

The ZIP contains, at its root, 4 directories.

| Directory   | Content                                                      |
| ----------- | ------------------------------------------------------------ |
| `runimage`  | Sample applications compiled for Windows or .NET             |
| `binaries`  | Binary version of the SpringCard libraries + external dependencies |
| `projects`  | Source code of the sample applications                       |
| `libraries` | Source code of most of the SpringCard libraries              |

### Documentation of the libraries

The documentation of the SpringCard libraries is available online:

| Library                    | Description                                                  | Link                                                         |
| -------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| `SpringCard.PCSC`          | Use PC/SC couplers and smart cards from .NET applications    | [docs.springcard.com/apis/NET/PCSC](https://docs.springcard.com/apis/NET/PCSC) |
| `SpringCard.PCSC.Helpers`  | High-level access to a few current cards and to advanced coupler features | [docs.springcard.com/apis/NET/PCSC-Helpers](https://docs.springcard.com/apis/NET/PCSC-Helpers) |
| `SpringCard.LibCs`         | A set of utilities to simplify the development of .NET application | [docs.springcard.com/apis/NET/Utils](https://docs.springcard.com/apis/NET/Utils) |
| `SpringCard.LibCs.Windows` | A set of utilities for the Windows platform                  | [docs.springcard.com/apis/NET/Windows](https://docs.springcard.com/apis/NET/Windows) |

### Required tools

This SDK requires a standard C compiler and a .Net environment.

The .Net examples and projects can be used with Visual Studio 2015 Community Edition or SharpDevelop.

Both are respectively available here:

* [www.microsoft.com/en-us/download/details.aspx?id=48146](https://www.microsoft.com/en-us/download/details.aspx?id=48146)

* [www.icsharpcode.net/opensource/sd/](http://www.icsharpcode.net/opensource/sd/)

To work with the beginners examples (see below), a MIFARE Ultralight card, a MIFARE Classic card and a MIFARE DESFire card are required.

Please contact our sales team, should  you need to purchase some cards for your tests.

## Getting started

The `projects/dotnet/beginners` directory contains a few very basic examples in C# and in Visual Basic. They are all build on to of the `SpringCard.PCSC.dll` library targeting .NET framework v4.6.2 (and Mono).

The C# solution comes with these sample projects:

* **listReaders**: It’s the simplest example, it lists all the PC/SC readers installed on  the computer. That’s the project to start with.

* **getCardUid**: This program built on the listReaders example, retrieves the readers but also retrieves some data from the inserted card as the ATR, UID, protocol and type.

* **readMifareUltralight**: This project reads all the content of a Mifare Ultralight card.

* **writeMifareUltralight**: This project writes some ASCII (text) data to a Mifare Ultralight card. It only write textual data but  it’s not a limitation of the card.

* **readMifareClassic**: Reads data (only textual data) from a Mifare Classic card.

* **writeMifareClassic**: Writes (textual) data in the card to a specific address.

* **desfireInformation**: Retrieves information from a desFire card. It reads the card’s version and lists the available applications.

* **getReaderInformation**: This project get some information from the reader:
* Vendor's Name
    
* Product's name
    
* Product's Serial number
    
* USB vendor ID and product ID
    
* Product's version
    
* etc

This program differs from the preceding ones, in that it shows how to communicate with the reader instead of communicating with the card.

Please note that when dealing with PC/SC, it is highly recommended to use threads, in order not to block the user interface. It is also mandatory to handle all possible exceptions.

All those examples don’t necessary use threads nor handle exceptions because the goal is to demonstrate how to deal with PC/SC, cards and readers.


## Other samples included in this SDK

### MemoryCardTool

A C# application to "explore" memory cards (show data and modify them). It shows data in raw mode.

### PcscDiag2

A C# application to show the PC/SC readers connected to the system and to work with the cards at APDU level.

### ScriptorXV

A C# application to send commands to a smartcard -from a batch file or manual entry- through a PC/SC reader.

### C

The `projects/c/src` directory contains all the samples written in ANSI C.

### NfcTagTool

A C# application to read and write NFC Forum Tags. Compliant with NFC Forum Type 2 Tags (Mifare UltraLight, NTAG etc) and pre-formatted NFC Forum Type 4 Tags.

### NfcTagEmul

A C# application to have the device emulate a NFC Forum Tag. Works with PUCK only.

### HCE-Demo

A C# application to have the device enter host card emulation mode. Works with PUCK only.

## See also...

### Other interesting open-source projects

#### pcsc-tools

On most Linux's distribution you can find the package pcsc-tools which provides some free software tools to send APDU commands to a card on Linux.

* [Official website](http://ludovic.rousseau.free.fr/softwares/pcsc-tools/)
* [Git repository](https://github.com/LudovicRousseau/pcsc-tools)

#### CardPeek

CardPeek can read cards' data, and is extensible with a scripting language (LUA). CardPeek runs on Windows, Linux, FreeBSD and Mac OS X.

* [Official website](http://pannetrat.com/Cardpeek)
* [Git repository](https://github.com/L1L1/cardpeek)

#### pcsc-sharp

pcsc-sharp is an alternative to `SpringCard.PCSC` library.

See [github.com/danm-de/pcsc-sharp](https://github.com/danm-de/pcsc-sharp).

## How to contact us

For technical information, this form is available:

[www.springcard.com/en/support/contact](http://www.springcard.com/en/support/contact)

For any commercial request, please use this other form:

[www.springcard.com/en/contact](http://www.springcard.com/en/contact)