# SpringCard Scriptor XV

**SpringCard Scriptor XV** is a Windows program to send commands to a smartcard -from a batch file or manual entry- through a PC/SC reader.

**SpringCard Scriptor XV** is a descendant of **scriptor** and **gscriptor** written by Ludovic Rousseau (see http://ludovic.rousseau.free.fr/softwares/pcsc-tools/ ).

## What's new in 2015?

In the past, **SpringCard** used to provide 2 scripting utilities
* a scripting utility for the PC/SC readers -most of them being USB-based devices
* a scripting utility for the SpringCard Legacy readers, using the SpringProx API to communicate with the reader over a serial line (namely CSB4, K632 and early K663 devices).

The introduction of K663 v2.00 firmware branch in 2015 allows to fill the gap between the 2 utilities.
Thanks to the introduction of most PC/SC-related features into the K663 (code-name 'CCID over Serial'), it is now possible to provide a single utility that works both with PC/SC readers and up-to-date versions of CSB4.6 and K663.

More than that, a new family of readers if being put on the market, the core communication channel being CCID over TCP/IP (code-name 'CCID over Network'). The new scripting utility is therefore able to communicate with this new readers (E663 core, FunkyGate-IP PC/SC).

## Using the application

Launch **Scriptor XV** from Windows' Start Menu (don't have a Start Menu on your Windows 8? Too bad, tell Microsoft how much you dislike the new interface).

On the top-right of the application's screen, click the *Reader* link to select the reader you want to work with. You may choose either a PC/SC reader, a 'CCID over Serial' or 'CCID over Network' reader.
(Note that the application will remember the reader you select, and could reconnect automatically to the same reader the next time you open it).

Put a card on the reader. The application shows the card's ATR (Answer to Reset, see our **glossary** http://tech.springcard.com/glossary if you're not familiar with the smartcard-related vocabulary).

You may now edit your script in the input box on the left. Enter the Command APDUs you want to send (see the **glossary** again for APDU), and click *Run* on the botton right. The result box on the right shows the Command APDU(s) and the Response APDU(s) received from the card.

For instance, you may start by sending the very basic 'FF CA 00 00 00' APDU to retrieve the card's protocol-level identifier (aka serial number, unique identifier, pseudo-unique identifier, random identifier... depending on the card and on its configuration).

## Recompiling the application

**Scriptor XV** has been developed in C# for the .NET 4.5 platform.

Use SharpDevelop 4 or 5 to open the .sln file and rebuild the application.

## License

See the LICENSE.txt file in the same directory.

