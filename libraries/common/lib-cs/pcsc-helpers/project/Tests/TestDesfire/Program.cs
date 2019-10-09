/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 07/09/2017
 * Time: 16:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace SpringCard.PCSC.SelfTest
{
  class Program
  {
    public static void Usage()
    {
      Console.WriteLine("Desfire.exe [options], where options are:");
      Console.WriteLine("\t -l: list available PC/SC couplers");
      Console.WriteLine("\t -r X: perform actions on PC/SC coupler X");
        
    }
    
    public static void Main(string[] args)
    {
      bool got_something = false;
      
      
      
      /* -------- 
      byte[] text = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
      //byte[] key = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
      byte[] key= {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
      byte[] iv = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
      byte[] res = Crypto.TripleDES_Decrypt(text, key, iv);
      foreach (byte b in res)
        Console.Write(String.Format("{0:x02}", b));
      Console.Write("\n");
      
      
      return;
      
      -------- */
      
      
      for(int i=0; i<args.Length; i++)
      {
        if (args[i].Equals("-l"))
        {
          got_something = true;
          for (int j=0; j< SCARD.Readers.Length; j++)
            Console.WriteLine("Reader 0: " + SCARD.Readers[j]);
        }
        if ( args[i].Equals("-r") && ((i+1) < args.Length )  )
        {
          int index_reader;
          got_something = true;
          
          if (! Int32.TryParse(args[i+1], out index_reader))
          {
            Console.WriteLine("Invalid argument");
            Usage();
          } else
          {
            if (index_reader < SCARD.Readers.Length)
            {
              DESFireTest(SCARD.Readers[index_reader]);
            } else
            {
              Console.WriteLine("Specified PC/SC coupler doesn't exist");
            }
            
          }
        }
      }
      
      if (!got_something)
        Usage();
    }
    
    public static void DESFireTest(string readerName)
    {
      SCardReader cardReader = new SCardReader(readerName);
      SCardChannel  cardChannel = new SCardChannel(readerName);
      if (!cardChannel.Connect())
      {
        Console.WriteLine("Can't connect to target Card");
        goto fail;
      }
      Desfire desfire = new Desfire(cardChannel);
      desfire.IsoWrapping(Desfire.DF_ISO_WRAPPING_CARD);

      /* --- 
      byte[] virgin16_key = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      long rc = desfire.Authenticate(0, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate ok");
      } else
      {
        Console.WriteLine("Authenticate ko - rc = " + rc);
      }
       --- */
      
      /*
      byte[] virgin16_key = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      long rc = desfire.AuthenticateIso(0, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }

      Console.WriteLine("-------------- FORMAT -------------");
      rc = desfire.FormatPICC();
      if (rc == 0)
      {
        Console.WriteLine("FormatPICC ok");
      } else
      {
        Console.WriteLine("FormatPICC ko - rc = " + rc);
      }      
      */
     
      // Master_test(desfire);
      
      DES_test(desfire);
      
      
      // ISO_DES_test(desfire);
      
      // AES_test(desfire);
     
      
fail :
        if (cardChannel.Connected)
          cardChannel.DisconnectEject();
        
    }

    public static void Master_test(Desfire desfire)
    {
      byte[] virgin16_key = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      
      Console.WriteLine("-------------- AUTHENTICATE ISO -------------");
      long rc = desfire.AuthenticateIso(0, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- CHANGE KEY AES -------------");
      byte[] aes_key = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
      rc = desfire.ChangeKeyAes(0x80, 0, aes_key, null);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeyAes ok");
      } else
      {
        Console.WriteLine("ChangeKeyAes ko - rc = " + rc);
      }     
      
      
      Console.WriteLine("-------------- AUTHENTICATE AES -------------");
      rc = desfire.AuthenticateAes(0, aes_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- CREATE APPLICATION 1-------------");
      rc = desfire.CreateApplication(1, 0x0f, 0x82);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- CREATE APPLICATION 2 -------------");
      rc = desfire.CreateApplication(2, 0x0f, 0x82);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }
           
      Console.WriteLine("-------------- GET APPLICATIONS IDs -------------");    
      uint[] aid_list = new uint[50];
      byte aid_count = 0;
      rc = desfire.GetApplicationIDs(10, ref aid_list, ref aid_count);
      if (rc == 0)
      {
        Console.Write("GetApplicationIDs ok: ");
        for (int k=0; k<aid_count; k++)
          Console.Write("" + aid_list[k] + "-");
        Console.Write("\n");
      } else
      {
        Console.WriteLine("GetApplicationIDs ko - rc = " + rc);
      }
      
      Console.WriteLine("--------------DELETE APPLI -------------");    
      rc = desfire.DeleteApplication(1);
      if (rc == 0)
      {
        Console.Write("DeleteApplication ok\n");

      } else
      {
        Console.WriteLine("DeleteApplication ko - rc = " + rc);
      }
      
      /*
      Console.WriteLine("-------------- GET APPLICATIONS IDs -------------");
      aid_list = new uint[50];
      aid_count = 0;
      rc = desfire.GetApplicationIDs(10, ref aid_list, ref aid_count);
      if (rc == 0)
      {
        Console.Write("GetApplicationIDs ok: ");
        for (int k=0; k<aid_count; k++)
          Console.Write("" + aid_list[k]);
        Console.Write("\n");
      } else
      {
        Console.WriteLine("GetApplicationIDs ko - rc = " + rc);
      }
      */
      Console.WriteLine("-------------- FORMAT -------------");
      rc = desfire.FormatPICC();
      if (rc == 0)
      {
        Console.WriteLine("FormatPICC ok");
      } else
      {
        Console.WriteLine("FormatPICC ko - rc = " + rc);
      }      
      
      /*
      Console.WriteLine("-------------- GET APPLICATIONS IDs -------------");
      aid_list = new uint[50];
      aid_count = 0;
      rc = desfire.GetApplicationIDs(10, ref aid_list, ref aid_count);
      if (rc == 0)
      {
        Console.Write("GetApplicationIDs ok: ");
        for (int k=0; k<aid_count; k++)
          Console.Write("" + aid_list[k]);
        Console.Write("\n");
      } else
      {
        Console.WriteLine("GetApplicationIDs ko - rc = " + rc);
      }      
      */
 
      Console.WriteLine("-------------- AUTHENTICATE AES -------------");
      rc = desfire.AuthenticateAes(0, aes_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }
      
      
      Console.WriteLine("------------ CHANGE BACK TO DES -----------");
      rc = desfire.ChangeKey(0, virgin16_key, null);
      if (rc != SCARD.S_SUCCESS)
      {
        Console.WriteLine("Desfire 'Authenticate' command failed - erreur=" + rc);
      } else
      {
        Console.WriteLine("Desfire 'Authenticate' command ok.");  
      }    
      

      Console.WriteLine("------------ AUTHENTICATE ISO -----------");
      rc = desfire.AuthenticateIso(0, virgin16_key);
      if (rc != SCARD.S_SUCCESS)
      {
        Console.WriteLine("Desfire 'AuthenticateIso' command failed - erreur=" + rc);
      } else
      {
        Console.WriteLine("Desfire 'AuthenticateIso' command ok.");  
      }   
      
    }
    
    public static void DES_test(Desfire desfire)
    {
      byte keyVersion = 0;
      long rc = desfire.GetKeyVersion(0, ref keyVersion);
      if (rc != 0)
      {
        Console.WriteLine("GetKeyVersion ko - rc = " + rc);
        return;
      }
      Console.WriteLine("Key version = " + String.Format("{0:x02}", keyVersion));
      
      byte[] virgin_key           = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
      if (keyVersion == 0xa2)
      {
        //byte[] aes_card_master_key  = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
        byte[] aes_card_master_key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };        
        Console.WriteLine("-------------- AUTHENTICATE AES -------------");
        rc = desfire.AuthenticateAes(0, aes_card_master_key);
        if (rc == 0)
        {
          Console.WriteLine("AuthenticateAes ok");
        } else
        {
          Console.WriteLine("AuthenticateAes ko - rc = " + rc);
        }
        
        
        Console.WriteLine("------------ CHANGE BACK TO DES -----------");
        rc = desfire.ChangeKey(0, virgin_key, null);
        if (rc != SCARD.S_SUCCESS)
        {
          Console.WriteLine("Desfire 'ChangeKey' command failed - erreur=" + rc);
        } else
        {
          Console.WriteLine("Desfire 'ChangeKey' command ok.");  
        }    
        
      }
      /*
      Console.WriteLine("-------------- AUTHENTICATE ISO -------------");
      rc = desfire.AuthenticateIso(0, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }      
      */
      Console.WriteLine("-------------- AUTHENTICATE LEGACY DES -------------");
      rc = desfire.Authenticate(0, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate ok");
      } else
      {
        Console.WriteLine("Authenticate ko - rc = " + rc);
      }          
      
      Console.WriteLine("-------------- FORMAT -------------");
      rc = desfire.FormatPICC();
      if (rc == 0)
      {
        Console.WriteLine("FormatPICC ok");
      } else
      {
        Console.WriteLine("FormatPICC ko - rc = " + rc);
      }   
      
      Console.WriteLine("-------------- CREATE APPLICATION DES -------------");
      rc = desfire.CreateApplication(1, 0x0f, 0x02);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }   
      
      Console.WriteLine("-------------- SELECT APPLICATION DES -------------");
      rc = desfire.SelectApplication(1);
      if (rc == 0)
      {
        Console.WriteLine("SelectApplication ok");
      } else
      {
        Console.WriteLine("SelectApplication ko - rc = " + rc);
      }         
      
      Console.WriteLine("-------------- AUTHENTICATE DES (0) -------------");      
      rc = desfire.Authenticate(0, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate ok");
      } else
      {
        Console.WriteLine("Authenticate ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- AUTHENTICATE DES (0) -------------");      
      rc = desfire.Authenticate(0, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate ok");
      } else
      {
        Console.WriteLine("Authenticate ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- CHANGE KEY 1 -------------");
      byte[] des_key0 = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
      byte[] des_key1 = { 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
      rc = desfire.ChangeKey(1, des_key1, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey (1) ok");
      } else
      {
        Console.WriteLine("ChangeKey (1) ko - rc = " + rc);
      }  
      
      
      Console.WriteLine("-------------- AUTHENTICATE DES (1) -------------");      
      rc = desfire.Authenticate(1, des_key1);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate (1)  ok");
      } else
      {
        Console.WriteLine("Authenticate (1) ko - rc = " + rc);
      }        

       
      Console.WriteLine("-------------- AUTHENTICATE DES (1) -------------");      
      rc = desfire.Authenticate(1, des_key1);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate (1)  ok");
      } else
      {
        Console.WriteLine("Authenticate (1) ko - rc = " + rc);
      }    
      
     
      Console.WriteLine("-------------- AUTHENTICATE DES (0) -------------");      
      rc = desfire.Authenticate(0, virgin_key);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate (0) ok");
      } else
      {
        Console.WriteLine("Authenticate (0) ko - rc = " + rc);
      }  
      
      
      Console.WriteLine("-------------- CHANGE KEY 0 -------------");
      rc = desfire.ChangeKey(0, des_key0, null);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey (0) ok");
      } else
      {
        Console.WriteLine("ChangeKey (0) ko - rc = " + rc);
      }        
      
      Console.WriteLine("-------------- AUTHENTICATE DES (0) -------------");      
      rc = desfire.Authenticate(0, des_key0);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate (0) ok");
      } else
      {
        Console.WriteLine("Authenticate (0) ko - rc = " + rc);
      } 

     
      Console.WriteLine("-------------- CREATE STD DATA FILE 1 -------------");      
      byte comm_mode = 3;
      rc = desfire.CreateStdDataFile(1, comm_mode, 0x1000, 250);
      if (rc == 0)
      {
        Console.WriteLine("CreateStdDataFile (0) ok");
      } else
      {
        Console.WriteLine("CreateStdDataFile (0) ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- WRITE STD DATA FILE 1 -------------");      
      byte[] data = new byte[200];
      for (int k=0; k < data.Length; k++)
        data[k] = (byte) k;
      
      rc = desfire.WriteData(1, comm_mode, 0, (uint) data.Length, data);
      if (rc == 0)
      {
        Console.WriteLine("CreateStdDataFile (0) ok");
      } else
      {
        Console.WriteLine("CreateStdDataFile (0) ko - rc = " + rc);
      }        
      
      Console.WriteLine("-------------- READ STD DATA FILE 1 -------------");      
      byte[] res = new byte[200];
      uint done_count = 0;
      rc = desfire.ReadData(1, comm_mode, 0, (uint) res.Length, ref res, ref done_count);
      if (rc == 0)
      {
        Console.Write("ReadData (0) ok - data=");
        for (int k=0; k<done_count; k++)
          Console.Write(String.Format("{0:x02}", res[k]));
        Console.Write("\n");
      } else
      {
        Console.WriteLine("ReadData (0) ko - rc = " + rc);
      }  

            
      Console.WriteLine("-------------- SELECT APPLICATION DES -------------");
      rc = desfire.SelectApplication(1);
      if (rc == 0)
      {
        Console.WriteLine("SelectApplication ok");
      } else
      {
        Console.WriteLine("SelectApplication ko - rc = " + rc);
      }         
      
      
      Console.WriteLine("-------------- AUTHENTICATE DES (1) -------------");      
      rc = desfire.Authenticate(1, des_key1);
      if (rc == 0)
      {
        Console.WriteLine("Authenticate (1)  ok");
      } else
      {
        Console.WriteLine("Authenticate (1) ko - rc = " + rc);
      }       
      
      Console.WriteLine("-------------- READ STD DATA FILE 1 -------------");      
      for (int k=0; k< res.Length; k++)
           res[k]=0;
      
      done_count = 0;
      rc = desfire.ReadData(1, comm_mode, 0, (uint) res.Length, ref res, ref done_count);
      if (rc == 0)
      {
        Console.Write("ReadData (0) ok - data=");
        for (int k=0; k<done_count; k++)
          Console.Write(String.Format("{0:x02}", res[k]));
        Console.Write("\n");
      } else
      {
        Console.WriteLine("ReadData (0) ko - rc = " + rc);
      }  
      
    }
    
    public static void ISO_DES_test(Desfire desfire)
    {
      long rc;
      byte[] virgin16_key = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      
      Console.WriteLine("-------------- CREATE APPLICATION -------------");
      rc = desfire.CreateApplication(1, 0x0F, 0x02);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }
      
      /*
      Console.WriteLine("-------------- DELETE APPLICATION -------------");
      rc = desfire.DeleteApplication(1);
      if (rc == 0)
      {
        Console.WriteLine("DeleteApplication ok");
      } else
      {
        Console.WriteLine("DeleteApplication ko - rc = " + rc);
      }
 
      Console.WriteLine("-------------- FORMAT -------------");
      rc = desfire.FormatPICC();
      if (rc == 0)
      {
        Console.WriteLine("FormatPICC ok");
      } else
      {
        Console.WriteLine("FormatPICC ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- CREATE APPLICATION -------------");
      rc = desfire.CreateApplication(1, 0x0F, 0x02);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }
      */
     
      Console.WriteLine("-------------- SELECT APPLICATION -------------");
      rc = desfire.SelectApplication(1);
      if (rc == 0)
      {
        Console.WriteLine("SelectApplication ok");
      } else
      {
        Console.WriteLine("SelectApplication ko - rc = " + rc);
      }

      Console.WriteLine("-------------- AUTHENTICATE ON APPLICATION -------------");
      rc = desfire.AuthenticateIso(0, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }

      byte comm_mode = 3;
      Console.WriteLine("-------------- CREATE STD DATA FILE -------------");
      rc = desfire.CreateStdDataFile(1, comm_mode, 0x1000, 20);
      if (rc == 0)
      {
        Console.WriteLine("CreateStdDataFile ok");
      } else
      {
        Console.WriteLine("CreateStdDataFile ko - rc = " + rc);
      }
        
      Console.WriteLine("-------------- WRITE DATA -------------");
      byte[] data = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
      rc = desfire.WriteData2(1, 0, 10, data);
      if (rc == 0)
      {
        Console.WriteLine("WriteData ok");
      } else
      {
        Console.WriteLine("WriteData ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- READ DATA -------------");
      byte[] read = new byte[20];
      UInt32 done_count = (UInt32) read.Length;
      rc = desfire.ReadData2(1, 0, 10, ref read, ref done_count);
      if (rc == 0)
      {
        Console.Write("ReadData ok, read: ");
        for (int k=0; k<done_count; k++)
          Console.Write(String.Format("{0:x02}", read[k]));
        Console.Write("\n");
      } else
      {
        Console.WriteLine("ReadData ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- CREATE RECORD FILE -------------");
      rc = desfire.CreateCyclicRecordFile(2, comm_mode, 0x1000, 10, 5);
      if (rc == 0)
      {
        Console.WriteLine("CreateCyclicRecordFile ok");
      } else
      {
        Console.WriteLine("CreateCyclicRecordFile ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- WRITE RECORD 1  -------------");
      byte[] rec_to_write1 = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa };
      rc = desfire.WriteRecord(2, comm_mode, 0, 10, rec_to_write1);
      if (rc == 0)
      {
        Console.WriteLine("WriteRecord 1 ok");
      } else
      {
        Console.WriteLine("WriteRecord 1 ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- COMMIT TRANSACTIONS -------------\n");
      rc = desfire.CommitTransaction();
      if (rc == 0)
      {
        Console.WriteLine("ReadRecords ok");
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- WRITE RECORD 2  -------------");
      byte[] rec_to_write2 = { 0xaa, 0x99, 0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11 };
      rc = desfire.WriteRecord(2, comm_mode, 0, 10, rec_to_write2);
      if (rc == 0)
      {
        Console.WriteLine("WriteRecord 2 ok");
      } else
      {
        Console.WriteLine("WriteRecord 2 ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- GET FILES SETTINGS  -------------");
      byte file_type;
      byte comm_mode_read;
      ushort acces_rights;
      Desfire.DF_FILE_SETTINGS file_settings;
      rc = desfire.GetFileSettings(2, out file_type, out comm_mode_read, out acces_rights, out file_settings);
      if (rc == 0)
      {
        Console.WriteLine("GetFileSettings ok");
        Console.WriteLine("\tfile_type=" + file_type);
        Console.WriteLine("\tcomm_mode_read=" + comm_mode_read);
        Console.WriteLine("\tacces_rights=0x" + String.Format("{0:x04}", acces_rights));
        if (file_type == Desfire.DF_CYCLIC_RECORD_FILE)
        {
          Console.WriteLine("\t\teRecordSize=" + file_settings.RecordFile.eRecordSize);
          Console.WriteLine("\t\teMaxNRecords=" + file_settings.RecordFile.eMaxNRecords);
          Console.WriteLine("\t\teCurrNRecords=" + file_settings.RecordFile.eCurrNRecords);
        }
        
      } else
      {
        Console.WriteLine("GetFileSettings ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- COMMIT TRANSACTIONS -------------\n");
      rc = desfire.CommitTransaction();
      if (rc == 0)
      {
        Console.WriteLine("ReadRecords ok");
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- GET FILES SETTINGS  -------------");
      rc = desfire.GetFileSettings(2, out file_type, out comm_mode_read, out acces_rights, out file_settings);
      if (rc == 0)
      {
        Console.WriteLine("GetFileSettings ok");
        Console.WriteLine("\tfile_type=" + file_type);
        Console.WriteLine("\tcomm_mode_read=" + comm_mode_read);
        Console.WriteLine("\tacces_rights=0x" + String.Format("{0:x04}", acces_rights));
        if (file_type == Desfire.DF_CYCLIC_RECORD_FILE)
        {
          Console.WriteLine("\t\teRecordSize=" + file_settings.RecordFile.eRecordSize);
          Console.WriteLine("\t\teMaxNRecords=" + file_settings.RecordFile.eMaxNRecords);
          Console.WriteLine("\t\teCurrNRecords=" + file_settings.RecordFile.eCurrNRecords);
        }
        
      } else
      {
        Console.WriteLine("GetFileSettings ko - rc = " + rc);
      }     
      
      Console.WriteLine("-------------- READ RECORDS  -------------");
      byte[] rec_to_read = new byte[50];
      UInt32 record_count = 1;
      rc = desfire.ReadRecords(2, comm_mode, 0, 0, 10, ref rec_to_read, ref record_count);
      if (rc == 0)
      {
        Console.Write("ReadRecords ok, read: \n\t");
        for (int k=0; k<record_count; k++)
        {
          for (int l=0; l<10;l++)
            Console.Write(String.Format("{0:x02}", rec_to_read[k*10+l]));
          Console.Write("\n\t");
        }
        Console.Write("\n");
        
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }
      
      rc = desfire.ChangeFileSettings(2, 3, 0x0000);
      if (rc == 0)
      {
        Console.WriteLine("ChangeFileSettings ok");
      } else
      {
        Console.WriteLine("ChangeFileSettings ko - rc = " + rc);
      }

      /*
      Console.WriteLine("-------------- CHANGE KEY  0 -------------");
      
      byte[] new_key0 = {  1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 } ;
      rc = desfire.ChangeKey(0, new_key0, null);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey ok");
      } else
      {
        Console.WriteLine("ChangeKey ko - rc = " + rc);
      }     
     
      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateIso(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }   
      

      byte[] new_key1 = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
      Console.WriteLine("-------------- CHANGE KEY 1 -------------");
      rc = desfire.ChangeKey(1, new_key1, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey ok");
      } else
      {
        Console.WriteLine("ChangeKey ko - rc = " + rc);
      }   
      
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateIso(1, new_key1);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey ok");
      } else
      {
        Console.WriteLine("ChangeKey ko - rc = " + rc);
      }   

      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateIso(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }   

      byte[] new_new_key1 = { 13, 46, 79, 64, 31, 64, 97, 64, 17, 28, 39, 28, 17, 78, 54, 12 };
      Console.WriteLine("-------------- CHANGE KEY 1 -------------");
      rc = desfire.ChangeKey(1, new_new_key1, new_key1);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey ok");
      } else
      {
        Console.WriteLine("ChangeKey ko - rc = " + rc);
      }   
       Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
     
      rc = desfire.AuthenticateIso(1, new_new_key1);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKey ok");
      } else
      {
        Console.WriteLine("ChangeKey ko - rc = " + rc);
      }  

      */   

     Console.WriteLine("-------------- CHANGE KEY SETTINGS -------------");
      rc = desfire.ChangeKeySettings(0x0f);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeySettings ok");
      } else
      {
        Console.WriteLine("ChangeKeySettings ko - rc = " + rc);
      }
    }
    
    public static void AES_test(Desfire desfire)
    {
      long rc;
      byte[] virgin16_key = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      
      Console.WriteLine("-------------- CREATE APPLICATION -------------");
      rc = desfire.CreateApplication(1, 0x0F, 0x82);
      if (rc == 0)
      {
        Console.WriteLine("CreateApplication ok");
      } else
      {
        Console.WriteLine("CreateApplication ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- SELECT APPLICATION -------------");
      rc = desfire.SelectApplication(1);
      if (rc == 0)
      {
        Console.WriteLine("SelectApplication ok");
      } else
      {
        Console.WriteLine("SelectApplication ko - rc = " + rc);
      }

      Console.WriteLine("-------------- AUTHENTICATE ON APPLICATION -------------");
      rc = desfire.AuthenticateAes(0, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateIso ok");
      } else
      {
        Console.WriteLine("AuthenticateIso ko - rc = " + rc);
      }
      
      
      Console.WriteLine("-------------- CHANGE KEY  0 -------------");
      
      byte[] new_key0 = {  1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 } ;
      rc = desfire.ChangeKeyAes(0, 0, new_key0, null);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeyAes ok");
      } else
      {
        Console.WriteLine("ChangeKeyAes ko - rc = " + rc);
      }     
     
      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateAes(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }   

      byte[] new_key1 = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
      Console.WriteLine("-------------- CHANGE KEY 1 -------------");
      rc = desfire.ChangeKeyAes(1, 0, new_key1, virgin16_key);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeyAes ok");
      } else
      {
        Console.WriteLine("ChangeKeyAes ko - rc = " + rc);
      }   
      
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateAes(1, new_key1);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }   
 
      /*----------------------------------*/
      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateAes(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }   

      byte[] new_new_key1 = { 13, 46, 79, 64, 31, 64, 97, 64, 17, 28, 39, 28, 17, 78, 54, 12 };
      Console.WriteLine("-------------- CHANGE KEY 1 -------------");
      rc = desfire.ChangeKeyAes(1, 1, new_new_key1, new_key1);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeyAes ok");
      } else
      {
        Console.WriteLine("ChangeKeyAes ko - rc = " + rc);
      }   
      
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateAes(1, new_new_key1);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateAes(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      
      byte comm_mode = 3;
      /*
      Console.WriteLine("-------------- CREATE STD DATA FILE -------------");
      rc = desfire.CreateStdDataFile(1, comm_mode, 0x1000, 20);
      if (rc == 0)
      {
        Console.WriteLine("CreateStdDataFile ok");
      } else
      {
        Console.WriteLine("CreateStdDataFile ko - rc = " + rc);
      }
        
      Console.WriteLine("-------------- WRITE DATA -------------");
      byte[] data = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
      rc = desfire.WriteData(1, comm_mode, 0, 10, data);
      if (rc == 0)
      {
        Console.WriteLine("WriteData ok");
      } else
      {
        Console.WriteLine("WriteData ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateAes(1, new_new_key1);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- READ DATA -------------");
      byte[] read = new byte[20];
      UInt32 done_count = (UInt32) read.Length;
      rc = desfire.ReadData(1, comm_mode, 0, 10, ref read, ref done_count);
      if (rc == 0)
      {
        Console.Write("ReadData ok, read: ");
        for (int k=0; k<done_count; k++)
          Console.Write(String.Format("{0:x02}", read[k]));
        Console.Write("\n");
      } else
      {
        Console.WriteLine("ReadData ko - rc = " + rc);
      }
      */
      Console.WriteLine("-------------- CREATE RECORD FILE -------------");
      rc = desfire.CreateCyclicRecordFile(2, comm_mode, 0x1000, 10, 5);
      if (rc == 0)
      {
        Console.WriteLine("CreateCyclicRecordFile ok");
      } else
      {
        Console.WriteLine("CreateCyclicRecordFile ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- WRITE RECORD 1  -------------");
      byte[] rec_to_write1 = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa };
      rc = desfire.WriteRecord(2, comm_mode, 0, 10, rec_to_write1);
      if (rc == 0)
      {
        Console.WriteLine("WriteRecord 1 ok");
      } else
      {
        Console.WriteLine("WriteRecord 1 ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- COMMIT TRANSACTIONS -------------\n");
      rc = desfire.CommitTransaction();
      if (rc == 0)
      {
        Console.WriteLine("ReadRecords ok");
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- WRITE RECORD 2  -------------");
      byte[] rec_to_write2 = { 0xaa, 0x99, 0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11 };
      rc = desfire.WriteRecord(2, comm_mode, 0, 10, rec_to_write2);
      if (rc == 0)
      {
        Console.WriteLine("WriteRecord 2 ok");
      } else
      {
        Console.WriteLine("WriteRecord 2 ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- GET FILES SETTINGS  -------------");
      byte file_type;
      byte comm_mode_read;
      ushort acces_rights;
      Desfire.DF_FILE_SETTINGS file_settings;      
      rc = desfire.GetFileSettings(2, out file_type, out comm_mode_read, out acces_rights, out file_settings);
      if (rc == 0)
      {
        Console.WriteLine("GetFileSettings ok");
        Console.WriteLine("\tfile_type=" + file_type);
        Console.WriteLine("\tcomm_mode_read=" + comm_mode_read);
        Console.WriteLine("\tacces_rights=0x" + String.Format("{0:x04}", acces_rights));
        if (file_type == Desfire.DF_CYCLIC_RECORD_FILE)
        {
          Console.WriteLine("\t\teRecordSize=" + file_settings.RecordFile.eRecordSize);
          Console.WriteLine("\t\teMaxNRecords=" + file_settings.RecordFile.eMaxNRecords);
          Console.WriteLine("\t\teCurrNRecords=" + file_settings.RecordFile.eCurrNRecords);
        }
        
      } else
      {
        Console.WriteLine("GetFileSettings ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- COMMIT TRANSACTIONS -------------\n");
      rc = desfire.CommitTransaction();
      if (rc == 0)
      {
        Console.WriteLine("ReadRecords ok");
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- GET FILES SETTINGS  -------------");
      rc = desfire.GetFileSettings(2, out file_type, out comm_mode_read, out acces_rights, out file_settings);
      if (rc == 0)
      {
        Console.WriteLine("GetFileSettings ok");
        Console.WriteLine("\tfile_type=" + file_type);
        Console.WriteLine("\tcomm_mode_read=" + comm_mode_read);
        Console.WriteLine("\tacces_rights=0x" + String.Format("{0:x04}", acces_rights));
        if (file_type == Desfire.DF_CYCLIC_RECORD_FILE)
        {
          Console.WriteLine("\t\teRecordSize=" + file_settings.RecordFile.eRecordSize);
          Console.WriteLine("\t\teMaxNRecords=" + file_settings.RecordFile.eMaxNRecords);
          Console.WriteLine("\t\teCurrNRecords=" + file_settings.RecordFile.eCurrNRecords);
        }
        
      } else
      {
        Console.WriteLine("GetFileSettings ko - rc = " + rc);
      }     
     
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateAes(1, new_new_key1);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      

      Console.WriteLine("-------------- READ RECORDS  -------------");
      byte[] rec_to_read = new byte[50];
      UInt32 record_count = 1;
      rc = desfire.ReadRecords(2, comm_mode, 0, 0, 10, ref rec_to_read, ref record_count);
      if (rc == 0)
      {
        Console.Write("ReadRecords ok, read: \n\t");
        for (int k=0; k<record_count; k++)
        {
          for (int l=0; l<10;l++)
            Console.Write(String.Format("{0:x02}", rec_to_read[k*10+l]));
          Console.Write("\n\t");
        }
        Console.Write("\n");
        
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }    
     
      Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateAes(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- CHANGE FILE SETTINGS  -------------");
      rc = desfire.ChangeFileSettings(2, 3, 0x0000);
      if (rc == 0)
      {
        Console.WriteLine("ChangeFileSettings ok");
      } else
      {
        Console.WriteLine("ChangeFileSettings ko - rc = " + rc);
      }
      
      Console.WriteLine("-------------- AUTHENTICATE WITH KEY 1 -------------");
      rc = desfire.AuthenticateAes(1, new_new_key1);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  
      
      Console.WriteLine("-------------- READ RECORDS  -------------");
      rec_to_read = new byte[50];
      record_count = 1;
      rc = desfire.ReadRecords(2, comm_mode, 0, 0, 10, ref rec_to_read, ref record_count);
      if (rc == 0)
      {
        Console.Write("ReadRecords ok, read: \n\t");
        for (int k=0; k<record_count; k++)
        {
          for (int l=0; l<10;l++)
            Console.Write(String.Format("{0:x02}", rec_to_read[k*10+l]));
          Console.Write("\n\t");
        }
        Console.Write("\n");
        
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }    
      
            Console.WriteLine("-------------- AUTHENTICATE WITH NEW KEY 0 -------------");
      rc = desfire.AuthenticateAes(0, new_key0);
      if (rc == 0)
      {
        Console.WriteLine("AuthenticateAes ok");
      } else
      {
        Console.WriteLine("AuthenticateAes ko - rc = " + rc);
      }  

      
      Console.WriteLine("-------------- READ RECORDS  -------------");
      rec_to_read = new byte[50];
      record_count = 1;
      rc = desfire.ReadRecords(2, comm_mode, 0, 0, 10, ref rec_to_read, ref record_count);
      if (rc == 0)
      {
        Console.Write("ReadRecords ok, read: \n\t");
        for (int k=0; k<record_count; k++)
        {
          for (int l=0; l<10;l++)
            Console.Write(String.Format("{0:x02}", rec_to_read[k*10+l]));
          Console.Write("\n\t");
        }
        Console.Write("\n");
        
      } else
      {
        Console.WriteLine("ReadRecords ko - rc = " + rc);
      }    


      Console.WriteLine("-------------- CHANGE KEY SETTINGS -------------");
      rc = desfire.ChangeKeySettings(0x0f);
      if (rc == 0)
      {
        Console.WriteLine("ChangeKeySettings ok");
      } else
      {
        Console.WriteLine("ChangeKeySettings ko - rc = " + rc);
      }      
      
    }
    
    /*
    Desfire.AuthenticateIso         - DES ok    - NA
    Desfire.AuthenticateAes         - NA        - AES ok
    Desfire.ChangeKeySettings       - DES ok    - AES ok
    Desfire.ChangeKey               - DES ok    - AES ok
    Desfire.ChangeKeyAes            - DES ok    - AES ok
    Desfire.FormatPICC              - DES ok
    Desfire.CreateApplication       - DES ok
    Desfire.DeleteApplication       - DES ok
    Desfire.SelectApplication       - DES ok
    Desfire.ChangeFileSettings      - DES ok    - AES ok
    Desfire.CreateStdDataFile       - DES ok    - AES ok
    Desfire.CreateCyclicRecordFile  - DES ok    - AES ok
    Desfire.ReadData                - DES ok    - AES ok
    Desfire.WriteData               - DES ok    - AES ok
    Desfire.ReadRecords             - DES ok    - AES ok
    */
  }

}