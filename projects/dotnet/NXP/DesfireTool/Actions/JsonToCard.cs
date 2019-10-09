using Sprincard.DesfireTool;
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;
using System.IO;
using System.Collections.Generic;

namespace DesfireTool
{
    partial class Program
    {
        Result JsonToCard(Desfire desfire, string fileName)
        {
            Logger.Trace("Reading input file {0}", fileName);

            string strInput;

            try
            {
                strInput = File.ReadAllText(fileName);
            }
            catch
            {
                Console.WriteLine("Failed to read {0}", fileName);
                return Result.ReadFromFileFailed;
            }

            JSONObject jsonInput;

            try
            {
                jsonInput = JSONDecoder.Decode(strInput);
            }
            catch
            {
                Console.WriteLine("Content of file {0} is malformed", fileName);
                return Result.InvalidDataInFile;
            }

            if (!DesfireContent.TryCreateFromJSON(jsonInput, out DesfireContent content))
            {
                Console.WriteLine("Content in file {0} is invalid", fileName);
                return Result.InvalidDataInFile;
            }

            return JsonToCard(desfire, content);
        }

        Result JsonToCard(Desfire desfire, DesfireContent content)
        {
            Result result;
            Parameters parameters;

            /* first of all, erase the card! */
            parameters = new Parameters();
            parameters.ApplicationId = 0;
            result = SelectApplication(desfire, parameters);
            if (result != Result.Success)
                goto failed;

            parameters = new Parameters();
            parameters.KeyId = 0;
            parameters.KeyValue = new byte[16];
            result = Authenticate(desfire, parameters);
            if (result != Result.Success)
                goto failed;

            parameters = new Parameters();
            result = Format(desfire, parameters);
            if (result != Result.Success)
                goto failed;            

            /* create all applications (if any) */
            foreach (KeyValuePair<byte, DesfireApplication> applicationEntry in content.DesfireApplicationEntries)
            {
                parameters = new Parameters();
                parameters.ApplicationId = applicationEntry.Value.aid;
                parameters.KeySettings = new byte[2];
                parameters.KeySettings[0] = applicationEntry.Value.keySettings1;
                parameters.KeySettings[1] = applicationEntry.Value.keySettings2;

                result = CreateApplication(desfire, parameters);
                if (result != Result.Success)
                    goto failed;

                /* Select this application */
                result = SelectApplication(desfire, parameters);
                if (result != Result.Success)
                    goto failed;

                /* okay, are there some files to create? */
                foreach (KeyValuePair<byte, DesfireFile> fileEntry in applicationEntry.Value.DesfireFileEntries)
                {
                    parameters = new Parameters();
                    parameters.FileId = fileEntry.Value.FileID;
                    parameters.CommMode = fileEntry.Value.bCommMode;
                    parameters.AccessRights = (long) fileEntry.Value.bFileAccess;
                    parameters.Size = fileEntry.Value.Size;

                    parameters.LowerLimit = fileEntry.Value.ValueMin;
                    parameters.UpperLimit = fileEntry.Value.ValueMax;
                    parameters.InitialValue = fileEntry.Value.Value;

                    parameters.Count = fileEntry.Value.RecordCount;
                    parameters.ValueFlags = 0;
                    if (fileEntry.Value.LimitedCreditEnabled)
                        parameters.ValueFlags |= 0x01;
                    if (fileEntry.Value.FreeGetValueEnabled)
                        parameters.ValueFlags |= 0x02;

                    switch (fileEntry.Value.Type)
                    {
                        case "standard":
                            result = CreateStdDataFile(desfire, parameters);
                            break;
                        case "value":
                            result = CreateValueFile(desfire, parameters);
                            break;
                        case "cyclic":
                            parameters.Size = fileEntry.Value.RecordSize;
                            result = CreateCyclicRecordFile(desfire, parameters);
                            break;
                        case "linear":
                            parameters.Size = fileEntry.Value.RecordSize;
                            result = CreateLinearRecordFile(desfire, parameters);
                            break;
                        case "backup":
                            result = CreateBackupDataFile(desfire, parameters);
                            break;
                        default:
                            result = Result.InvalidArgument;
                            break;
                    }

                    if (result != Result.Success)
                        goto failed;
                }

                /* now, authenticate with this application's master key (still 0) */
                parameters = new Parameters();
                parameters.KeyId = 0;
                switch (applicationEntry.Value.KeyType)
                {                    
                    case DesfireApplication.KeyTypeEnum.KEY_TYPE_AES:
                        parameters.KeyValue = new byte[16];
                        result = AuthenticateAES(desfire, parameters);
                        break;
                    case DesfireApplication.KeyTypeEnum.KEY_TYPE_3DES:
                        parameters.KeyValue = new byte[16];
                        result = Authenticate(desfire, parameters);
                        break;
                    case DesfireApplication.KeyTypeEnum.KEY_TYPE_3DES3K:
                        parameters.KeyValue = new byte[24];
                        result = AuthenticateISO(desfire, parameters);
                        break;
                    default:
                        result = Result.InvalidArgument;
                        break;
                }

                if (result != Result.Success)
                    goto failed;

                /* write data if available! */
                foreach (KeyValuePair<byte, DesfireFile> fileEntry in applicationEntry.Value.DesfireFileEntries)
                {
                    parameters = new Parameters();
                    parameters.Data = fileEntry.Value.Data;
                    if (parameters.Data != null)
                    {
                        parameters.FileId = fileEntry.Value.FileID;
                        parameters.CommMode = fileEntry.Value.bCommMode;
                        parameters.Offset = fileEntry.Value.Offset;
                        parameters.Size = fileEntry.Value.Size;

                        /* shall we use authentication for writing? */
                        if ( (fileEntry.Value.WriteKeyIdx == 0x0E) || (fileEntry.Value.ReadWriteKeyIdx == 0x0E) )
                        {
                            parameters.CommMode = 0; /* use plain communication! */
                            Logger.Info("Must use plain communication to write datas!");
                        }

                        /* write data! */
                        switch (fileEntry.Value.Type)
                        {
                            case "standard":
                            case "backup":
                                result = WriteData(desfire, parameters);
                                break;
                            case "cyclic":
                            case "linear":
                                parameters.Number = 0;
                                parameters.Size = fileEntry.Value.RecordSize;
                                result = WriteRecord(desfire, parameters);
                                if (result == Result.Success)
                                {
                                    /* Validate all previous write access on Backup Data Files, Value Files and Record Files within one application! */
                                    result = CommitTransaction(desfire, parameters);
                                }
                                break;
                            default:
                                result = Result.InvalidArgument;
                                break;
                        }

                        if (result != Result.Success)
                            goto failed;
                    }
                }

                /* Create application keys (if needed) */
                foreach (KeyValuePair<byte, byte[]> keyEntry in applicationEntry.Value.Keys)
                {
                    parameters = new Parameters();
                    parameters.KeyId = keyEntry.Key;
                    parameters.KeyValue = keyEntry.Value;
                    
                    switch (applicationEntry.Value.KeyType)
                    {
                        case DesfireApplication.KeyTypeEnum.KEY_TYPE_AES:
                            parameters.KeyVersion = 0x00;
                            parameters.OldKeyValue = new byte[16];
                            result = ChangeKeyAES(desfire, parameters);
                            break;
                        case DesfireApplication.KeyTypeEnum.KEY_TYPE_3DES:
                            parameters.OldKeyValue = new byte[16];
                            result = ChangeKey(desfire, parameters);
                            break;
                        case DesfireApplication.KeyTypeEnum.KEY_TYPE_3DES3K:                            
                            parameters.OldKeyValue = new byte[24];
                            result = ChangeKey(desfire, parameters);
                            break;
                        default:
                            result = Result.InvalidArgument;
                            break;
                    }

                    if (result != Result.Success)
                        goto failed;
                }
            }

            return Result.Success;

            failed:
            return result;
        }
    }
}
