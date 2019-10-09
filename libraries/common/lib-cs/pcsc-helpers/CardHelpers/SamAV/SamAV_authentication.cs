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

        public bool AuthenticateAV1(AuthTypeE AuthType, byte KeyIdx, byte KeyVersion, byte[] KeyValue)
        {
            string ver = GetSernoString();
            if (ver == null)
                return false;

            if (ver.Equals(""))
                return false;

            Debug("------------------------------SAM: " + ver + "------------------------------");


            /* Send first APDU to get Ek(RndB)	*/
            byte[] apdu = { 0x80, 0xA4, 0x00, 0x00, 0x02, KeyIdx, KeyVersion, 0x00 };

            CAPDU capdu = new CAPDU(apdu);

            Debug("Sending first capdu to get Ek(RndB) ...");
            Debug("Capdu:" + capdu.AsString());

            RAPDU rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            Debug("Rapdu:" + rapdu.AsString());

            if ((rapdu.SW != 0x90AF) || (rapdu.GetBytes().Length < 3))
            {
                Debug("Invalid rapdu");
                userInteraction.Error("Invalid Rapdu (" + rapdu.AsString() + ") while trying to connect with given key.");
                return false;
            }

            byte[] EkRndB = new byte[rapdu.GetBytes().Length - 2];
            for (int i = 0; i < EkRndB.Length; i++)
                EkRndB[i] = rapdu.GetByte(i);

            /* Decrypt EkRndB to get RndB	*/
            byte[] RndB = new byte[EkRndB.Length];
            Debug("Decrypting EkRndB to get RndB ...");
            switch (AuthType)
            {
                case AuthTypeE.TDES_CRC16:
                    RndB = CryptoPrimitives.TripleDES_Decrypt(EkRndB, KeyValue, initVector);
                    break;

                case AuthTypeE.TDES_CRC32:
                    RndB = CryptoPrimitives.TripleDES_Decrypt(EkRndB, KeyValue, initVector);
                    break;

                case AuthTypeE.AES:
                    RndB = CryptoPrimitives.AES_Decrypt(EkRndB, KeyValue, initVector);
                    break;

                default:
                    Debug("Can't decrypt: specified key is not 3DES nor AES");
                    userInteraction.Error("Type of Authentication Key should be 3DES or AES");
                    return false;

            }

            /* Keep IV	*/
            if (keepInitVector)
            {
                Debug("Keeping IV ...");
                Array.ConstrainedCopy(EkRndB, 0, initVector, 0, EkRndB.Length);
            }

            /* Generate RndA 	*/
            byte[] RndA;

            switch (AuthType)
            {
                case AuthTypeE.TDES_CRC16:
                    RndA = new byte[8];
                    break;

                case AuthTypeE.TDES_CRC32:
                    RndA = new byte[8];
                    break;

                case AuthTypeE.AES:
                    RndA = new byte[16];
                    break;

                default:
                    Debug("Can't decrypt: specified key is not 3DES nor AES");
                    userInteraction.Error("Type of Authentication Key should be 3DES or AES");
                    return false;
            }


            for (int i = 0; i < RndA.Length; i++)
                RndA[i] = (byte)rand.Next(0x00, 0xFF);

            /* Generate and crypt RndA+RndB' 	*/
            Debug("Generating RndA+RndB' ...");
            byte[] RndBp = CryptoPrimitives.rotate_left(RndB);
            byte[] concat = new byte[RndBp.Length + RndA.Length];
            Array.ConstrainedCopy(RndA, 0, concat, 0, RndA.Length);
            Array.ConstrainedCopy(RndBp, 0, concat, RndA.Length, RndBp.Length);

            Debug("Crypting RndA+RndB' ...");
            byte[] cryptConcat = new byte[concat.Length];

            switch (AuthType)
            {
                case AuthTypeE.TDES_CRC16:
                    cryptConcat = CryptoPrimitives.TripleDES_Encrypt(concat, KeyValue, initVector);
                    break;

                case AuthTypeE.TDES_CRC32:
                    cryptConcat = CryptoPrimitives.TripleDES_Encrypt(concat, KeyValue, initVector);
                    break;

                case AuthTypeE.AES:
                    cryptConcat = CryptoPrimitives.AES_Encrypt(concat, KeyValue, initVector);
                    break;

                default:
                    Debug("Can't decrypt: specified key is not 3DES nor AES");
                    userInteraction.Error("Type of Authentication Key should be 3DES or AES");
                    return false;
            }

            /* Keep IV	*/
            /* si type AES : faire commencer à 16 pour garder les 16 derniers octets des 32	*/
            /* sinon, faire commencer à 8 pour garder les 8 derniers de 16	*/
            if (keepInitVector)
            {
                Debug("Keeping IV ...");
                if (AuthType == AuthTypeE.AES)
                {
                    Array.ConstrainedCopy(cryptConcat, 16, initVector, 0, initVector.Length);
                }
                else
                {
                    Array.ConstrainedCopy(cryptConcat, 8, initVector, 0, initVector.Length);
                }
            }


            /* Send Ek(RndA+RndB') to get Ek(RndAp)	*/
            Debug("Sending second capdu: Ek(RndA+RndB') to get Ek(RndAp) ...");

            capdu = new CAPDU(0x80, 0xA4, 0x00, 0x00, cryptConcat, 0x00);
            Debug("Capdu:" + capdu.AsString());

            rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            Debug("Rapdu:" + rapdu.AsString());
            if (rapdu.SW != 0x9000)
            {
                Debug("Incorrect Rapdu !");
				userInteraction.Error("Authentication to the SAM failed! (SAM says we are not genuine)");
				return false;
            }

            byte[] EkRndAp = new byte[rapdu.GetBytes().Length - 2];
            for (int i = 0; i < EkRndAp.Length; i++)
                EkRndAp[i] = rapdu.GetByte(i);

            byte[] RndAp_received = new byte[EkRndAp.Length];

            // Decrypt RndAp, rotate and check 
            Debug("Decrypting Ek(RndAp), rotating and checking ...");
            switch (AuthType)
            {
                case AuthTypeE.TDES_CRC16:
                    RndAp_received = CryptoPrimitives.TripleDES_Decrypt(EkRndAp, KeyValue, initVector);
                    break;

                case AuthTypeE.TDES_CRC32:
                    RndAp_received = CryptoPrimitives.TripleDES_Decrypt(EkRndAp, KeyValue, initVector);
                    break;

                case AuthTypeE.AES:
                    RndAp_received = CryptoPrimitives.AES_Decrypt(EkRndAp, KeyValue, initVector);
                    break;

                default:
                    Debug("Can't decrypt: specified key is not 3DES nor AES");
                    userInteraction.Error("Type of Authentication Key should be 3DES or AES");
                    return false;

            }

            // Keep IV
            if (keepInitVector)
            {
                Debug("Keeping IV ...");
                Array.ConstrainedCopy(EkRndAp, 0, initVector, 0, initVector.Length);
            }


            byte[] RndA_received = CryptoPrimitives.rotate_right(RndAp_received);


            for (int i = 0; i < RndA_received.Length; i++)
            {
                if (RndA[i] != RndA_received[i])
                {
                    Debug("Authentication failed !");
                    userInteraction.Error("Authentication to the SAM failed! (SAM is not genuine)");
                    return false;
                }
            }
            Debug("Authentication ok !");
            //System.Windows.Forms.MessageBox.Show("\n !! Authentication ok !! \n");

            /* Generate session key	*/
            Debug("Generating session key ...");

            session_key = new byte[KeyValue.Length];
            byte[] key_first = new byte[KeyValue.Length / 2];
            byte[] key_second = new byte[KeyValue.Length / 2];

            if ((AuthType == AuthTypeE.TDES_CRC16) || (AuthType == AuthTypeE.TDES_CRC32))
            {
                Array.ConstrainedCopy(KeyValue, 0, key_first, 0, KeyValue.Length / 2);
                Array.ConstrainedCopy(KeyValue, KeyValue.Length / 2, key_second, 0, KeyValue.Length / 2);
                int count;
                for (count = 0; count < key_first.Length; count++)
                {
                    if (key_first[count] != key_second[count])
                    {
                        Debug("Two halves of key not equal: 3DES session key");
                        Array.ConstrainedCopy(RndA, 0, session_key, 0, 4);
                        Array.ConstrainedCopy(RndB, 0, session_key, 4, 4);
                        Array.ConstrainedCopy(RndA, 4, session_key, 8, 4);
                        Array.ConstrainedCopy(RndB, 4, session_key, 12, 4);
                        break;
                    }
                }
                if (count == key_first.Length)
                {
                    Debug("Two halves of key equal: DES session key");
                    Array.ConstrainedCopy(RndA, 0, session_key, 0, 4);
                    Array.ConstrainedCopy(RndB, 0, session_key, 4, 4);
                    Array.ConstrainedCopy(RndA, 0, session_key, 8, 4);
                    Array.ConstrainedCopy(RndB, 0, session_key, 12, 4);
                }
            }
            else
            if (AuthType == AuthTypeE.AES)
            {
                Array.ConstrainedCopy(KeyValue, 0, key_first, 0, KeyValue.Length / 2);
                Array.ConstrainedCopy(KeyValue, KeyValue.Length / 2, key_second, 0, KeyValue.Length / 2);

                Array.ConstrainedCopy(RndA, 0, session_key, 0, 4);
                Array.ConstrainedCopy(RndB, 0, session_key, 4, 4);
                Array.ConstrainedCopy(RndA, 12, session_key, 8, 4);
                Array.ConstrainedCopy(RndB, 12, session_key, 12, 4);
            }
            else
            {
                Debug("Can't generate session key: specified key is not 3DES nor AES");
                userInteraction.Error("Type of Authentication Key should be 3DES or AES");
                return false;
            }
            Debug("Session key generated!");

            currentAuthType = AuthType;
            return true;
        }

        public bool AuthenticateAV2(AuthTypeE AuthType, byte KeyIdx, byte KeyVersion, byte[] KeyValue)
        {
            Debug("AuthenticateAV2");

            /*
			byte[] apdutest_v0 = {0x80, 0xc5, 0x00, 0x00, 0x03, 0x22, 0x01, 0x0a, 0x00};
			CAPDU capdutest = new CAPDU(apdutest_v0);		  
		  
		  RAPDU rapdutest_v0 = scard.Transmit(capdutest);
		  MessageBox.Show("rapdu test dump key=" + rapdutest_v0.AsString());
			*/


            byte[] apdu = { 0x80, 0xA4, 0x00, 0x00, 0x03, KeyIdx, KeyVersion, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(apdu);

            Debug("CAPDU: " + capdu.AsString());

            //2. rapdu
            RAPDU rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            Debug("RAPDU: " + rapdu.AsString());

            if ((rapdu.SW != 0x90AF) || (rapdu.GetBytes().Length < 3))
            {
                userInteraction.Error("Invalid response " + rapdu.AsString() + " while trying to connect with given key.");
                return false;
            }


            //3 et 4. rnd2 + CMAC_load
            Debug("Getting Rnd2 and CMAC Load...");
            byte[] Rnd2 = new byte[rapdu.GetBytes().Length - 2];
            byte[] CMAC_load = new byte[Rnd2.Length + 4];
            int offset = 0;
            for (int i = 0; i < rapdu.GetBytes().Length - 2; i++)
            {
                Rnd2[offset] = rapdu.GetByte(i);
                CMAC_load[offset] = rapdu.GetByte(i);
                offset++;
            }

            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;

            /*
			Console.Write("rnd2 = ");
			for (int i = 0; i<Rnd2.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", Rnd2[i]));
			Console.Write("\n");
			
			Console.Write("CMAC_load = ");
			for (int i = 0; i<CMAC_load.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC_load[i]));
			Console.Write("\n");
			*/

            //5. CMAC
            //byte[] Key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
            //byte[] IV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            //byte[] CMAC_load2 = {0xcc, 0x3e, 0xe4, 0x98, 0x8e, 0x60, 0x2a, 0xc1, 0x3a, 0x50, 0x06, 0x65, 0x00, 0x00, 0x00, 0x00 };

            Debug("Calculating CMAC...");
            byte[] CMAC_full = CryptoPrimitives.CalculateCMAC(KeyValue, initVector, CMAC_load);
            byte[] CMAC = new byte[CMAC_full.Length / 2];

            int j = 0;
            for (int i = 1; i < CMAC_full.Length;)
            {
                CMAC[j++] = CMAC_full[i];
                i += 2;
            }

            /*
			Console.Write("CMAC_full = ");
			for (int i = 0; i<CMAC_full.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC_full[i]));
			Console.Write("\n");	
			
			Console.Write("CMAC = ");
			for (int i = 0; i<CMAC.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC[i]));
			Console.Write("\n");				
			*/

            //6. Rnd1
            //byte[] Rnd1 = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b };
            Debug("Generating Rnd1 ...");
            Random rand = new Random();
            byte[] Rnd1 = new byte[12];
            for (int i = 0; i < Rnd1.Length; i++)
                Rnd1[i] = (byte)rand.Next(0x00, 0xFF);


            //byte[] rnd2_ex = { 0xcc,  0x3e, 0xe4, 0x98, 0x8e, 0x60, 0x2a, 0xc1, 0x3a, 0x50, 0x06, 0x65  };

            //7. CAPDU-part2
            Debug("Generating Second CAPDU ...");
            byte[] Capdu2 = new byte[CMAC.Length + Rnd1.Length];
            offset = 0;
            for (int i = 0; i < CMAC.Length; i++)
                Capdu2[offset++] = CMAC[i];

            for (int i = 0; i < Rnd1.Length; i++)
                Capdu2[offset++] = Rnd1[i];


            capdu = new CAPDU(0x80, 0xa4, 0x00, 0x00, Capdu2, 0x00);


            //8. SV1
            Debug("Generating SV1 ...");
            byte[] sv1 = new byte[16];
            offset = 0;
            for (int i = 7; i < 12; i++)
                sv1[offset++] = Rnd1[i];

            for (int i = 7; i < 12; i++)
                sv1[offset++] = Rnd2[i];

            for (int i = 0; i < 5; i++)
                sv1[offset++] = (byte)(Rnd1[i] ^ Rnd2[i]);

            sv1[offset] = 0x91;

            /*
			Console.Write("sv1 = ");
			for (int i = 0; i<sv1.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", sv1[i]));
			Console.Write("\n");	
			*/

            //9. KXE
            Debug("Calculating KXE ...");
            byte[] KXE = CryptoPrimitives.AES_Encrypt(sv1, KeyValue, initVector);

            /*
            Console.Write("KXE = ");
			for (int i = 0; i<KXE.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", KXE[i]));
			Console.Write("\n");	
			*/

            //10. R-APDU

            Debug("CAPDU: " + capdu.AsString());
            rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            Debug("RAPDU: " + rapdu.AsString());


            //11.CMAC Load
            Debug("Getting CMAC Load ...");
            offset = 0;
            for (int i = 0; i < Rnd1.Length; i++)
                CMAC_load[offset++] = Rnd1[i];
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;

            /*
			Console.Write("CMAC_load = ");
			for (int i = 0; i<CMAC_load.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC_load[i]));
			Console.Write("\n");	
			*/

            //12. CMAC
            Debug("Getting CMAC ...");
            CMAC_full = CryptoPrimitives.CalculateCMAC(KeyValue, initVector, CMAC_load);

            /*
			Console.Write("CMAC_full = ");
			for (int i = 0; i<CMAC_full.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC_full[i]));
			Console.Write("\n");	
			*/

            j = 0;

            for (int i = 1; i < CMAC_full.Length;)
            {
                CMAC[j++] = CMAC_full[i];
                i += 2;
            }

            /*
			Console.Write("CMAC = ");
			for (int i = 0; i<CMAC.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", CMAC[i]));
			Console.Write("\n");	
			*/

            //13. RndB
            Debug("Retrieving RndB ...");
            byte[] RndB_received = new byte[16];
            offset = 0;
            for (int i = 8; i < 24; i++)
                RndB_received[offset++] = rapdu.GetByte(i);

            /*
            Console.Write("RndB_received = ");
            for (int i = 0; i < RndB_received.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", RndB_received[i]));
            Console.Write("\n");
            */

            //	byte[] RndBReceived_Ex = { 0x32, 0x5a, 0x37, 0xf6, 0x50, 0x6e, 0xc6, 0x55, 0x77, 0x12, 0x4e, 0xaf, 0xc5, 0x99, 0xa4, 0xb4 };
            //	byte[] KXE_Ex = { 0x05, 0x5f, 0x21, 0x8d, 0xaa, 0xa1, 0xd8, 0x5d, 0x21, 0xde, 0x86, 0x07, 0x09, 0xca, 0xfc, 0xe8 };
            byte[] RndB = CryptoPrimitives.AES_Decrypt(RndB_received, KXE, initVector);
            /*
			Console.Write("RndB = ");
			for (int i = 0; i<RndB.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", RndB[i]));
			Console.Write("\n");	
			*/


            //14. RndA
            Debug("Generating RndA ...");
            byte[] RndA = new byte[16];
            rand = new Random();
            for (int i = 0; i < RndA.Length; i++)
                RndA[i] = (byte)rand.Next(0x00, 0xFF);

            //15. RndBpp
            Debug("Rotating RndB two times to get RndBpp ...");
            byte[] RndBp = CryptoPrimitives.rotate_left(RndB);
            byte[] RndBpp = CryptoPrimitives.rotate_left(RndBp);

            /*
            Console.Write("RndBpp = ");
            for (int i = 0; i < RndBpp.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", RndBpp[i]));
            Console.Write("\n");
            */

            //16. RndA+RndBpp
            Debug("Concatening RndA+RndBpp ...");
            byte[] concat = new byte[RndA.Length + RndBpp.Length];
            offset = 0;
            for (int i = 0; i < RndA.Length; i++)
                concat[offset++] = RndA[i];

            for (int i = 0; i < RndB.Length; i++)
                concat[offset++] = RndBpp[i];


            //17. Ek(Kxe, RndA+RndB)
            Debug("Encrypting the concatenation ...");
            byte[] Ek_Kxe_concat = CryptoPrimitives.AES_Encrypt(concat, KXE, initVector);

            //18. CAPDU, part 3
            capdu = new CAPDU(0x80, 0xA4, 0x00, 0x00, Ek_Kxe_concat, 0x00);

            //19. Rnda''
            Debug("Rotating RndA two times to get RndApp ...");
            byte[] RndAp = CryptoPrimitives.rotate_left(RndA);
            byte[] RndApp = CryptoPrimitives.rotate_left(RndAp);

            /*
			Console.Write("RndApp = ");
			for (int i = 0; i<RndApp.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", RndApp[i]));
			Console.Write("\n");		
			*/

            //20. Rapdu
            Debug("CAPDU: " + capdu.AsString());

            rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            Debug("RAPDU: " + rapdu.AsString());
            if (rapdu.GetBytes().Length <= 2)
            {
                return false;
            }
            //Console.WriteLine("rapdu=" + rapdu.AsString());			

            //21. Ek_RndA'' received

            byte[] Ek_RndApp_received = new byte[rapdu.Length - 2];
            offset = 0;
            for (int i = 0; i < rapdu.Length - 2; i++)
                Ek_RndApp_received[offset++] = rapdu.GetByte(i);

            //22.RndA'' received
            byte[] Rnda_received = CryptoPrimitives.AES_Decrypt(Ek_RndApp_received, KXE, initVector);

            /*
			Console.Write("RndApp Reveid = ");
			for (int i = 0; i<Rnda_received.Length; i++)
				Console.Write("-" + String.Format("{0:x02}", Rnda_received[i]));
			Console.Write("\n");			
			*/


            //23. Final test
            Debug("Final test ...");
            for (int i = 0; i < Rnda_received.Length; i++)
                if (Rnda_received[i] != RndApp[i])
                    return false;

            /*
              byte[] apdutest = {0x80, 0xc5, 0x00, 0x00, 0x03, 0x22, 0x01, 0x0a, 0x00};
              capdu = new CAPDU(apdutest);		  

            rapdu = scard.Transmit(capdu);
            MessageBox.Show("rapdu test dump key=" + rapdu.AsString());
            */

            Debug("Authenticating OK...");

            currentAuthType = AuthType;
            currentAuthKeyIdx = KeyIdx;
            currentAuthKeyVersion = KeyVersion;
            return true;

        }
    }
}
