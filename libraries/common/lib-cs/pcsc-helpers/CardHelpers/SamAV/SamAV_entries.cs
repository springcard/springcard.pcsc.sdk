/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 17/07/2012
 * Time: 09:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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
        public const byte KeyEntriesCount = 128;

        public class KeyEntry
        {
            public byte VersionA;
            public byte VersionB;
            public byte VersionC;
            public uint DesfireAid;
            public byte DesfireKeyIdx;
            public byte ChangeKeyIdx;
            public byte ChangeKeyVersion;
            public byte CounterIdx;

            public enum KeyTypeE : byte
            {
                DesfireEV0 = 0,
                TripleDes_Iso10116_Crc16_Mac32 = 1,
                Mifare = 2,
                TripleDes3K = 3,
                Aes128 = 4,
                Aes192 = 5,
                TripleDes_Iso10116_Crc32_Mac64 = 6,
                Rfu7 = 7
            }
            public KeyTypeE KeyType;

            public enum KeyClassE : byte
            {
                Host = 0,
                PICC = 1,
                OfflineChange = 2,
                Rfu3 = 3,
                OfflineCrypto = 4,
                Rfu5 = 5,
                Rfu6 = 6,
                Rfu7 = 7
            }
            public KeyClassE KeyClass;

            public bool KeepIV;
            public bool HostAuthKey;
            public bool LockUnlockKey;
            public bool DiversifiedOnly;
            public bool DisableKeyEntry;
            public bool EnableDumpSecretKey;
            public bool EnableDumpSessionKey;
            public bool DisableWriteToPICC;
            public bool DisableDecrypt;
            public bool DisableEncrypt;
            public bool DisableVerifyMAC;
            public bool DisableGenerateMAC;
            public byte[] ValueA;
            public byte[] ValueB;
            public byte[] ValueC;

            public KeyEntry()
            {
                CounterIdx = 0xFF;
                SET = 0x2002;
                ExtSET = 0x00;
            }

            protected bool IsValidSecretKey()
            {
                if (ValueA == null)
                    return false;
                if (ValueB == null)
                    return false;
                if (ValueC == null)
                    return false;
                return true;
            }

            public KeyEntry(KeyEntry source)
            {
                this.VersionA = source.VersionA;
                this.VersionB = source.VersionB;
                this.VersionC = source.VersionC;
                this.DesfireAid = source.DesfireAid;
                this.DesfireKeyIdx = source.DesfireKeyIdx;
                this.ChangeKeyIdx = source.ChangeKeyIdx;
                this.ChangeKeyVersion = source.ChangeKeyVersion;
                this.CounterIdx = source.CounterIdx;
                this.SET_HI = source.SET_HI;
                this.SET_LO = source.SET_LO;
                this.ExtSET = source.ExtSET;
                this.ValueA = source.ValueA;
                this.ValueB = source.ValueB;
                this.ValueC = source.ValueC;
            }

            [Flags]
            public enum ESET_HI : byte
            {
                EnableDumpSessionKey = 0x01,
                KeepIV = 0x04,
            }

            public byte SET_HI
            {
                get
                {
                    byte result = 0;
                    if (EnableDumpSessionKey)
                        result |= (byte)ESET_HI.EnableDumpSessionKey;
                    if (KeepIV)
                        result |= (byte)ESET_HI.KeepIV;
                    result |= (byte)(((byte)KeyType << 3) & 0x38);
                    return result;
                }
                set
                {
                    KeyType = (KeyTypeE)((value >> 3) & 0x07);
                    KeepIV = ((value & (byte)ESET_HI.KeepIV) != 0);
                    EnableDumpSessionKey = ((value & (byte)ESET_HI.EnableDumpSessionKey) != 0);
                }
            }

            [Flags]
            public enum ESET_LO : byte
            {
                HostAuthKey = 0x01,
                DisableKeyEntry = 0x02,
                LockUnlockKey = 0x04,
                DisableWriteToPICC = 0x08,
                DisableDecrypt = 0x10,
                DisableEncrypt = 0x20,
                DisableVerifyMAC = 0x40,
                DisableGenerateMAC = 0x80,
            }

            public byte SET_LO
            {
                get
                {
                    byte result = 0;
                    if (HostAuthKey)
                        result |= (byte)ESET_LO.HostAuthKey;
                    if (DisableKeyEntry)
                        result |= (byte)ESET_LO.DisableKeyEntry;
                    if (LockUnlockKey)
                        result |= (byte)ESET_LO.LockUnlockKey;
                    if (DisableWriteToPICC)
                        result |= (byte)ESET_LO.DisableWriteToPICC;
                    if (DisableDecrypt)
                        result |= (byte)ESET_LO.DisableDecrypt;
                    if (DisableEncrypt)
                        result |= (byte)ESET_LO.DisableEncrypt;
                    if (DisableVerifyMAC)
                        result |= (byte)ESET_LO.DisableVerifyMAC;
                    if (DisableGenerateMAC)
                        result |= (byte)ESET_LO.DisableGenerateMAC;
                    return result;
                }
                set
                {
                    HostAuthKey = ((value & (byte)ESET_LO.HostAuthKey) != 0);
                    DisableKeyEntry = ((value & (byte)ESET_LO.DisableKeyEntry) != 0);
                    LockUnlockKey = ((value & (byte)ESET_LO.LockUnlockKey) != 0);
                    DisableWriteToPICC = ((value & (byte)ESET_LO.DisableWriteToPICC) != 0);
                    DisableDecrypt = ((value & (byte)ESET_LO.DisableDecrypt) != 0);
                    DisableEncrypt = ((value & (byte)ESET_LO.DisableEncrypt) != 0);
                    DisableVerifyMAC = ((value & (byte)ESET_LO.DisableVerifyMAC) != 0);
                    DisableGenerateMAC = ((value & (byte)ESET_LO.DisableGenerateMAC) != 0);
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

            public byte ExtSET
            {
                get
                {
                    byte result = 0;
                    result |= (byte)(((byte)KeyClass) & 0x07);
                    if (EnableDumpSecretKey)
                        result |= 0x08;
                    if (DiversifiedOnly)
                        result |= 0x10;
                    return result;
                }
                set
                {
                    KeyClass = (KeyClassE)(value & 0x07);
                    EnableDumpSecretKey = ((value & 0x08) != 0);
                    DiversifiedOnly = ((value & 0x10) != 0);
                }
            }

            public byte[] ToChangeKeyEntry()
            {
                byte[] result = new byte[61];

                int offset = 0;
                Array.Copy(ValueA, 0, result, offset, 16); offset += 16;
                Array.Copy(ValueB, 0, result, offset, 16); offset += 16;
                Array.Copy(ValueC, 0, result, offset, 16); offset += 16;
                result[offset++] = (byte)(DesfireAid >> 16);
                result[offset++] = (byte)(DesfireAid >> 8);
                result[offset++] = (byte)(DesfireAid);
                result[offset++] = DesfireKeyIdx;
                result[offset++] = ChangeKeyIdx;
                result[offset++] = ChangeKeyVersion;
                result[offset++] = CounterIdx;
                result[offset++] = SET_HI;
                result[offset++] = SET_LO;
                result[offset++] = VersionA;
                result[offset++] = VersionB;
                result[offset++] = VersionC;
                result[offset++] = ExtSET;

                return result;
            }

            public void FromGetKeyEntry(byte[] GetKeyEntryData, bool VersionsAhead)
            {
                int offset = 0;

                if (VersionsAhead)
                {
                    VersionA = GetKeyEntryData[offset++];
                    VersionB = GetKeyEntryData[offset++];
                    VersionC = GetKeyEntryData[offset++];
                }
                DesfireAid = 0;
                DesfireAid |= GetKeyEntryData[offset++]; DesfireAid <<= 8;
                DesfireAid |= GetKeyEntryData[offset++]; DesfireAid <<= 8;
                DesfireAid |= GetKeyEntryData[offset++];
                DesfireKeyIdx = GetKeyEntryData[offset++];
                ChangeKeyIdx = GetKeyEntryData[offset++];
                ChangeKeyVersion = GetKeyEntryData[offset++];
                CounterIdx = GetKeyEntryData[offset++];
                SET_HI = GetKeyEntryData[offset++];
                SET_LO = GetKeyEntryData[offset++];
                if (!VersionsAhead)
                {
                    VersionA = GetKeyEntryData[offset++];
                    VersionB = GetKeyEntryData[offset++];
                    VersionC = GetKeyEntryData[offset++];
                }
                ExtSET = GetKeyEntryData[offset++];
            }
        }

        public Result GetKeyEntry(byte keyIdx, out KeyEntry keyEntry)
        {
            keyEntry = null;
            Result result = GetKeyEntry(keyIdx, out byte[] keyEntryData);
            if (result != Result.Success)
                return result;

            keyEntry = new KeyEntry();

            int offset = 0;
            keyEntry.VersionA = keyEntryData[offset++];
            keyEntry.VersionB = keyEntryData[offset++];
            keyEntry.VersionC = keyEntryData[offset++];

            keyEntry.DesfireAid = keyEntryData[offset++]; keyEntry.DesfireAid <<= 8;
            keyEntry.DesfireAid |= keyEntryData[offset++]; keyEntry.DesfireAid <<= 8;
            keyEntry.DesfireAid |= keyEntryData[offset++];

            keyEntry.DesfireKeyIdx = keyEntryData[offset++];
            keyEntry.ChangeKeyIdx = keyEntryData[offset++];
            keyEntry.ChangeKeyVersion = keyEntryData[offset++];
            keyEntry.CounterIdx = keyEntryData[offset++];

            keyEntry.SET_HI = keyEntryData[offset++];
            keyEntry.SET_LO = keyEntryData[offset++];

            return Result.Success;
        }

        public Result GetKeyEntrySecretKeys(byte keyIdx, ref KeyEntry keyEntry, bool ignoreErrors = false)
        {
            Result result;

            result = DumpSecretKey(keyIdx, keyEntry.VersionA, out keyEntry.ValueA);
            if ((result != Result.Success) && !ignoreErrors)
                return result;
            if (keyEntry.VersionB != keyEntry.VersionA)
            {
                result = DumpSecretKey(keyIdx, keyEntry.VersionB, out keyEntry.ValueB);
                if ((result != Result.Success) && !ignoreErrors)
                    return result;
            }
            if ((keyEntry.VersionC != keyEntry.VersionA) && (keyEntry.VersionC != keyEntry.VersionB))
            {
                result = DumpSecretKey(keyIdx, keyEntry.VersionC, out keyEntry.ValueC);
                if ((result != Result.Success) && !ignoreErrors)
                    return result;
            }

            return Result.Success;
        }

        public Result DumpSecretKey(byte KeyIdx, byte KeyVersion, out byte[] KeyValue)
        {
            byte P1 = 0x00;
            byte[] in_data = new byte[2];
            in_data[0] = KeyIdx;
            in_data[1] = KeyVersion;

            Result result = Command(INS.DumpSecretKey, P1, 0x00, in_data, true, out KeyValue, Expect.Success);
            if (result != Result.Success)
            {
                KeyValue = null;
                return result;
            }

            return Result.Success;
        }

        public Result ChangeKeyEntry(byte keyIdx, KeyEntry keyEntry)
        {
            byte[] data = keyEntry.ToChangeKeyEntry();
            byte P2;
            byte set0 = data[55];
            if (((set0 & 0x38) == 0x18) || ((set0 & 0x38) == 0x28))
            {
                /* 24-byte keys : don't update VC (in P2)	*/
                P2 = 0xDF;
            }
            else
            {
                P2 = 0xFF;
            }

            return Command(INS.ChangeKeyEntry, keyIdx, P2, data);
        }



















        public const int SymmetricKeyCount = 128;
        public const int AsymmetricKeyCount = 3;



        public abstract class Entry
        {
            public byte Index { get; private set; }

            public byte[] Value { get; private set; }

            public bool MustWrite { get; private set; }
            public bool MustGenerate { get; private set; }

            public static Entry CreateFromIniLine(byte index, string IniValue)
            {
                if (index < SymmetricKeyCount)
                {
                    if (IniValue.Length == 124)
                    {
                        string iniValueSym = IniValue.Substring(0, 122);
                        string iniValueFlags = IniValue.Substring(122, 2);
                        byte[] valueSym = BinConvert.HexToBytes(iniValueSym);
                        if (valueSym.Length != 61)
                            return null;

                        SymmetricEntry result = new SymmetricEntry();
                        result.Index = index;
                        result.Value = valueSym;
                        if (iniValueFlags.Contains("W"))
                            result.MustWrite = true;
                        if (iniValueFlags.Contains("R"))
                            result.MustGenerate = true;

                        return result;
                    }
                }

                if ((index >= SymmetricKeyCount) && (index < SymmetricKeyCount + AsymmetricKeyCount))
                {
                    int firstIndex = IniValue.IndexOf('-');
                    int lastIndex = IniValue.LastIndexOf('-');

                    if (firstIndex < lastIndex)
                    {
                        string iniKeyEntryAsym = IniValue.Substring(0, firstIndex);
                        lastIndex += 1;
                        string iniKeyValueAsym = IniValue.Substring(lastIndex, IniValue.Length - (lastIndex));
                        string iniValueFlags = IniValue.Substring(firstIndex, lastIndex);

                        

                        AsymetricEntry result = new AsymetricEntry();
                        result.Index = index;
                        byte[] valueAsym = BinConvert.HexToBytes(iniKeyValueAsym);
                        result.Value = valueAsym;

                        if (iniValueFlags.Contains("W"))
                            result.MustWrite = true;
                        if (iniValueFlags.Contains("R"))
                            result.MustGenerate = true;

                        return result;
                    }
                }

                return null;
            }
        }

        public class Entries
        {
            List<Entry> entries = new List<Entry>();

            public int Length { get { return entries.Count; } }
            public Entry this[int index] { get { return entries[index]; } }


            public static Entries CreateFromIniFile(string FileName)
            {
                IniFile iniFile = new IniFile(FileName);

                Entries result = new Entries();

                for (int index=0; index<SymmetricKeyCount + AsymmetricKeyCount; index++)
                {
                    string IniKey = string.Format("{0}", index);
                    string IniValue = iniFile.ReadString("Configuration", IniKey, null);
                    if (IniValue != null)
                    {
                        Entry entry = Entry.CreateFromIniLine((byte) index, IniValue);
                        if (entry != null)
                            result.entries.Add(entry);
                    }
                }

                return result;
            }
        }

        public class SymmetricEntry : Entry
        {
            public byte ChangeKeyEntry
            {
                get
                {
                    return Value[51];
                }
            }

            public byte ChangeKeyVersion
            {
                get
                {
                    return Value[52];
                }
            }
        }

        public class AsymetricEntry : Entry
        {

        }


    }
}
