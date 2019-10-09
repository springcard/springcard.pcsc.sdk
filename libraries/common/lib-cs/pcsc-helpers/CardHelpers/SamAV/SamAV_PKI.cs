using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte PKIKeyEntriesCount = 3;
        public const byte PKIPrivateKeyEntriesCount = 2;
        public const byte PKIPublicKeyEntriesCount = 2;

        public class PKIKeyEntry
        {
            public byte ChangeKeyIdx;
            public byte ChangeKeyVersion;
            public byte CounterIdx;
            public bool PrivateKey;
            public bool DisableKeyEntry;
            public bool EnableDumpPrivateKey;
            public bool EnableRemoteUpdates;
            public bool DisableEncryptDecrypt;
            public bool DisableSignature;
            public bool PrivateKeyHasCrt;

            public PKIKeyEntry()
            {

            }

            public PKIKeyEntry(PKIKeyEntry source)
            {
                this.ChangeKeyIdx = source.ChangeKeyIdx;
                this.ChangeKeyVersion = source.ChangeKeyVersion;
                this.CounterIdx = source.CounterIdx;
                this.SET_HI = source.SET_HI;
                this.SET_LO = source.SET_LO;
            }

            [Flags]
            public enum ESET_LO : byte
            {
                PrivateKey = 0x01,
                EnableDumpPrivateKey = 0x02,
                DisableKeyEntry = 0x04,
                DisableEncryptDecrypt = 0x08,
                DisableSignature = 0x10,
                EnableRemoteUpdates = 0x20,
                PrivateKeyHasCrt = 0x40
            }

            public byte SET_LO
            {
                get
                {
                    byte result = 0;
                    if (PrivateKey)
                        result |= (byte)ESET_LO.PrivateKey;
                    if (EnableDumpPrivateKey)
                        result |= (byte)ESET_LO.EnableDumpPrivateKey;
                    if (DisableKeyEntry)
                        result |= (byte)ESET_LO.DisableKeyEntry;
                    if (DisableEncryptDecrypt)
                        result |= (byte)ESET_LO.DisableEncryptDecrypt;
                    if (DisableSignature)
                        result |= (byte)ESET_LO.DisableSignature;
                    if (EnableRemoteUpdates)
                        result |= (byte)ESET_LO.EnableRemoteUpdates;
                    if (PrivateKeyHasCrt)
                        result |= (byte)ESET_LO.PrivateKeyHasCrt;

                    result |= 0x40; /* CRT */
                    return result;
                }
                set
                {
                    PrivateKey = ((value & (byte)ESET_LO.PrivateKey) != 0);
                    EnableDumpPrivateKey = ((value & (byte)ESET_LO.EnableDumpPrivateKey) != 0);
                    DisableKeyEntry = ((value & (byte)ESET_LO.DisableKeyEntry) != 0);
                    DisableEncryptDecrypt = ((value & (byte)ESET_LO.DisableEncryptDecrypt) != 0);
                    DisableSignature = ((value & (byte)ESET_LO.DisableSignature) != 0);
                    EnableRemoteUpdates = ((value & (byte)ESET_LO.EnableRemoteUpdates) != 0);
                    PrivateKeyHasCrt = ((value & (byte)ESET_LO.PrivateKeyHasCrt) != 0);
                }
            }

            public byte SET_HI
            {
                get
                {
                    return 0;
                }
                set
                {

                }
            }

            public ushort SET
            {
                get
                {
                    return (ushort)((SET_HI << 8) | SET_LO);
                }
                set
                {
                    SET_HI = (byte)(value >> 8);
                    SET_LO = (byte)(value);
                }
            }
        }

        public class PKIPublicKeyEntry : PKIKeyEntry
        {
            public byte[] ValueN;
            public byte[] ValueE;

            public PKIPublicKeyEntry()
            {

            }

            public PKIPublicKeyEntry(PKIPublicKeyEntry source) : base(source)
            {
                this.ValueN = source.ValueN;
                this.ValueE = source.ValueE;
            }

            protected bool IsValidPublicKey()
            {
                if (ValueN == null)
                    return false;
                if (ValueE == null)
                    return false;
                return true;
            }
        }

        public class PKIPrivateKeyEntry : PKIPublicKeyEntry
        {
            public byte[] ValueP;
            public byte[] ValueQ;
            public byte[] ValueDP;
            public byte[] ValueDQ;
            public byte[] ValueIPQ;

            public PKIPrivateKeyEntry()
            {

            }

            public PKIPrivateKeyEntry(PKIPublicKeyEntry source) : base(source)
            {

            }

            public PKIPrivateKeyEntry(PKIPrivateKeyEntry source) : base(source)
            { 
                this.ValueP = source.ValueP;
                this.ValueQ = source.ValueQ;
                this.ValueDP = source.ValueDP;
                this.ValueDQ = source.ValueDQ;
                this.ValueIPQ = source.ValueIPQ;
            }

            protected bool IsValidPrivateKey()
            {
                if (!IsValidPublicKey())
                    return false;
                if (ValueP == null)
                    return false;
                if (ValueQ == null)
                    return false;
                if (ValueDP == null)
                    return false;
                if (ValueDQ == null)
                    return false;
                if (ValueIPQ == null)
                    return false;
                return true;
            }
        }

        public Result PKIGenerateKeyPair(byte KeyIdx, PKIKeyEntry KeyEntry, int PrivateKeyBytes = 256, int PublicExponentBytes = 4, ulong PublicExponentValue = 0x00010001)
        {
            byte[] data = new byte[10 + PublicExponentBytes];
            int offset = 0;

            data[offset++] = KeyIdx;
            data[offset++] = KeyEntry.SET_HI;
            data[offset++] = KeyEntry.SET_LO;
            data[offset++] = KeyEntry.ChangeKeyIdx;
            data[offset++] = KeyEntry.ChangeKeyVersion;
            data[offset++] = KeyEntry.CounterIdx;
            data[offset++] = (byte)(PrivateKeyBytes / 0x0100);
            data[offset++] = (byte)(PrivateKeyBytes % 0x0100);
            data[offset++] = (byte)(PublicExponentBytes / 0x0100);
            data[offset++] = (byte)(PublicExponentBytes % 0x0100);

            ulong t = PublicExponentValue;
            for (int i=0; i<PublicExponentBytes; i++)
            {
                data[offset + PublicExponentBytes - i - 1] = (byte) (t % 0x0100);
                t /= 0x0100;
            }

            return Command(INS.PKIGenerateKeyPair, 0x01, 0x00, data);
        }

        public Result PKIExportPublicKey(byte KeyIdx, out PKIPublicKeyEntry KeyEntry)
        {
            bool first = true;
            int sizeModulus = 0, offsetModulus = 0;
            int sizeExponent = 0, offsetExponent = 0;

            KeyEntry = null;

            for (; ; )
            {
                Result result;
                int offset = 0;
                byte[] data;

                if (first)
                {
                    /* Provide KeyIdx in P1 */
                    result = Command(INS.PKIExportPublicKey, KeyIdx, 0x00, out data, Expect.SuccessOrContinue);
                }
                else
                {
                    /* Chaining - do not provide KeyIdx */
                    result = Command(INS.PKIExportPublicKey, 0x00, 0x00, out data, Expect.SuccessOrContinue);
                }


                if ((result != Result.Success) && (result != Result.Continue))
                    return result;

                if (first)
                {
                    first = false;

                    if ((data == null) || (data.Length < 9))
                        return Result.InvalidResponseData;

                    /* Read pki_entry */
                    KeyEntry = new PKIPublicKeyEntry();
                    KeyEntry.SET_HI = data[offset++];
                    KeyEntry.SET_LO = data[offset++];
                    KeyEntry.ChangeKeyIdx = data[offset++];
                    KeyEntry.ChangeKeyVersion = data[offset++];
                    KeyEntry.CounterIdx = data[offset++];

                    /* Read sizes */
                    sizeModulus = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueN = new byte[sizeModulus];
                    sizeExponent = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueE = new byte[sizeExponent];
                }
                else
                {
                    if (data == null)
                        return Result.InvalidResponseData;
                }

                /* Read the data bytes */
                while (offset < data.Length)
                {
                    if (offsetModulus < sizeModulus)
                    {
                        KeyEntry.ValueN[offsetModulus++] = data[offset++];
                    }
                    else if (offsetExponent < sizeExponent)
                    {
                        KeyEntry.ValueE[offsetExponent++] = data[offset++];
                    }
                    else
                    {
                        return Result.InvalidResponseData;
                    }
                }

                if (result == Result.Success)
                    return result;
            }
        }

        private Result PKIImportKeyEx(byte P1, byte[] data)
        {
            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    Result result = Command(INS.PKIImportKey, P1, 0x00, buffer, Expect.Success);
                    if (result != Result.Success)
                        return result;
                }
                else
                {
                    /* Chaining */
                    Result result = Command(INS.PKIImportKey, P1, 0xAF, buffer, Expect.Continue);
                    if (result != Result.Continue)
                        return result;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return Result.Success;
        }

        public Result PKIImportPublicKey(byte KeyIdx, PKIPublicKeyEntry KeyEntry)
        {
            byte[] data = new byte[10 + KeyEntry.ValueN.Length + KeyEntry.ValueE.Length];
            int offset = 0;

            data[offset++] = KeyIdx;
            data[offset++] = KeyEntry.SET_HI;
            data[offset++] = KeyEntry.SET_LO;
            data[offset++] = KeyEntry.ChangeKeyIdx;
            data[offset++] = KeyEntry.ChangeKeyVersion;
            data[offset++] = KeyEntry.CounterIdx;
            data[offset++] = (byte)(KeyEntry.ValueN.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueN.Length % 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueE.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueE.Length % 0x0100);

            int i;
            for (i = 0; i < KeyEntry.ValueN.Length; i++)
                data[offset++] = KeyEntry.ValueN[i];
            for (i = 0; i < KeyEntry.ValueE.Length; i++)
                data[offset++] = KeyEntry.ValueE[i];

            return PKIImportKeyEx(0x00, data);
        }

        public Result PKIExportPrivateKey(byte KeyIdx, out PKIPrivateKeyEntry KeyEntry)
        {
            bool first = true;
            int sizeModulus = 0, offsetModulus = 0;
            int sizeExponent = 0, offsetExponent = 0;
            int sizeP = 0, offsetP = 0, offsetDP = 0;
            int sizeQ = 0, offsetQ = 0, offsetDQ = 0, offsetQInv = 0;

            KeyEntry = null;

            for (; ; )
            {
                Result result;
                int offset = 0;
                byte[] data;

                if (first)
                {
                    /* Provide KeyIdx in P1 */
                    result = Command(INS.PKIExportPrivateKey, KeyIdx, 0x00, out data, Expect.SuccessOrContinue);
                }
                else
                {
                    /* Chaining - do not provide KeyIdx */
                    result = Command(INS.PKIExportPrivateKey, 0x00, 0x00, out data, Expect.SuccessOrContinue);
                }

                if ((result != Result.Success) && (result != Result.Continue))
                    return result;

                if (first)
                {
                    first = false;

                    if ((data == null) || (data.Length < 9))
                        return Result.InvalidResponseData;

                    /* Read pki_entry */
                    KeyEntry = new PKIPrivateKeyEntry();
                    KeyEntry.SET_HI = data[offset++];
                    KeyEntry.SET_LO = data[offset++];
                    KeyEntry.ChangeKeyIdx = data[offset++];
                    KeyEntry.ChangeKeyVersion = data[offset++];
                    KeyEntry.CounterIdx = data[offset++];

                    /* Read sizes */
                    sizeModulus = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueN = new byte[sizeModulus];
                    sizeExponent = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueE = new byte[sizeExponent];
                    sizeP = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueP = new byte[sizeP];
                    KeyEntry.ValueDP = new byte[sizeP];
                    sizeQ = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.ValueQ = new byte[sizeQ];
                    KeyEntry.ValueDQ = new byte[sizeQ];
                    KeyEntry.ValueIPQ = new byte[sizeQ];
                }
                else
                {
                    if (data == null)
                        return Result.InvalidResponseData;
                }

                /* Read the data bytes */
                while (offset < data.Length)
                {
                    if (offsetModulus < sizeModulus)
                    {
                        KeyEntry.ValueN[offsetModulus++] = data[offset++];
                    }
                    else if (offsetExponent < sizeExponent)
                    {
                        KeyEntry.ValueE[offsetExponent++] = data[offset++];
                    }
                    else if (offsetP < sizeP)
                    {
                        KeyEntry.ValueP[offsetP++] = data[offset++];
                    }
                    else if (offsetQ < sizeQ)
                    {
                        KeyEntry.ValueQ[offsetQ++] = data[offset++];
                    }
                    else if (offsetDP < sizeP)
                    {
                        KeyEntry.ValueDP[offsetDP++] = data[offset++];
                    }
                    else if (offsetDQ < sizeQ)
                    {
                        KeyEntry.ValueDQ[offsetDQ++] = data[offset++];
                    }
                    else if (offsetQInv < sizeQ)
                    {
                        KeyEntry.ValueIPQ[offsetQInv++] = data[offset++];
                    }
                    else
                    {
                        return Result.InvalidResponseData;
                    }
                }

                if (result == Result.Success)
                    return result;
            }
        }

        public Result PKIImportPrivateKey(byte KeyIdx, PKIPrivateKeyEntry KeyEntry)
        {
            byte[] data = new byte[14 + KeyEntry.ValueN.Length + KeyEntry.ValueE.Length + 2 * KeyEntry.ValueP.Length + 3 * KeyEntry.ValueQ.Length];
            int offset = 0;

            data[offset++] = KeyIdx;
            data[offset++] = KeyEntry.SET_HI;
            data[offset++] = KeyEntry.SET_LO;
            data[offset++] = KeyEntry.ChangeKeyIdx;
            data[offset++] = KeyEntry.ChangeKeyVersion;
            data[offset++] = KeyEntry.CounterIdx;
            data[offset++] = (byte)(KeyEntry.ValueN.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueN.Length % 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueE.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueE.Length % 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueP.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueP.Length % 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueQ.Length / 0x0100);
            data[offset++] = (byte)(KeyEntry.ValueQ.Length % 0x0100);

            int i;
            for (i = 0; i < KeyEntry.ValueN.Length; i++)
                data[offset++] = KeyEntry.ValueN[i];
            for (i = 0; i < KeyEntry.ValueE.Length; i++)
                data[offset++] = KeyEntry.ValueE[i];
            for (i = 0; i < KeyEntry.ValueP.Length; i++)
                data[offset++] = KeyEntry.ValueP[i];
            for (i = 0; i < KeyEntry.ValueQ.Length; i++)
                data[offset++] = KeyEntry.ValueQ[i];
            for (i = 0; i < KeyEntry.ValueDP.Length; i++)
                data[offset++] = KeyEntry.ValueDP[i];
            for (i = 0; i < KeyEntry.ValueDQ.Length; i++)
                data[offset++] = KeyEntry.ValueDQ[i];
            for (i = 0; i < KeyEntry.ValueIPQ.Length; i++)
                data[offset++] = KeyEntry.ValueIPQ[i];

            return PKIImportKeyEx(0x00, data);
        }

        public enum EPKIHash : byte
        {
            SHA1 = 0x00,
            SHA224 = 0x01,
            SHA256 = 0x03
        }

        public Result PKIGenerateHash(EPKIHash HashMode, byte[] Message, out byte[] Digest)
        {
            Digest = null;
            byte P1 = (byte)HashMode;
            byte[] data = BinUtils.FromDword((uint) Message.Length, BinUtils.Endianness.BigEndian);
            data = BinUtils.Concat(data, Message);
            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    Result result = Command(INS.PKIGenerateHash, P1, 0x00, buffer, true, out Digest, Expect.Success);
                    if (result != Result.Success)
                        return result;
                }
                else
                {
                    /* Chaining */
                    Result result = Command(INS.PKIGenerateHash, P1, 0xAF, buffer, Expect.Continue);
                    if (result != Result.Continue)
                        return result;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return Result.Success;
        }

        public Result PKIGenerateSignature(byte KeyIdx, EPKIHash HashMode, byte[] Digest, out byte[] Signature)
        {
            Signature = null;
            byte P1 = (byte)HashMode;
            byte[] data = new byte[1 + Digest.Length];
            data[0] = KeyIdx;
            Array.Copy(Digest, 0, data, 1, Digest.Length);

            Result result = Command(INS.PKIGenerateSignature, P1, 0x00, data, Expect.Success);
            if (result != Result.Success)
                return result;

            result = Command(INS.PKISendSignature, 0x00, 0x00, out Signature, Expect.Success);
            if (result != Result.Success)
                return result;

            return Result.Success;
        }

        public Result PKIVerifySignature(byte KeyIdx, EPKIHash HashMode, byte[] Digest, byte[] Signature, out bool Valid)
        {
            Valid = false;
            byte P1 = (byte)HashMode;

            byte[] data = new byte[1 + Digest.Length + Signature.Length];
            data[0] = KeyIdx;
            Array.Copy(Digest, 0, data, 1, Digest.Length);
            Array.Copy(Signature, 0, data, 1 + Digest.Length, Signature.Length);

            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    Result result = Command(INS.PKIVerifySignature, P1, 0x00, buffer, false, out ushort SW);
                    if (result != Result.Success)
                        return result;

                    if (SW == 0x9000)
                        Valid = true;
                }
                else
                {
                    /* Chaining */
                    Result result = Command(INS.PKIVerifySignature, P1, 0xAF, buffer, Expect.Continue);
                    if (result != Result.Continue)
                        return result;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return Result.Success;
        }
    }
}
