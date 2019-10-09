using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;

namespace DesfireTool
{
    partial class Program
    {
        Result CreateApplication(Desfire desfire, Parameters parameters)
        {
            //static byte[] KeySettings = { 0x0F, 0x81 };

            if (parameters.ApplicationId < 0)
            {
                ConsoleError("CreateApplication: Application ID must be specified");
                ConsoleWriteLine("Use parameter --application-id|-i");
                return Result.MissingArgument;
            }
            if (parameters.ApplicationId == 0)
            {
                ConsoleError("CreateApplication: Application ID 000000 is forbidden");
                return Result.InvalidArgument;
            }
            if (parameters.KeySettings == null)
            {
                ConsoleError("CreateApplication: Key settings must be specified");
                ConsoleWriteLine("Use parameter --key-settings");
                return Result.MissingArgument;
            }


            /* key settings 1 */
            if ((parameters.KeySettings[0] & 0xF0) != 0x00)
            {
                ConsoleError("CreateApplication: Access rights for changing application keys are not currently managed by this application");
                return Result.OptionNotImplemented;
            }

            if ((parameters.KeySettings[0] & 0x08) == 0x00)
            {
                if (!ForceAnswerYes)
                {
                    if (!AskForUserConfirmation("The configuration will not be changeable anymore (frozen), are you sure (Y/N)?"))
                    {
                        ConsoleError("Operation cancelled by the user!");
                        return Result.Cancelled;
                    }
                }
            }
            ConsoleWriteLine("Configuration is changeable if authenticated with the application master key.");

            if ((parameters.KeySettings[0] & 0x04) == 0x00)
            {
                ConsoleWriteLine("Application master key authentification is required before CreateFile/DeleteFile operations.");
            }
            else
            {
                ConsoleWriteLine("You are free to use CreateFile/Delete file operations.");
            }

            if ((parameters.KeySettings[0] & 0x02) == 0x00)
            {
                ConsoleWriteLine("Application master key authentification is required before GetFileIDs/GetISOFileIDs/GetFileSEttings and GetKEYSettings operations.");
            }
            else
            {
                ConsoleWriteLine("You are free to use GetFileIDs/GetISOFileIDs/GetFileSEttings and GetKEYSettings commands.");
            }

            if ((parameters.KeySettings[0] & 0x01) == 0x00)
            {
                if (!ForceAnswerYes)
                {
                    if (!AskForUserConfirmation("Application master key will not be changeable anymore (frozen), are you sure (Y/N)?"))
                    {
                        ConsoleError("Operation cancelled by the user!");
                        return Result.Cancelled;
                    }
                }
            }
            ConsoleWriteLine("Application master key is changeable after application master key authentification.");

            /* key settings 2 */
            if ((parameters.KeySettings[1] & 0x20) != 0)
            {
                ConsoleError("ISO/IEC 7816-4 File are not currently managed by this application");
                return Result.OptionNotImplemented;
            }

            if ((parameters.KeySettings[1] & 0x10) != 0)
            {
                ConsoleError("Bit 4 is RFU and shall not be used!");
                return Result.InvalidArgument;
            }

            if ((parameters.KeySettings[1] & 0x0F) > 14)
            {
                ConsoleError("A maximum of 14 keys could be stored in a Desfire application!");
                return Result.InvalidArgument;
            }

            switch (parameters.KeySettings[1] >> 6)
            {
                case 0:
                    ConsoleWriteLine("Using (3)DES operations");
                    break;
                case 1:
                    ConsoleWriteLine("Using 3K3DES operations");
                    break;
                case 2:
                    ConsoleWriteLine("Using AES operations");
                    break;
                default:
                    ConsoleWriteLine("Using RFU operations");
                    break;
            }
            Console.WriteLine("Maximum number of keys: " + (parameters.KeySettings[1] & 0x0F));

            long rc = desfire.CreateApplication((uint)parameters.ApplicationId, parameters.KeySettings[0], parameters.KeySettings[1]);
            if (rc != SCARD.S_SUCCESS)
                return OnError("CreateApplication", rc);

            return Result.Success;
        }

        Result SelectApplication(Desfire desfire, Parameters parameters)
        {
            if (parameters.ApplicationId < 0)
            {
                ConsoleError("SelectApplication: ApplicationID must be specified");
                ConsoleWriteLine("Use parameter --application-id|-a");
            }

            long rc = desfire.SelectApplication((uint)parameters.ApplicationId);
            if (rc != SCARD.S_SUCCESS)
                return OnError("SelectApplication", rc);

            return Result.Success;
        }

        Result GetApplicationIDs(Desfire desfire, Parameters parameters)
        {
            uint[] ApplicationIDs = new uint[19];
            byte ApplicationIDCounter = 0;

            /* retreive Application IDs */
            long rc = desfire.GetApplicationIDs(19, ref ApplicationIDs, ref ApplicationIDCounter);
            if (rc != SCARD.S_SUCCESS)
                return OnError("GetApplicationIDs", rc);

            if (ApplicationIDCounter > 0)
            {
                for (byte cptid = 0; cptid < ApplicationIDCounter; cptid++)
                {
                    Console.WriteLine("Defire 'Application ID' " + BinConvert.tohex((byte)(ApplicationIDs[cptid] & 0xFF)).ToUpper() +
                            BinConvert.tohex((byte)((ApplicationIDs[cptid] >> 8) & 0xFF)).ToUpper() +
                            BinConvert.tohex((byte)((ApplicationIDs[cptid] >> 16) & 0xFF)).ToUpper() + " found.");
                }
            }
            else
            {
                ConsoleError("No Desfire application found.");
            }

            return Result.Success;
        }
    }
}
