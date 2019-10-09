/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 12/04/2012
 * Time: 15:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardLibraries.Desfire;
using System;

namespace NFCToolDesfire
{
    class Program
    {

        const int KEY_DES_OR_3DES = 1;
        const int KEY_DES_OR_3DES_ISO = 2;
        const int KEY_AES_ISO = 3;
        const byte DF_ISO_WRAPPING_OFF = 0;
        const byte DF_ISO_WRAPPING_CARD = 1;
        const byte DF_ISO_WRAPPING_READER = 2;


        const byte DF_APPLSETTING1_MASTER_CHANGEABLE = 0x01;
        const byte DF_APPLSETTING1_FREE_LISTING = 0x02;
        const byte DF_APPLSETTING1_FREE_CREATE_DELETE = 0x04;
        const byte DF_APPLSETTING1_CONFIG_CHANGEABLE = 0x08;
        const byte DF_APPLSETTING1_SAME_KEY_NEEDED = 0xE0;
        const byte DF_APPLSETTING1_ALL_KEYS_FROZEN = 0xF0;

        const byte DF_APPLSETTING2_ISO_EF_IDS = 0x20;
        const byte DF_APPLSETTING2_DES_OR_3DES2K = 0x00;
        const byte DF_APPLSETTING2_3DES3K = 0x40;
        const byte DF_APPLSETTING2_AES = 0x80;

        const byte NFC_NDEF_VERSION_2 = 0x20;
        const byte NFC_NDEF_DESFIRE_DATA_EF_ID = 0x04;
        const byte NDEF_FILE_CONTROL_TAG = 0x04;
        const byte NFC_NDEF_DESFIRE_CC_EF_ID = 0x03;

        const ushort NFC_NDEF_7816_DATA_EF_ID = 0xE104;
        const ushort NFC_NDEF_7816_CC_EF_ID = 0xE103;
        const ushort NFC_NDEF_7816_DF_ID = 0xE100;
        const UInt32 NFC_NDEF_DESFIRE_DF_ID = 0xEEEE10;

        byte[] NFC_NDEF_7816_AID = new byte[7] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01 };


        public static void Main(string[] args)
        {

            bool p_erase = false;
            bool p_prepare = false;
            bool p_lock = false;
            bool p_batch = false;
            bool p_continue = false;
            string p_key = null;
            int p_key_type = 0;
            string p_set_key = null;
            int p_set_key_type = 0;
            string p_reader = null;
            int p_size = 0;
            bool p_list = false;

            bool nfc_verbose = false;
            bool nfc_silent = false;

            byte[] blank_buffer = new byte[32];

            byte[] key_buffer = new byte[24];
            int key_length = 0;
            byte[] set_key_buffer = new byte[24];
            int set_key_length = 0;

            int rc;
            UInt32 ndef_size;
            UInt32 cc_size = 32;

            Getopt g = new Getopt("NfcTagToolDesfire", args, "epwz:k:t:s:r:bclvqh");

            int c;

            while ((c = g.getopt()) != -1)
            {
                switch (c)
                {
                    case 'e': p_erase = true; break;
                    case 'p': p_prepare = true; break;
                    case 'w': p_lock = true; break;

                    case 'z':
                        p_size = Int32.Parse(g.Optarg);
                        break;

                    case 'k':
                        p_key = g.Optarg;
                        break;

                    case 't':
                        p_key_type = Int32.Parse(g.Optarg);
                        break;

                    case 's':
                        p_set_key = g.Optarg;
                        break;

                    case 'n':
                        p_set_key_type = Int32.Parse(g.Optarg);
                        break;

                    case 'r':
                        p_reader = g.Optarg;
                        break;
                    case 'l': p_list = true; break;

                    case 'b': p_batch = true; break;
                    case 'c': p_continue = true; break;

                    case 'v': nfc_verbose = true; break;
                    case 'q': nfc_silent = true; break;

                    case 'h':
                        hello();
                        usage();
                        return;

                    case '?':
                        hello();
                        helpmsg("Incorrect parameter(s)", nfc_silent);
                        return;

                    default:
                        hello();
                        helpmsg("Invalid command line.", nfc_silent);
                        return;

                }
            }


            if (!nfc_silent)
                hello();

            if (p_list)
            {
                string reader_list = "";
                for (int i = 0; i < SCARD.Readers.Length - 1; i++)
                    reader_list += "Reader " + i + "=" + SCARD.Readers[i] + "  -  ";
                reader_list += "Reader " + (SCARD.Readers.Length - 1) + "=" + SCARD.Readers[SCARD.Readers.Length - 1];
                Console.WriteLine(reader_list);
                return;
            }

            /* Check the minimum mandatory parameters */
            if ((!p_erase) && (!p_prepare) && (!p_lock))
            {
                helpmsg("No mode specified, nothing to do!", nfc_silent);
                return;
            }

            if ((p_erase || p_prepare) && (p_lock))
            {
                helpmsg("Multiple modes specified, choose one!", nfc_silent);
                return;
            }

            /* Retrieve the key(s) */
            if (p_erase || p_prepare)
            {
                if (p_key == null)
                {
                    if (nfc_verbose)
                        Console.WriteLine("Key not specified, using default");

                    key_buffer = new byte[24];
                    for (int i = 0; i < key_buffer.Length; i++)
                        key_buffer[i] = 0;

                    key_length = 8;
                }
                else
                {
                    CardBuffer key = new CardBuffer(p_key);
                    key_buffer = new byte[24];
                    Array.ConstrainedCopy(key.GetBytes(), 0, key_buffer, 0, key.GetBytes().Length);
                    key_length = key.GetBytes().Length;

                }

                if ((key_length != 8) && (key_length != 16) && (key_length != 24))
                {
                    helpmsg("Key: invalid parameter", nfc_silent);
                    return;
                }

                if (key_length == 8)
                {

                    Array.ConstrainedCopy(key_buffer, 0, key_buffer, 8, 8);
                    Array.ConstrainedCopy(key_buffer, 0, key_buffer, 16, 8);
                }
                else
                if (key_length == 16)
                {
                    Array.ConstrainedCopy(key_buffer, 0, key_buffer, 16, 8);
                }

                if (p_key_type == 0)
                {
                    if (key_length == 8)
                    {
                        if (!nfc_silent)
                            Console.WriteLine("Type of key not specified, assuming DES");
                        p_key_type = KEY_DES_OR_3DES;
                    }
                    else
                    if (key_length == 16)
                    {
                        if (!nfc_silent)
                            Console.WriteLine("Type of key not specified, assuming TripleDES");
                        p_key_type = KEY_DES_OR_3DES;
                    }
                    else
                    if (key_length == 24)
                    {
                        if (!nfc_silent)
                            Console.WriteLine("Type of key not specified, assuming TripleDES with 3 keys");
                        p_key_type = KEY_DES_OR_3DES_ISO;
                    }
                    else
                    {
                        helpmsg("You must specify the type of the key!", nfc_silent);
                        return;
                    }
                }


                if (p_set_key != null)
                {
                    CardBuffer set_key = new CardBuffer(p_set_key);
                    set_key_buffer = new byte[24];
                    Array.ConstrainedCopy(set_key.GetBytes(), 0, set_key_buffer, 0, set_key.GetBytes().Length);
                    set_key_length = set_key.GetBytes().Length;


                    if ((set_key_length != 8) && (set_key_length != 16) && (set_key_length != 24))
                    {
                        helpmsg("New key: invalid parameter", nfc_silent);
                        return;
                    }

                    if (set_key_length == 8)
                    {

                        Array.ConstrainedCopy(set_key_buffer, 0, set_key_buffer, 8, 8);
                        Array.ConstrainedCopy(set_key_buffer, 0, set_key_buffer, 16, 8);
                    }
                    else
                    if (set_key_length == 16)
                    {
                        Array.ConstrainedCopy(set_key_buffer, 0, set_key_buffer, 16, 8);
                    }

                    if (p_set_key_type == 0)
                    {
                        if (set_key_length == 8)
                        {
                            if (!nfc_silent)
                                Console.WriteLine("Type of new key not specified, assuming DES");
                            p_set_key_type = KEY_DES_OR_3DES;
                        }
                        else
                        if (set_key_length == 16)
                        {
                            if (!nfc_silent)
                                Console.WriteLine("Type of new key not specified, assuming TripleDES");
                            p_set_key_type = KEY_DES_OR_3DES;
                        }
                        else
                        if (set_key_length == 24)
                        {
                            if (!nfc_silent)
                                Console.WriteLine("Type of new key not specified, assuming TripleDES with 3 keys");
                            p_set_key_type = KEY_DES_OR_3DES_ISO;
                        }
                        else
                        {
                            helpmsg("You must specify the type of the new key!", nfc_silent);
                            return;
                        }
                    }
                }

            }

            if (p_reader == null)
            {
                helpmsg("No Reader selected!", nfc_silent);
                return;
            }


            string ReaderName;

            /* Verify the reader */
            try
            {
                ReaderName = SCARD.Readers[Int32.Parse(p_reader)];
            }
            catch
            {
                Console.WriteLine("Reader not found!");
                return;
            }
            if (p_reader == null)
            {
                Console.WriteLine("Reader not found!");
                return;
            }

            if (!nfc_silent)
                Console.WriteLine("Looking for Desfire EV1 card on reader " + ReaderName);


            batch_begin:

            /* Connect to the card */
            SCardChannel scard = new SCardChannel(ReaderName);

            if (scard == null)
            {
                Console.WriteLine("Failed to connect to the target card.");
                if (p_batch && p_continue)
                    goto batch_begin;
                goto failed;
            }

            if (!scard.Connect())
            {
                Console.WriteLine("Failed to connect to the target card.");
                if (p_batch && p_continue)
                    goto batch_begin;
                goto failed;
            }

            /* Open the Desfire DLL */
            rc = SCARD_DESFIRE.AttachLibrary(scard.hCard);
            if (rc != SCARD.S_SUCCESS)
            {
                Console.WriteLine("Failed to instantiate the PC/SC Desfire DLL.");
                goto failed;
            }

            rc = SCARD_DESFIRE.IsoWrapping(scard.hCard, DF_ISO_WRAPPING_CARD);
            if (rc != SCARD.S_SUCCESS)
            {
                Console.WriteLine("Failed to select the ISO 7816 wrapping mode.");
                goto failed;
            }

            byte[] version_info = new byte[30];

            rc = SCARD_DESFIRE.GetVersion(scard.hCard, version_info);
            if (rc != SCARD.S_SUCCESS)
            {
                Console.WriteLine("Desfire 'get version' command failed.");
                if (p_batch && p_continue)
                    goto batch_end;
                goto failed;
            }

            if (!nfc_silent)
                Console.WriteLine("Found a Desfire card\n");

            if (nfc_verbose)
            {
                Console.WriteLine("Hardware: Vendor=" + version_info[0]
                                  + ", Type=" + version_info[1]
                                  + ", SubType=" + version_info[2]
                                  + ", v" + version_info[3] + "." + version_info[4]);

                Console.WriteLine("Software: Vendor=" + version_info[7]
                                  + ", Type=" + version_info[8]
                                  + ", SubType=" + version_info[9]
                                  + ", v" + version_info[10] + "." + version_info[11]);
            }


            if ((version_info[0] != 0x04) || (version_info[7] != 0x04))
            {
                Console.WriteLine("Manufacturer is not NXP\n");
                if (p_batch && p_continue)
                    goto batch_end;
                goto failed;
            }

            if ((version_info[1] != 0x01) || (version_info[8] != 0x01))
            {
                Console.WriteLine("Type is not Desfire\n");
                if (p_batch && p_continue)
                    goto batch_end;
                goto failed;
            }

            if (version_info[10] < 1)
            {
                Console.WriteLine("Software version is below EV1\n");
                if (p_batch && p_continue)
                    goto batch_end;
                goto failed;
            }

            if (p_erase || p_prepare)
            {
                if (nfc_verbose)
                    Console.WriteLine("Authenticating...");

                switch (p_key_type)
                {
                    case KEY_DES_OR_3DES: rc = SCARD_DESFIRE.Authenticate(scard.hCard, 0, key_buffer); break;
                    case KEY_DES_OR_3DES_ISO: rc = SCARD_DESFIRE.AuthenticateIso24(scard.hCard, 0, key_buffer); break;
                    case KEY_AES_ISO: rc = SCARD_DESFIRE.AuthenticateAes(scard.hCard, 0, key_buffer); break;
                    default: rc = -1; break;
                }
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Authentication failed");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (set_key_length > 0)
                {
                    if (nfc_verbose)
                        Console.WriteLine("Setting the new key...");

                    if ((p_key_type == KEY_DES_OR_3DES) && (p_set_key_type == KEY_DES_OR_3DES))
                    {
                        rc = SCARD_DESFIRE.ChangeKey(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_DES_OR_3DES) && (p_set_key_type == KEY_DES_OR_3DES_ISO))
                    {
                        rc = SCARD_DESFIRE.AuthenticateIso24(scard.hCard, 0, key_buffer);
                        if (rc == SCARD.S_SUCCESS)
                            rc = SCARD_DESFIRE.ChangeKey24(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_DES_OR_3DES) && (p_set_key_type == KEY_AES_ISO))
                    {
                        rc = SCARD_DESFIRE.ChangeKeyAes(scard.hCard, DF_APPLSETTING2_AES | 0, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_DES_OR_3DES_ISO) && (p_set_key_type == KEY_DES_OR_3DES))
                    {
                        rc = SCARD_DESFIRE.ChangeKeyAes(scard.hCard, DF_APPLSETTING2_AES | 0, 0, blank_buffer, null);
                        if (rc == SCARD.S_SUCCESS)
                            rc = SCARD_DESFIRE.AuthenticateAes(scard.hCard, 0, blank_buffer);
                        if (rc == SCARD.S_SUCCESS)
                            rc = SCARD_DESFIRE.ChangeKey(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_DES_OR_3DES_ISO) && (p_set_key_type == KEY_DES_OR_3DES_ISO))
                    {
                        rc = SCARD_DESFIRE.ChangeKey24(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_DES_OR_3DES_ISO) && (p_set_key_type == KEY_AES_ISO))
                    {
                        rc = SCARD_DESFIRE.ChangeKeyAes(scard.hCard, DF_APPLSETTING2_AES | 0, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_AES_ISO) && (p_set_key_type == KEY_DES_OR_3DES))
                    {
                        rc = SCARD_DESFIRE.ChangeKey(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_AES_ISO) && (p_set_key_type == KEY_DES_OR_3DES_ISO))
                    {
                        rc = SCARD_DESFIRE.ChangeKey24(scard.hCard, 0, set_key_buffer, null);
                    }
                    else
                    if ((p_key_type == KEY_AES_ISO) && (p_set_key_type == KEY_AES_ISO))
                    {
                        rc = SCARD_DESFIRE.ChangeKeyAes(scard.hCard, DF_APPLSETTING2_AES | 0, 0, set_key_buffer, null);
                    }

                    if (rc != SCARD.S_SUCCESS)
                    {
                        Console.WriteLine("Change key failed");
                        if (!nfc_silent)
                            Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                        if (p_batch && p_continue)
                            goto batch_end;
                        goto failed;
                    }

                    switch (p_set_key_type)
                    {
                        case KEY_DES_OR_3DES: rc = SCARD_DESFIRE.Authenticate(scard.hCard, 0, set_key_buffer); break;
                        case KEY_DES_OR_3DES_ISO: rc = SCARD_DESFIRE.AuthenticateIso24(scard.hCard, 0, set_key_buffer); break;
                        case KEY_AES_ISO: rc = SCARD_DESFIRE.AuthenticateAes(scard.hCard, 0, set_key_buffer); break;
                        default: rc = -1; break;
                    }
                    if (rc != SCARD.S_SUCCESS)
                    {
                        Console.WriteLine("Authentication with new key failed");
                        if (!nfc_silent)
                            Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                        if (p_batch && p_continue)
                            goto batch_end;
                        goto failed;
                    }

                }
            }


            if (p_erase)
            {
                if (nfc_verbose)
                    Console.WriteLine("Formating the card...");

                rc = SCARD_DESFIRE.FormatPICC(scard.hCard);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Format PICC failed\n");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (p_set_key != null)
                {
                    if (nfc_verbose)
                        Console.WriteLine("Changing the key...");

                }

                if (!nfc_silent)
                    Console.WriteLine("Card erase done...");
            }


            if (p_prepare)
            {
                byte[] nfc_aid = new byte[7];
                byte[] NFC_NDEF_7816_AID = new byte[7] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01 };
                nfc_aid = NFC_NDEF_7816_AID;
                byte[] cc_buffer = new byte[15];

                if (nfc_verbose)
                    Console.WriteLine("Creating the NFC NDEF application...");

                rc = SCARD_DESFIRE.CreateIsoApplication(scard.hCard,
                                                       NFC_NDEF_DESFIRE_DF_ID,
                                                                                           0xFF,
                                                                                           DF_APPLSETTING2_ISO_EF_IDS | 0,
                                                                                             NFC_NDEF_7816_DF_ID,
                                                                                             nfc_aid,
                                                                                             (byte)nfc_aid.Length);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Create application failed");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));

                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;

                }

                if (nfc_verbose)
                    Console.WriteLine("Entering the NFC NDEF directory...");

                rc = SCARD_DESFIRE.SelectApplication(scard.hCard, NFC_NDEF_DESFIRE_DF_ID);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Select application failed");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (nfc_verbose)
                    Console.WriteLine("Creating the NFC CC file...");

                rc = SCARD_DESFIRE.CreateIsoStdDataFile(scard.hCard,
                                                       NFC_NDEF_DESFIRE_CC_EF_ID,
                                                                                             NFC_NDEF_7816_CC_EF_ID,
                                                                                           0,
                                                                                         0xEEEE,
                                                                                           cc_size);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Create CC file failed");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));

                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;

                }

                ndef_size = 0;
                rc = SCARD_DESFIRE.GetFreeMemory(scard.hCard, ref ndef_size);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Get free memory failed");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (p_size != 0)
                {
                    if (ndef_size < p_size)
                    {
                        Console.WriteLine("NDEF size=" + p_size + ", bigger than available space on card (" + ndef_size + ")\n", p_size, ndef_size);
                        if (!nfc_silent)
                            Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                        if (p_batch && p_continue)
                            goto batch_end;
                        goto failed;
                    }
                    ndef_size = (UInt32)p_size;
                }

                if (nfc_verbose)
                    Console.WriteLine("Creating the NFC NDEF file...");

                rc = SCARD_DESFIRE.CreateIsoStdDataFile(scard.hCard,
                                                       NFC_NDEF_DESFIRE_DATA_EF_ID,
                                                                                           NFC_NDEF_7816_DATA_EF_ID,
                                                                                          0,
                                                                                           0xEEEE,
                                                                                           ndef_size);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Create NDEF file failed: " + NFC_NDEF_DESFIRE_DATA_EF_ID + "-"
                                      + NFC_NDEF_7816_DATA_EF_ID + "-" + ndef_size);
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (nfc_verbose)
                    Console.WriteLine("Populating the NFC CC file...\n");

                cc_buffer[0] = 0x00;
                cc_buffer[1] = 0x0F;
                cc_buffer[2] = NFC_NDEF_VERSION_2;
                cc_buffer[3] = 0x00;
                cc_buffer[4] = 0x3B;
                cc_buffer[5] = 0x00;
                cc_buffer[6] = 0x34;
                cc_buffer[7] = NDEF_FILE_CONTROL_TAG;
                cc_buffer[8] = 0x06;
                cc_buffer[9] = NFC_NDEF_7816_DATA_EF_ID >> 8;
                cc_buffer[10] = NFC_NDEF_7816_DATA_EF_ID & 0x00FF;
                cc_buffer[11] = (byte)(ndef_size >> 8);
                cc_buffer[12] = (byte)(ndef_size & 0x00FF);
                cc_buffer[13] = 0x00;
                cc_buffer[14] = 0x00;

                rc = SCARD_DESFIRE.WriteData2(scard.hCard,
                                             NFC_NDEF_DESFIRE_CC_EF_ID,
                                             0,
                                             (UInt32)cc_buffer.Length,
                                             cc_buffer);
                if (rc != SCARD.S_SUCCESS)
                {
                    Console.WriteLine("Populate CC file failed\n");
                    if (!nfc_silent)
                        Console.WriteLine(SCARD_DESFIRE.GetErrorMessage(rc));
                    if (p_batch && p_continue)
                        goto batch_end;
                    goto failed;
                }

                if (!nfc_silent)
                    Console.WriteLine("Card prepare done...\n");
            }


        batch_end:
            SCARD_DESFIRE.DetachLibrary(scard.hCard);
            scard.Disconnect();
            return;

        /* Error */
        failed:
            if (scard.hCard != null)
                SCARD_DESFIRE.DetachLibrary(scard.hCard);
            scard.Disconnect();
            return;

        }


        public static void usage()
        {
            Console.WriteLine("");
            Console.WriteLine("Prepare the Desfire EV1 card to hold a SmartPoster:\n");
            Console.WriteLine("-p [OPTIONS]");
            Console.WriteLine("OPTIONS are:");
            Console.WriteLine(" -z <Size>");
            Console.WriteLine("\tSize of the NDEF file to be created");
            Console.WriteLine("\t(default is to take all the free space in the card)");
            Console.WriteLine(" -k <Key>");
            Console.WriteLine("\tCurrent value for root key (default is blank key)");
            Console.WriteLine(" -t <Type>");
            Console.WriteLine("\nCurrent type for root key (default is DES)");
            Console.WriteLine(" -s  <KeyToSet>");
            Console.WriteLine("\tNew value for root key (default is unchanged)");
            Console.WriteLine(" -n <NewKeyTypeToSet>");
            Console.WriteLine("\tNew type for root key (default is unchanged)");
            Console.WriteLine("");
            Console.WriteLine("Lock the Desfire EV1 SmartPoster:\n");
            Console.WriteLine(" -l [OPTIONS]");
            Console.WriteLine("");
            Console.WriteLine("Erase the Desfire EV1 card:\n");
            Console.WriteLine(" -e [OPTIONS]");
            Console.WriteLine("OPTIONS are:");
            Console.WriteLine(" -k <Key>");
            Console.WriteLine("\tCurrent value for root key (default is blank key)");
            Console.WriteLine(" -t <Type>");
            Console.WriteLine("\nCurrent type for root key (default is DES)");
            Console.WriteLine(" -s  <KeyToSet>");
            Console.WriteLine("\tNew value for root key (default is unchanged)");
            Console.WriteLine(" -n <NewKeyTypeToSet>");
            Console.WriteLine("\tNew type for root key (default is unchanged)");
            Console.WriteLine("");
            Console.WriteLine("Common options:\n");
            Console.WriteLine(" -v");
            Console.WriteLine("\tGive details on what's going on.");
            Console.WriteLine(" -q");
            Console.WriteLine("\tShut up unless fatal errors occur.\n");
            Console.WriteLine(" -h");
            Console.WriteLine("\tDisplay this help screen, then exits.\n");
            Console.WriteLine("PC/SC readers:\n");
            Console.WriteLine(" -l");
            Console.WriteLine("\tPrint the list of available readers, then exits\n");
            Console.WriteLine(" -r <Reader>");
            Console.WriteLine("\tWork with the specified Reader. It could be either the");
            Console.WriteLine("\tcomplete name of the Reader, or its index in the list.\n");
        }

        public static void hello()
        {
            Console.WriteLine("NfcTagToolDesfire: Format Desfire EV1 cards to hold NFC Forum compliant Tags (SmartPoster)");
            Console.WriteLine("");
            Console.WriteLine("This software is copyright (c) 2000-2011 PRO ACTIVE SAS.");
            Console.WriteLine("All rights reserved.");
            Console.WriteLine("Visit www.springcard.com/solutions/nfc for information and updates");
            Console.WriteLine("");
            Console.WriteLine("THIS SOFTWARE IS PROVIDED \"AS IS\" WITH NO EXPRESSED OR IMPLIED WARRANTY.");
            Console.WriteLine("You're using it at your own risks.");
            Console.WriteLine("");
        }

        public static void helpmsg(string s, bool nfc_silent)
        {
            if (!nfc_silent)
            {
                if (!s.Equals(""))
                    Console.WriteLine(s);

                Console.WriteLine("Call NfcTagToolDesfire.exe -h for Help");
            }
        }

    }

}