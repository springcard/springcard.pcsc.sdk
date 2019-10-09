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
        public enum Expect
        {
            Success,
            Continue,
            SuccessOrContinue,
            DontCare
        }

        private Result CommandEx(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out byte[] out_data, out ushort out_sw, Expect expect)
        {
            out_data = null;
            out_sw = 0xFFFF;

            CAPDU capdu;

            if (in_data != null)
            {
                if (send_le)
                {
                    capdu = new CAPDU(CLA, (byte)ins, P1, P2, in_data, 0x00);
                }
                else
                {
                    capdu = new CAPDU(CLA, (byte)ins, P1, P2, in_data);
                }
            }
            else
            {
                capdu = new CAPDU(CLA, (byte)ins, P1, P2, 0x00);
            }

            Logger.Debug("<{0}", capdu);

            RAPDU rapdu = samReader.Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return Result.CommunicationError;
            }

            Logger.Debug(">{0}", rapdu);

            out_data = rapdu.DataBytes;
            out_sw = rapdu.SW;

            Result result;

            switch (rapdu.SW)
            {
                case 0x9000:
                    if (expect != Expect.Continue)
                    {
                        result = Result.Success;
                    }
                    else
                    {
                        result = Result.UnexpectedStatus;
                    }
                    break;

                case 0x90AF:
                    if (expect != Expect.Success)
                    {
                        result = Result.Continue;
                    }
                    else
                    {
                        result = Result.UnexpectedStatus;
                    }
                    break;

                default:
                    if (expect != Expect.DontCare)
                    {
                        OnStatusWordError(rapdu.SW);
                        result = Result.ExecutionFailed;
                    }
                    else
                    {
                        result = Result.Success;
                    }
                    break;
            }

            return result;
        }

        public Result Command(INS ins, byte P1, byte P2, byte[] in_data, Expect expect)
        {
            return CommandEx(ins, P1, P2, in_data, false, out byte[] out_data, out ushort out_sw, expect);
        }

        public Result Command(INS ins, byte P1, byte P2, byte[] in_data)
        {
            return CommandEx(ins, P1, P2, in_data, false, out byte[] out_data, out ushort out_sw, Expect.Success);
        }

        public Result Command(INS ins, byte P1, byte P2, out byte[] out_data, Expect expect)
        {
            return CommandEx(ins, P1, P2, null, true, out out_data, out ushort out_sw, expect);
        }

        public Result Command(INS ins, byte P1, byte P2, out byte[] out_data)
        {
            return CommandEx(ins, P1, P2, null, true, out out_data, out ushort out_sw, Expect.Success);
        }

        public Result Command(INS ins, byte P1, byte P2, Expect expect)
        {
            return CommandEx(ins, P1, P2, null, true, out byte[] out_data, out ushort out_sw, expect);
        }

        public Result Command(INS ins, byte P1, byte P2)
        {
            return CommandEx(ins, P1, P2, null, true, out byte[] out_data, out ushort out_sw, Expect.Success);
        }

        public Result Command(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out ushort out_sw)
        {
            return CommandEx(ins, P1, P2, in_data, send_le, out byte[] out_data, out out_sw, Expect.DontCare);
        }

        public Result Command(INS ins, byte P1, byte P2, byte[] in_data, bool send_le, out byte[] out_data, Expect expect)
        {
            return CommandEx(ins, P1, P2, in_data, send_le, out out_data, out ushort out_sw, expect);
        }

        public Result GetKeyEntry(byte keyIdx, out byte[] keyEntryData)
        {
            keyEntryData = null;

            CAPDU capdu = new CAPDU(0x80, 0x64, keyIdx, 0x00, 0x00);

            Logger.Debug("<{0}", capdu);

            RAPDU rapdu = samReader.Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return Result.CommunicationError;
            }

            Logger.Debug(">{0}", rapdu);

            if (rapdu.SW != 0x9000)
            {
                OnStatusWordError(rapdu.SW);
                return Result.ExecutionFailed;
            }

            keyEntryData = rapdu.DataBytes;
            return Result.Success;
        }

        public Result ChangeKeyEntryAV2(byte keyIdx, byte[] keyEntryData)
        {
            Logger.Debug("ChangeKeyEntryAV2({0}, {1})", keyIdx, BinConvert.ToHex(keyEntryData));

            if (keyEntryData == null)
            {
                Logger.Error("No data");
                userInteraction.Error("Key Entry " + BinConvert.ToHex(keyIdx) + " has no data.");
                return Result.InvalidParameters;
            }

            if (keyEntryData.Length != 61)
            {
                Logger.Error("Invalid data length");
                userInteraction.Error("Key Entry " + BinConvert.ToHex(keyIdx) + " has an invalid length.");
                return Result.InvalidParameters;
            }

            Byte set0 = keyEntryData[55];
            CAPDU capdu;
            if (((set0 & 0x38) == 0x18) || ((set0 & 0x38) == 0x28))
            {
                /* 24-byte keys : don't update VC (in P2)	*/
                capdu = new CAPDU(0x80, (byte)INS.ChangeKeyEntry, keyIdx, 0xDF, keyEntryData);
            }
            else
            {
                capdu = new CAPDU(0x80, (byte)INS.ChangeKeyEntry, keyIdx, 0xFF, keyEntryData);
            }

            Logger.Debug("<{0}", capdu);

            RAPDU rapdu = samReader.Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return Result.CommunicationError;
            }

            Logger.Debug(">{0}", rapdu);

            if (rapdu.SW != 0x9000)
            {
                Logger.Error("SW={0:X04}", rapdu.SW);
                userInteraction.Error(string.Format("Failed to change key entry {0:X02} (SAM error)", keyIdx));
                return Result.ExecutionFailed;
            }

            return Result.Success;
        }

        public Result ChangePkiEntry(byte keyEntry, byte[] keyEntryData, bool setRandomKeyValues)
        {
            Debug("ChangePkiEntry(" + BinConvert.ToHex(keyEntry) + ")");

            if (keyEntryData == null)
            {
                Debug("No data!");
                userInteraction.Error("Pki Entry " + BinConvert.ToHex(keyEntry) + " has no data.");
                return Result.InvalidParameters;
            }

            if (keyEntryData.Length == 0)
            {
                Debug("Invalid length!");
                userInteraction.Error("Pki Entry " + BinConvert.ToHex(keyEntry) + " has an invalid length.");
                return Result.InvalidParameters;
            }

            if (setRandomKeyValues)
            {
                throw new Exception("This part is not implemented!");
            }

            CAPDU capdu;
            RAPDU rapdu;

            byte[] cmde;
            byte[] data;

            if (keyEntryData.Length > 248)
            {
                // write 14 header before data PKI_No to PKi_ipq
                //0000630000ff0100000400800080
                int offset = 13;
                if (keyEntry == 0x02)
                {
                    offset = 9;
                }

                data = BinUtils.Copy(keyEntryData, 0, offset);

                cmde = new byte[data.Length + 1];

                //cmde[0] = 0x0E;
                cmde[0] = keyEntry;
                data.CopyTo(cmde, 1);

                capdu = new CAPDU(0x80, 0x19, 0x00, 0xAF, cmde);
                Debug("CAPDU: " + capdu.AsString());

                rapdu = samReader.Transmit(capdu);
                if (rapdu == null)
                    return Result.CommunicationError;

                Debug("RAPDU: " + rapdu.AsString());

                // maximum two call to get full key
                if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0xAF))
                {
                    do
                    {
                        if ((keyEntryData.Length - offset) > 248)
                        {
                            cmde = BinUtils.Copy(keyEntryData, offset, 248);
                            capdu = new CAPDU(0x80, 0x19, 0x00, 0xAF, cmde);
                        }
                        else
                        {
                            cmde = BinUtils.Copy(keyEntryData, offset, keyEntryData.Length - offset);
                            capdu = new CAPDU(0x80, 0x19, 0x00, 0x00, cmde);
                        }
                        Debug("CAPDU: " + capdu.AsString());

                        rapdu = samReader.Transmit(capdu);
                        if (rapdu == null)
                            return Result.CommunicationError;
                        
                        Debug("RAPDU: " + rapdu.AsString());
                        offset += 248;
                    }
                    while ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0xAF));

                }
            }
            else
            {
                // rest of command
                data = BinUtils.Copy(keyEntryData, 0, keyEntryData.Length);
                cmde = new byte[data.Length + 1];
                data.CopyTo(cmde, 1);
                cmde[0] = (byte)keyEntryData.Length;

                capdu = new CAPDU(0x80, 0x19, 0x00, 0x00, cmde);
                Debug("CAPDU: " + capdu.AsString());

                rapdu = samReader.Transmit(capdu);
                if (rapdu == null)
                    return Result.CommunicationError;
                
                Debug("RAPDU: " + rapdu.AsString());
            }

            if ((rapdu.SW != 0x9000))
            {
                Logger.Debug("ChangePkiEntry(" + BinConvert.ToHex(keyEntry) + "): SW=" + BinConvert.ToHex(rapdu.SW));
				userInteraction.Error(string.Format("Failed to change PKI key entry {0:X02} (SAM error)", keyEntry));
				return Result.ExecutionFailed;
            }

            return Result.Success;
        }

        public bool ChangeKeyEntry(byte KeyEntry, string val)
        {
            Debug("------------------------------Changing Key Entry " + String.Format("{0:x02}", KeyEntry) + "------------------------------");
            Debug("New value: " + val);
            byte[] new_key_entry_data;

            /* Tests sur val	*/
            if (val == null)
            {
                Debug("No value to change !");
                userInteraction.Error("Key Entry " + String.Format("{0:x02}", KeyEntry).ToUpper() + " has no value !");
                return false;
            }
            if (val.Length != 120)
            {
                Debug("New value has invalid length");
                userInteraction.Error("Key Entry " + String.Format("{0:x02}", KeyEntry).ToUpper() + " has invalid length.");
                return false;
            }

            for (int i = 0; i < val.Length; i++)
            {
                if (!isByte(val[i]))
                {
                    Debug("New value is not in hexadecimal format");
                    userInteraction.Error("Key Entry " + String.Format("{0:x02}", KeyEntry).ToUpper() + " is not in hexadecimal format.");
                    return false;
                }
            }

            CardBuffer CardBufKey_entry = new CardBuffer(val);
            new_key_entry_data = CardBufKey_entry.GetBytes();

            byte[] data_to_encrypt = new byte[new_key_entry_data.Length + 4];
            int offset = 0;

            for (int i = 0; i < new_key_entry_data.Length; i++)
                data_to_encrypt[offset++] = new_key_entry_data[i];

            /* ------- calculate crc and concat	-------	*/
            if (currentAuthType == AuthTypeE.TDES_CRC16)
            {
                Debug("Calculating CRC16 and padding with 00 00 ...");
                byte[] crc = CryptoPrimitives.ComputeCrc16(new_key_entry_data);
                data_to_encrypt[offset++] = crc[0];
                data_to_encrypt[offset++] = crc[1];
                data_to_encrypt[offset++] = 0x00;
                data_to_encrypt[offset++] = 0x00;

            }
            else
            if ((currentAuthType == AuthTypeE.AES) || (currentAuthType == AuthTypeE.TDES_CRC32))
            {
                Debug("Calculating CRC32 ...");
                byte[] crc = CryptoPrimitives.ComputeCrc32(new_key_entry_data);
                data_to_encrypt[offset++] = crc[0];
                data_to_encrypt[offset++] = crc[1];
                data_to_encrypt[offset++] = crc[2];
                data_to_encrypt[offset++] = crc[3];

            }
            else
            {
                Debug("Can't calculate CRC: Authentication Key is not 3DES nor AES");
                userInteraction.Error("Authentication Key must be 3DES or AES.\nKey Entry " + String.Format("{0:x02}", KeyEntry).ToUpper() + " can't be changed.");
                return false;
            }

            string data = "";
            for (int i = 0; i < data_to_encrypt.Length; i++)
                data += "-" + data_to_encrypt[i];

            /*------- encrypt	------- */
            Debug("Encrypting ...");
            byte[] encrypted_data;

            if ((currentAuthType == AuthTypeE.TDES_CRC16) || (currentAuthType == AuthTypeE.TDES_CRC32))
            {
                encrypted_data = CryptoPrimitives.TripleDES_Encrypt(data_to_encrypt, session_key, initVector);
            }
            else
            if (currentAuthType == AuthTypeE.AES)
            {
                encrypted_data = CryptoPrimitives.AES_Encrypt(data_to_encrypt, session_key, initVector);
            }
            else
            {
                Debug("Can't encrypt: Authentication Key is not 3DES nor AES");
                userInteraction.Error("Authentication Key must be 3DES or AES.\nKey Entry " + String.Format("{0:x02}", KeyEntry).ToUpper() + " can't be changed.");
                return false;
            }

            /*	 Keep IV	*/
            if (keepInitVector)
            {
                Debug("Keeping IV ...");

                if (currentAuthType == AuthTypeE.AES)
                {
                    /* il faut 16 octets	*/
                    Array.ConstrainedCopy(encrypted_data, encrypted_data.Length - 16, initVector, 0, initVector.Length);
                }
                else
                {
                    /*	il faut 8 octets	*/
                    Array.ConstrainedCopy(encrypted_data, encrypted_data.Length - 8, initVector, 0, initVector.Length);
                }
            }

            /*	------- Send APDU	------- 	*/
            Debug("Sending 'Change Entry' Command ...");
            CAPDU capdu = new CAPDU(0x80, (byte)INS.ChangeKeyEntry, KeyEntry, 0xFF, encrypted_data);
            RAPDU rapdu = samReader.Transmit(capdu);
            Debug("Capdu=" + capdu.AsString());
            Debug("Rapdu=" + rapdu.AsString());
            //MessageBox.Show("Changement de cle " + String.Format("{0:x02}", KeyEntry) + " RAPDU=" + rapdu.AsString());

            if (rapdu.AsString().Equals("90 00"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
