# PassKitRdr

This is SpringCard's terminal-side implementation of the Apple VAS NFC protocol (aka Apple Wallet or Apple PassKit).

The solution is made of 4 projects:

- **PassKitRdr.exe** : the GUI application,
- **SpringCard.AppleVas.dll** : the class Library that actually implements all the interesting parts (APDU exchange and cryptographic stuff),
- **PassKitReadCli.exe** : a command-line application that does only the reading (APDU exchange),
- **PassKitDecryptCli.exe** : a command-line application that does only the decryption (cryptographic part).

They all target the .NET Framework version 4.6.2, and are compiled using Microsoft Visual Studio 2017.

## Documentation of the Library

The complete and up-to-date documentation is available online at:

https://docs.springcard.com/apis/NET/AppleVAS/

### Using the Library in your own project

Observe the workflow that is implemented in **PassKitRdr**'s MainForm:

1. Configure an Apple VAS merchant: a **merchant Id** and a **private key** valid other the ECC P-256 curve. Note that the **merchant Id** is no more than the SHA-256 hash of the human-readable **merchant name**; of course these merchant data must match a valid pass in the mobiles,

```C#
AppleVasConfig config = new AppleVasConfig(merchantId);
config.AddPrivateKey(merchantPrivateKey);
```

2. List the PC/SC readers (using the `SCARD.Readers` method of the **SpringCard.PCSC.dll** class Library) and choose the NFC reader that will communicate with the NFC-enabled mobiles, and instantiate a **SCardReader** object over it:

```C#
SCardReader activeReader = new SCardReader(selectedReaderName);
```

3. Let the **SCardReader** object create a thread to monitor the reader (and later on to perform the transaction with the mobiles or NFC cards) using

```C#
activeReader.StartWaitCard(
    new SCardReader.CardConnectedCallback(CardConnectedCallback)),
	new SCardReader.CardRemovedCallback(CardRemovedCallback))
);
```

4. In **CardConnectedCallback**, instantiate an **AppleVasTerminal** object over the merchant configuration, and run a transaction other the newly connected smartcard, whose connection is encapsulated in a **SCardChannel** object:

```C#
AppleVasTerminal terminal = new AppleVasTerminal(config);
bool result = terminal.DoTransaction(
    cardChannel,
    out AppleVasData data,
    out AppleVasError error
);
```

Note that **CardConnectedCallback** (and **CardRemovedCallback** as well) are called from a background thread. You shall not access any control or GUI object directly. Please read [this article](<https://docs.microsoft.com/fr-fr/dotnet/framework/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls>) for reference.

5. If `result` is `false`, the `error` output explains what has gone wrong. If `result` is `true`, the `data` output is populated with the message that has been received from the mobile and decrypted.
6. The **AppleVasData** object contains two fields: the actual message (available as **data.Text** or **data.Bytes** depending you want a string or the raw value) and its time-stamp (**data.TimeStamp**). The time-stamp has been set by the mobile. The application <u>shall</u> verify that the time-stamp is acceptable (not too old, not too young) to prevent replay attacks. Of course the computer's clock must be right...
7. The **CardRemovedCallback** fires when the mobile leaves the NFC reader.

### License restriction

The **SpringCard.AppleVas.dll** is not free.

Customer shall buy a license. Please contact sales@springcard.com for information.

#### Loading a license

Every application that uses the **SpringCard.AppleVas.dll** shall supply a license string to the Library at the very beginning of its execution.

There are two ways to supply a license string to the Library:

- Store the license string in a file named **SpringCard.AppleVas.License.dat**, in the directory of the application (and of the Library), and invoke the Library's `AppleVasLicense.AutoLoad()` static method,

or

- Store the license string wherever you want (registry entry, configuration file, etc) and invoke the Library's `AppleVasLicense.LoadFromText(string LicenseText)` static method.

**Tip:** both methods return a `bool` to tell whether a valid License has been loaded or not. Tell the user that the application is not able to go any further if either method return `false`.

#### Activating the license

Licenses are tied a SpringCard PC/SC reader (or to a set of PC/SC readers). Therefore, the Library must be fed with the serial number of the reader it is working with.

Invoke the Library's `AppleVasLicense.ReadDeviceId(SCardChannel CardChannel)` static method where the `CardChannel` object refers to a SpringCard PC/SC reader <u>before</u> using any other function of the Library.

It **PassKitRdr**, this is done right into the **CardConnectedCallback** function.

#### Evaluation licenses

Use `AppleVasLicense.Get().IsEvaluation` to know whether you have a full-featured license, or an evaluation license.

Evaluation licenses have a limit in execution time: they stop working after 10 minutes. You may anyway exit the application, and start it again, to have a session of 10 minutes.