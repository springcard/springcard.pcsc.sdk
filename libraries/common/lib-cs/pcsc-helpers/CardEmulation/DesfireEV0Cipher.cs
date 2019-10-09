//#define _DEBUG_CIPHER
//#define _DEBUG_MAC

using System;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	public class DesfireEV0Cipher 
	{
	  private TDES_CORE core;
	  private byte[] IV;
	  
		public DesfireEV0Cipher(byte[] key)
		{
		  core = new TDES_CORE(key);
		  IV = new byte[8];
		}
	
		public bool Recv(ref byte[] data)
		{	  
		  if (data.Length % 8 != 0)
		    return false;
		  
			int block_count = data.Length / 8;
		
#if _DEBUG_CIPHER
      Logger.Trace("CipherRecv(" + (new CardBuffer(data)).AsString() + ")");
      Logger.Trace("Blocks : " + block_count);
#endif

			byte[] buffer = new byte[8];

			for (int block=0; block<block_count; block++)
			{
			  for (int i=0; i<8; i++)
			    IV[i] = buffer[i];
				for (int i=0; i<8; i++)
				  buffer[i] = data[8 * block + i];
			  
			  byte[] t = new byte[8];
			  
				for (int i=0; i<8; i++)
				  t[i] = data[8 * block + i];
				
#if _DEBUG_CIPHER
        Logger.Trace("Block " + block + "/" + block_count + ", IV=" + (new CardBuffer(IV)).AsString() + ", C=" + (new CardBuffer(t)).AsString());
#endif

        core.Encrypt(ref t);
				  
#if _DEBUG_CIPHER
        Logger.Trace("\t   " + (new CardBuffer(t)).AsString());
#endif

				for (int i=0; i<8; i++)
				  data[8 * block + i] = (byte) (t[i] ^ IV[i]);
				
#if _DEBUG_CIPHER
        Logger.Trace("\t--> " + (new CardBuffer(data, 8 * block, 8)).AsString());
#endif
			}
			
			return true;
		}
		
		public bool Send(ref byte[] data)
		{
		  return Send(ref data, false);
		}

	  public bool Send(ref byte[] data, bool padd80)
		{
	    int data_length = data.Length;
	    if (padd80) data_length++;
	    int block_count = data_length / 8;

#if _DEBUG_CIPHER
      Logger.Trace("CipherSend(" + (new CardBuffer(data)).AsString() + ")");
#endif

      if (data_length % 8 != 0)
	    {
	      block_count += 1;    
        byte[] t = new byte[8 * block_count];
        for (int i=0; i<data.Length; i++)
          t[i] = data[i];
        data = t;
	    }
	    
      if (padd80)
        data[data_length-1] = 0x80;      
	    
#if _DEBUG_CIPHER
      Logger.Trace("           " + (new CardBuffer(data)).AsString());
      Logger.Trace("Blocks : " + block_count);
#endif

			for (int i=0; i<8; i++)
			  IV[i] = 0;

			for (int block=0; block<block_count; block++)
			{
#if _DEBUG_CIPHER
        Trace.Write("Block " + block + "/" + block_count + ", IV=" + (new CardBuffer(IV)).AsString() + ", P=" + (new CardBuffer(data, 8 * block, 8)).AsString());
#endif
			  
			  byte[] t = new byte[8];
			  
				for (int i=0; i<8; i++)
				  t[i] = (byte) (data[8 * block + i] ^ IV[i]);
				
#if _DEBUG_CIPHER
        Trace.Write(" -> " + (new CardBuffer(t)).AsString());
#endif
				
				core.Encrypt(ref t);
				  
				for (int i=0; i<8; i++)
				{
				  IV[i] = t[i];
				  data[8 * block + i] = t[i];
				}
				
#if _DEBUG_CIPHER
        Logger.Trace(", C=" + (new CardBuffer(data, 8 * block, 8)).AsString());
#endif
			}
			
			return true;
		}
	  
	  private class DES_CORE
    {
	    DESCryptoServiceProvider des;
	    
	    ICryptoTransform enc;
	    ICryptoTransform dec;
	    
  		static ICryptoTransform GetEncryptor(DESCryptoServiceProvider _des, byte[] _k)
  		{
  		  if (DESCryptoServiceProvider.IsWeakKey(_k) || DESCryptoServiceProvider.IsSemiWeakKey(_k))
  		  {
    			MethodInfo mi = _des.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
    			object[] Par = { _k, _des.Mode, _des.IV, _des.FeedbackSize, 0 };
    			ICryptoTransform trans = mi.Invoke(_des, Par) as ICryptoTransform;
    			return trans;
  		  } else
  		  {
  		    return _des.CreateEncryptor(_k, _des.IV);
  		  }
  		}
  
  		static ICryptoTransform GetDecryptor(DESCryptoServiceProvider _des, byte[] _k)
  		{
  		  if (DESCryptoServiceProvider.IsWeakKey(_k) || DESCryptoServiceProvider.IsSemiWeakKey(_k))
  		  {
    			MethodInfo mi = _des.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
    			object[] Par = { _k, _des.Mode, _des.IV, _des.FeedbackSize, 1 };
    			ICryptoTransform trans = mi.Invoke(_des, Par) as ICryptoTransform;
    			return trans;
  		  } else
  		  {
  		    return _des.CreateDecryptor(_k, _des.IV);
  		  }
  		}

	    public DES_CORE(byte[] k)
	    {
	      byte[] iv = new byte[8];
	      
	      des = new DESCryptoServiceProvider();	     
	      
	      des.IV = new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
			  des.Mode = CipherMode.ECB;
			  des.Padding= PaddingMode.Zeros;

				enc = GetEncryptor(des, k);
				dec = GetDecryptor(des, k);
	    }
	    
	    public void Encrypt(ref byte[] b)
	    {
  			MemoryStream ms = new MemoryStream();
  			CryptoStream es = new CryptoStream(ms, enc, CryptoStreamMode.Write);
  			
  			es.Write(b,0,8);
  			b = ms.ToArray();
  
  			es.Close();
  			ms.Close();
	    }

	    public void Decrypt(ref byte[] b)
	    {
  			MemoryStream ms = new MemoryStream();
  			CryptoStream ds = new CryptoStream(ms, dec, CryptoStreamMode.Write);
  			
  			ds.Write(b,0,8);
  			b = ms.ToArray();
  
  			ds.Close();
  			ms.Close();
	    }
    }

	  private class TDES_CORE
    {
	    private DES_CORE des1;
	    private DES_CORE des2;
	    private DES_CORE des3;
	    
	    public TDES_CORE(byte[] k)
	    {
	      byte[] k1 = null;
	      byte[] k2 = null;
	      byte[] k3 = null;
	            
	      if (k.Length >= 8)
	      {
	        k1 = new byte[8];
	        for (int i=0; i<8; i++)
	          k1[i] = k[i];
	      }
	      
	      if (k.Length >= 16)
	      {
	        k2 = new byte[8];
	        for (int i=0; i<8; i++)
	          k2[i] = k[8+i];
	      } else
	        k2 = k1;
	      
	      if (k.Length >= 24)
	      {
	        k3 = new byte[8];	        
	        for (int i=0; i<8; i++)
	          k3[i] = k[16+i];
	      } else
	        k3 = k1;
	      
        des1 = new DES_CORE(k1);
        des2 = new DES_CORE(k2);
        des3 = new DES_CORE(k3);
	    }
	    
	    public void Encrypt(ref byte[] b)
	    {
	      des1.Encrypt(ref b);
	      des2.Decrypt(ref b);
	      des3.Encrypt(ref b);
	    }

	    public void Decrypt(ref byte[] b)
	    {
	      des1.Decrypt(ref b);
	      des2.Encrypt(ref b);
	      des3.Decrypt(ref b);
	    }
    }

    static bool do_des_test(string key, string plain, string cipher)
    {
      DES_CORE des;
      
      Logger.Trace(key + "," + plain + "," + cipher);
      
      byte[] k = (new CardBuffer(key)).GetBytes();
      byte[] p = (new CardBuffer(plain)).GetBytes();
      byte[] c = (new CardBuffer(cipher)).GetBytes();
      
      des = new DES_CORE(k);
    
      byte[] b = new byte[8];
      for (int i=0; i<8; i++)
        b[i] = p[i];
      
      des.Encrypt(ref b);
      
      Logger.Trace((new CardBuffer(b)).AsString());
      Logger.Trace((new CardBuffer(c)).AsString());
      
      for (int i=0; i<8; i++)
        if (b[i] != c[i])
          return false;
      
      des.Decrypt(ref b);
      
      Logger.Trace((new CardBuffer(b)).AsString());
      Logger.Trace((new CardBuffer(p)).AsString());

      for (int i=0; i<8; i++)
        if (b[i] != p[i])
          return false;
    
      return true;
    }
    
    static bool do_tdes_test(string key, string plain, string cipher)
    {
      TDES_CORE tdes;
      
      Logger.Trace(key + "," + plain + "," + cipher);
      
      byte[] k = (new CardBuffer(key)).GetBytes();
      byte[] p = (new CardBuffer(plain)).GetBytes();
      byte[] c = (new CardBuffer(cipher)).GetBytes();
    
      tdes = new TDES_CORE(k);
    
      byte[] b = new byte[8];
      for (int i=0; i<8; i++)
        b[i] = p[i];
      
      tdes.Encrypt(ref b);
      
      Logger.Trace((new CardBuffer(b)).AsString());
      Logger.Trace((new CardBuffer(c)).AsString());

      for (int i=0; i<8; i++)
        if (b[i] != c[i])
          return false;
      
      tdes.Decrypt(ref b);

      Logger.Trace((new CardBuffer(b)).AsString());
      Logger.Trace((new CardBuffer(p)).AsString());
      
      for (int i=0; i<8; i++)
        if (b[i] != p[i])
          return false;
    
      return true;
    }
	  
	  static public bool SelfTest()
	  {
      if (!do_des_test("0000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("FFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFF", "7359B2163E4EDC58")) return false;
      if (!do_des_test("3000000000000000", "1000000000000001", "958E6E627A05557B")) return false;
      if (!do_des_test("1111111111111111", "1111111111111111", "F40379AB9E0EC533")) return false;
      if (!do_des_test("0123456789ABCDEF", "1111111111111111", "17668DFC7292532D")) return false;
      if (!do_des_test("1111111111111111", "0123456789ABCDEF", "8A5AE1F81AB8F2DD")) return false;
      if (!do_des_test("0000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("FEDCBA9876543210", "0123456789ABCDEF", "ED39D950FA74BCC4")) return false;
      if (!do_des_test("7CA110454A1A6E57", "01A1D6D039776742", "690F5B0D9A26939B")) return false;
      if (!do_des_test("0131D9619DC1376E", "5CD54CA83DEF57DA", "7A389D10354BD271")) return false;
      if (!do_des_test("07A1133E4A0B2686", "0248D43806F67172", "868EBB51CAB4599A")) return false;
      if (!do_des_test("3849674C2602319E", "51454B582DDF440A", "7178876E01F19B2A")) return false;
      if (!do_des_test("04B915BA43FEB5B6", "42FD443059577FA2", "AF37FB421F8C4095")) return false;
      if (!do_des_test("0113B970FD34F2CE", "059B5E0851CF143A", "86A560F10EC6D85B")) return false;
      if (!do_des_test("0170F175468FB5E6", "0756D8E0774761D2", "0CD3DA020021DC09")) return false;
      if (!do_des_test("43297FAD38E373FE", "762514B829BF486A", "EA676B2CB7DB2B7A")) return false;
      if (!do_des_test("07A7137045DA2A16", "3BDD119049372802", "DFD64A815CAF1A0F")) return false;
      if (!do_des_test("04689104C2FD3B2F", "26955F6835AF609A", "5C513C9C4886C088")) return false;
      if (!do_des_test("37D06BB516CB7546", "164D5E404F275232", "0A2AEEAE3FF4AB77")) return false;
      if (!do_des_test("1F08260D1AC2465E", "6B056E18759F5CCA", "EF1BF03E5DFA575A")) return false;
      if (!do_des_test("584023641ABA6176", "004BD6EF09176062", "88BF0DB6D70DEE56")) return false;
      if (!do_des_test("025816164629B007", "480D39006EE762F2", "A1F9915541020B56")) return false;
      if (!do_des_test("49793EBC79B3258F", "437540C8698F3CFA", "6FBF1CAFCFFD0556")) return false;
      if (!do_des_test("4FB05E1515AB73A7", "072D43A077075292", "2F22E49BAB7CA1AC")) return false;
      if (!do_des_test("49E95D6D4CA229BF", "02FE55778117F12A", "5A6B612CC26CCE4A")) return false;
      if (!do_des_test("018310DC409B26D6", "1D9D5C5018F728C2", "5F4C038ED12B2E41")) return false;
      if (!do_des_test("1C587F1C13924FEF", "305532286D6F295A", "63FAC0D034D9F793")) return false;
      if (!do_des_test("0101010101010101", "0123456789ABCDEF", "617B3A0CE8F07100")) return false;
      if (!do_des_test("1F1F1F1F0E0E0E0E", "0123456789ABCDEF", "DB958605F8C8C606")) return false;
      if (!do_des_test("E0FEE0FEF1FEF1FE", "0123456789ABCDEF", "EDBFD1C66C29CCC7")) return false;
      if (!do_des_test("0000000000000000", "FFFFFFFFFFFFFFFF", "355550B2150E2451")) return false;
      if (!do_des_test("FFFFFFFFFFFFFFFF", "0000000000000000", "CAAAAF4DEAF1DBAE")) return false;
      if (!do_des_test("0123456789ABCDEF", "0000000000000000", "D5D44FF720683D0D")) return false;
      if (!do_des_test("FEDCBA9876543210", "FFFFFFFFFFFFFFFF", "2A2BB008DF97C2F2")) return false;
    
      if (!do_des_test("8000000000000000", "0000000000000000", "95A8D72813DAA94D")) return false;
      if (!do_des_test("4000000000000000", "0000000000000000", "0EEC1487DD8C26D5")) return false;
      if (!do_des_test("2000000000000000", "0000000000000000", "7AD16FFB79C45926")) return false;
      if (!do_des_test("1000000000000000", "0000000000000000", "D3746294CA6A6CF3")) return false;
      if (!do_des_test("0800000000000000", "0000000000000000", "809F5F873C1FD761")) return false;
      if (!do_des_test("0400000000000000", "0000000000000000", "C02FAFFEC989D1FC")) return false;
      if (!do_des_test("0200000000000000", "0000000000000000", "4615AA1D33E72F10")) return false;
      if (!do_des_test("0100000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0080000000000000", "0000000000000000", "2055123350C00858")) return false;
      if (!do_des_test("0040000000000000", "0000000000000000", "DF3B99D6577397C8")) return false;
      if (!do_des_test("0020000000000000", "0000000000000000", "31FE17369B5288C9")) return false;
      if (!do_des_test("0010000000000000", "0000000000000000", "DFDD3CC64DAE1642")) return false;
      if (!do_des_test("0008000000000000", "0000000000000000", "178C83CE2B399D94")) return false;
      if (!do_des_test("0004000000000000", "0000000000000000", "50F636324A9B7F80")) return false;
      if (!do_des_test("0002000000000000", "0000000000000000", "A8468EE3BC18F06D")) return false;
      if (!do_des_test("0001000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000800000000000", "0000000000000000", "A2DC9E92FD3CDE92")) return false;
      if (!do_des_test("0000400000000000", "0000000000000000", "CAC09F797D031287")) return false;
      if (!do_des_test("0000200000000000", "0000000000000000", "90BA680B22AEB525")) return false;
      if (!do_des_test("0000100000000000", "0000000000000000", "CE7A24F350E280B6")) return false;
      if (!do_des_test("0000080000000000", "0000000000000000", "882BFF0AA01A0B87")) return false;
      if (!do_des_test("0000040000000000", "0000000000000000", "25610288924511C2")) return false;
      if (!do_des_test("0000020000000000", "0000000000000000", "C71516C29C75D170")) return false;
      if (!do_des_test("0000010000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000008000000000", "0000000000000000", "5199C29A52C9F059")) return false;
      if (!do_des_test("0000004000000000", "0000000000000000", "C22F0A294A71F29F")) return false;
      if (!do_des_test("0000002000000000", "0000000000000000", "EE371483714C02EA")) return false;
      if (!do_des_test("0000001000000000", "0000000000000000", "A81FBD448F9E522F")) return false;
      if (!do_des_test("0000000800000000", "0000000000000000", "4F644C92E192DFED")) return false;
      if (!do_des_test("0000000400000000", "0000000000000000", "1AFA9A66A6DF92AE")) return false;
      if (!do_des_test("0000000200000000", "0000000000000000", "B3C1CC715CB879D8")) return false;
      if (!do_des_test("0000000100000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000000080000000", "0000000000000000", "19D032E64AB0BD8B")) return false;
      if (!do_des_test("0000000040000000", "0000000000000000", "3CFAA7A7DC8720DC")) return false;
      if (!do_des_test("0000000020000000", "0000000000000000", "B7265F7F447AC6F3")) return false;
      if (!do_des_test("0000000010000000", "0000000000000000", "9DB73B3C0D163F54")) return false;
      if (!do_des_test("0000000008000000", "0000000000000000", "8181B65BABF4A975")) return false;
      if (!do_des_test("0000000004000000", "0000000000000000", "93C9B64042EAA240")) return false;
      if (!do_des_test("0000000002000000", "0000000000000000", "5570530829705592")) return false;
      if (!do_des_test("0000000001000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000000000800000", "0000000000000000", "8638809E878787A0")) return false;
      if (!do_des_test("0000000000400000", "0000000000000000", "41B9A79AF79AC208")) return false;
      if (!do_des_test("0000000000200000", "0000000000000000", "7A9BE42F2009A892")) return false;
      if (!do_des_test("0000000000100000", "0000000000000000", "29038D56BA6D2745")) return false;
      if (!do_des_test("0000000000080000", "0000000000000000", "5495C6ABF1E5DF51")) return false;
      if (!do_des_test("0000000000040000", "0000000000000000", "AE13DBD561488933")) return false;
      if (!do_des_test("0000000000020000", "0000000000000000", "024D1FFA8904E389")) return false;
      if (!do_des_test("0000000000010000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000000000008000", "0000000000000000", "D1399712F99BF02E")) return false;
      if (!do_des_test("0000000000004000", "0000000000000000", "14C1D7C1CFFEC79E")) return false;
      if (!do_des_test("0000000000002000", "0000000000000000", "1DE5279DAE3BED6F")) return false;
      if (!do_des_test("0000000000001000", "0000000000000000", "E941A33F85501303")) return false;
      if (!do_des_test("0000000000000800", "0000000000000000", "DA99DBBC9A03F379")) return false;
      if (!do_des_test("0000000000000400", "0000000000000000", "B7FC92F91D8E92E9")) return false;
      if (!do_des_test("0000000000000200", "0000000000000000", "AE8E5CAA3CA04E85")) return false;
      if (!do_des_test("0000000000000100", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000000000000080", "0000000000000000", "9CC62DF43B6EED74")) return false;
      if (!do_des_test("0000000000000040", "0000000000000000", "D863DBB5C59A91A0")) return false;
      if (!do_des_test("0000000000000020", "0000000000000000", "A1AB2190545B91D7")) return false;
      if (!do_des_test("0000000000000010", "0000000000000000", "0875041E64C570F7")) return false;
      if (!do_des_test("0000000000000008", "0000000000000000", "5A594528BEBEF1CC")) return false;
      if (!do_des_test("0000000000000004", "0000000000000000", "FCDB3291DE21F0C0")) return false;
      if (!do_des_test("0000000000000002", "0000000000000000", "869EFD7F9F265A09")) return false;
      if (!do_des_test("0000000000000001", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0000000000000000", "8000000000000000", "95F8A5E5DD31D900")) return false;
      if (!do_des_test("0000000000000000", "4000000000000000", "DD7F121CA5015619")) return false;
      if (!do_des_test("0000000000000000", "2000000000000000", "2E8653104F3834EA")) return false;
      if (!do_des_test("0000000000000000", "1000000000000000", "4BD388FF6CD81D4F")) return false;
      if (!do_des_test("0000000000000000", "0800000000000000", "20B9E767B2FB1456")) return false;
      if (!do_des_test("0000000000000000", "0400000000000000", "55579380D77138EF")) return false;
      if (!do_des_test("0000000000000000", "0200000000000000", "6CC5DEFAAF04512F")) return false;
      if (!do_des_test("0000000000000000", "0100000000000000", "0D9F279BA5D87260")) return false;
      if (!do_des_test("0000000000000000", "0080000000000000", "D9031B0271BD5A0A")) return false;
      if (!do_des_test("0000000000000000", "0040000000000000", "424250B37C3DD951")) return false;
      if (!do_des_test("0000000000000000", "0020000000000000", "B8061B7ECD9A21E5")) return false;
      if (!do_des_test("0000000000000000", "0010000000000000", "F15D0F286B65BD28")) return false;
      if (!do_des_test("0000000000000000", "0008000000000000", "ADD0CC8D6E5DEBA1")) return false;
      if (!do_des_test("0000000000000000", "0004000000000000", "E6D5F82752AD63D1")) return false;
      if (!do_des_test("0000000000000000", "0002000000000000", "ECBFE3BD3F591A5E")) return false;
      if (!do_des_test("0000000000000000", "0001000000000000", "F356834379D165CD")) return false;
      if (!do_des_test("0000000000000000", "0000800000000000", "2B9F982F20037FA9")) return false;
      if (!do_des_test("0000000000000000", "0000400000000000", "889DE068A16F0BE6")) return false;
      if (!do_des_test("0000000000000000", "0000200000000000", "E19E275D846A1298")) return false;
      if (!do_des_test("0000000000000000", "0000100000000000", "329A8ED523D71AEC")) return false;
      if (!do_des_test("0000000000000000", "0000080000000000", "E7FCE22557D23C97")) return false;
      if (!do_des_test("0000000000000000", "0000040000000000", "12A9F5817FF2D65D")) return false;
      if (!do_des_test("0000000000000000", "0000020000000000", "A484C3AD38DC9C19")) return false;
      if (!do_des_test("0000000000000000", "0000010000000000", "FBE00A8A1EF8AD72")) return false;
      if (!do_des_test("0000000000000000", "0000008000000000", "750D079407521363")) return false;
      if (!do_des_test("0000000000000000", "0000004000000000", "64FEED9C724C2FAF")) return false;
      if (!do_des_test("0000000000000000", "0000002000000000", "F02B263B328E2B60")) return false;
      if (!do_des_test("0000000000000000", "0000001000000000", "9D64555A9A10B852")) return false;
      if (!do_des_test("0000000000000000", "0000000800000000", "D106FF0BED5255D7")) return false;
      if (!do_des_test("0000000000000000", "0000000400000000", "E1652C6B138C64A5")) return false;
      if (!do_des_test("0000000000000000", "0000000200000000", "E428581186EC8F46")) return false;
      if (!do_des_test("0000000000000000", "0000000100000000", "AEB5F5EDE22D1A36")) return false;
      if (!do_des_test("0000000000000000", "0000000080000000", "E943D7568AEC0C5C")) return false;
      if (!do_des_test("0000000000000000", "0000000040000000", "DF98C8276F54B04B")) return false;
      if (!do_des_test("0000000000000000", "0000000020000000", "B160E4680F6C696F")) return false;
      if (!do_des_test("0000000000000000", "0000000010000000", "FA0752B07D9C4AB8")) return false;
      if (!do_des_test("0000000000000000", "0000000008000000", "CA3A2B036DBC8502")) return false;
      if (!do_des_test("0000000000000000", "0000000004000000", "5E0905517BB59BCF")) return false;
      if (!do_des_test("0000000000000000", "0000000002000000", "814EEB3B91D90726")) return false;
      if (!do_des_test("0000000000000000", "0000000001000000", "4D49DB1532919C9F")) return false;
      if (!do_des_test("0000000000000000", "0000000000800000", "25EB5FC3F8CF0621")) return false;
      if (!do_des_test("0000000000000000", "0000000000400000", "AB6A20C0620D1C6F")) return false;
      if (!do_des_test("0000000000000000", "0000000000200000", "79E90DBC98F92CCA")) return false;
      if (!do_des_test("0000000000000000", "0000000000100000", "866ECEDD8072BB0E")) return false;
      if (!do_des_test("0000000000000000", "0000000000080000", "8B54536F2F3E64A8")) return false;
      if (!do_des_test("0000000000000000", "0000000000040000", "EA51D3975595B86B")) return false;
      if (!do_des_test("0000000000000000", "0000000000020000", "CAFFC6AC4542DE31")) return false;
      if (!do_des_test("0000000000000000", "0000000000010000", "8DD45A2DDF90796C")) return false;
      if (!do_des_test("0000000000000000", "0000000000008000", "1029D55E880EC2D0")) return false;
      if (!do_des_test("0000000000000000", "0000000000004000", "5D86CB23639DBEA9")) return false;
      if (!do_des_test("0000000000000000", "0000000000002000", "1D1CA853AE7C0C5F")) return false;
      if (!do_des_test("0000000000000000", "0000000000001000", "CE332329248F3228")) return false;
      if (!do_des_test("0000000000000000", "0000000000000800", "8405D1ABE24FB942")) return false;
      if (!do_des_test("0000000000000000", "0000000000000400", "E643D78090CA4207")) return false;
      if (!do_des_test("0000000000000000", "0000000000000200", "48221B9937748A23")) return false;
      if (!do_des_test("0000000000000000", "0000000000000100", "DD7C0BBD61FAFD54")) return false;
      if (!do_des_test("0000000000000000", "0000000000000080", "2FBC291A570DB5C4")) return false;
      if (!do_des_test("0000000000000000", "0000000000000040", "E07C30D7E4E26E12")) return false;
      if (!do_des_test("0000000000000000", "0000000000000020", "0953E2258E8E90A1")) return false;
      if (!do_des_test("0000000000000000", "0000000000000010", "5B711BC4CEEBF2EE")) return false;
      if (!do_des_test("0000000000000000", "0000000000000008", "CC083F1E6D9E85F6")) return false;
      if (!do_des_test("0000000000000000", "0000000000000004", "D2FD8867D50D2DFE")) return false;
      if (!do_des_test("0000000000000000", "0000000000000002", "06E7EA22CE92708F")) return false;
      if (!do_des_test("0000000000000000", "0000000000000001", "166B40B44ABA4BD6")) return false;
      if (!do_des_test("0000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_des_test("0101010101010101", "0101010101010101", "994D4DC157B96C52")) return false;
      if (!do_des_test("0202020202020202", "0202020202020202", "E127C2B61D98E6E2")) return false;
      if (!do_des_test("0303030303030303", "0303030303030303", "984C91D78A269CE3")) return false;
      if (!do_des_test("0404040404040404", "0404040404040404", "1F4570BB77550683")) return false;
      if (!do_des_test("0505050505050505", "0505050505050505", "3990ABF98D672B16")) return false;
      if (!do_des_test("0606060606060606", "0606060606060606", "3F5150BBA081D585")) return false;
      if (!do_des_test("0707070707070707", "0707070707070707", "C65242248C9CF6F2")) return false;
      if (!do_des_test("0808080808080808", "0808080808080808", "10772D40FAD24257")) return false;
      if (!do_des_test("0909090909090909", "0909090909090909", "F0139440647A6E7B")) return false;
      if (!do_des_test("0A0A0A0A0A0A0A0A", "0A0A0A0A0A0A0A0A", "0A288603044D740C")) return false;
      if (!do_des_test("0B0B0B0B0B0B0B0B", "0B0B0B0B0B0B0B0B", "6359916942F7438F")) return false;
      if (!do_des_test("0C0C0C0C0C0C0C0C", "0C0C0C0C0C0C0C0C", "934316AE443CF08B")) return false;
      if (!do_des_test("0D0D0D0D0D0D0D0D", "0D0D0D0D0D0D0D0D", "E3F56D7F1130A2B7")) return false;
      if (!do_des_test("0E0E0E0E0E0E0E0E", "0E0E0E0E0E0E0E0E", "A2E4705087C6B6B4")) return false;
      if (!do_des_test("0F0F0F0F0F0F0F0F", "0F0F0F0F0F0F0F0F", "D5D76E09A447E8C3")) return false;
      if (!do_des_test("1010101010101010", "1010101010101010", "DD7515F2BFC17F85")) return false;
      if (!do_des_test("1111111111111111", "1111111111111111", "F40379AB9E0EC533")) return false;
      if (!do_des_test("1212121212121212", "1212121212121212", "96CD27784D1563E5")) return false;
      if (!do_des_test("1313131313131313", "1313131313131313", "2911CF5E94D33FE1")) return false;
      if (!do_des_test("1414141414141414", "1414141414141414", "377B7F7CA3E5BBB3")) return false;
      if (!do_des_test("1515151515151515", "1515151515151515", "701AA63832905A92")) return false;
      if (!do_des_test("1616161616161616", "1616161616161616", "2006E716C4252D6D")) return false;
      if (!do_des_test("1717171717171717", "1717171717171717", "452C1197422469F8")) return false;
      if (!do_des_test("1818181818181818", "1818181818181818", "C33FD1EB49CB64DA")) return false;
      if (!do_des_test("1919191919191919", "1919191919191919", "7572278F364EB50D")) return false;
      if (!do_des_test("1A1A1A1A1A1A1A1A", "1A1A1A1A1A1A1A1A", "69E51488403EF4C3")) return false;
      if (!do_des_test("1B1B1B1B1B1B1B1B", "1B1B1B1B1B1B1B1B", "FF847E0ADF192825")) return false;
      if (!do_des_test("1C1C1C1C1C1C1C1C", "1C1C1C1C1C1C1C1C", "521B7FB3B41BB791")) return false;
      if (!do_des_test("1D1D1D1D1D1D1D1D", "1D1D1D1D1D1D1D1D", "26059A6A0F3F6B35")) return false;
      if (!do_des_test("1E1E1E1E1E1E1E1E", "1E1E1E1E1E1E1E1E", "F24A8D2231C77538")) return false;
      if (!do_des_test("1F1F1F1F1F1F1F1F", "1F1F1F1F1F1F1F1F", "4FD96EC0D3304EF6")) return false;
      if (!do_des_test("2020202020202020", "2020202020202020", "18A9D580A900B699")) return false;
      if (!do_des_test("2121212121212121", "2121212121212121", "88586E1D755B9B5A")) return false;
      if (!do_des_test("2222222222222222", "2222222222222222", "0F8ADFFB11DC2784")) return false;
      if (!do_des_test("2323232323232323", "2323232323232323", "2F30446C8312404A")) return false;
      if (!do_des_test("2424242424242424", "2424242424242424", "0BA03D9E6C196511")) return false;
      if (!do_des_test("2525252525252525", "2525252525252525", "3E55E997611E4B7D")) return false;
      if (!do_des_test("2626262626262626", "2626262626262626", "B2522FB5F158F0DF")) return false;
      if (!do_des_test("2727272727272727", "2727272727272727", "2109425935406AB8")) return false;
      if (!do_des_test("2828282828282828", "2828282828282828", "11A16028F310FF16")) return false;
      if (!do_des_test("2929292929292929", "2929292929292929", "73F0C45F379FE67F")) return false;
      if (!do_des_test("2A2A2A2A2A2A2A2A", "2A2A2A2A2A2A2A2A", "DCAD4338F7523816")) return false;
      if (!do_des_test("2B2B2B2B2B2B2B2B", "2B2B2B2B2B2B2B2B", "B81634C1CEAB298C")) return false;
      if (!do_des_test("2C2C2C2C2C2C2C2C", "2C2C2C2C2C2C2C2C", "DD2CCB29B6C4C349")) return false;
      if (!do_des_test("2D2D2D2D2D2D2D2D", "2D2D2D2D2D2D2D2D", "7D07A77A2ABD50A7")) return false;
      if (!do_des_test("2E2E2E2E2E2E2E2E", "2E2E2E2E2E2E2E2E", "30C1B0C1FD91D371")) return false;
      if (!do_des_test("2F2F2F2F2F2F2F2F", "2F2F2F2F2F2F2F2F", "C4427B31AC61973B")) return false;
      if (!do_des_test("3030303030303030", "3030303030303030", "F47BB46273B15EB5")) return false;
      if (!do_des_test("3131313131313131", "3131313131313131", "655EA628CF62585F")) return false;
      if (!do_des_test("3232323232323232", "3232323232323232", "AC978C247863388F")) return false;
      if (!do_des_test("3333333333333333", "3333333333333333", "0432ED386F2DE328")) return false;
      if (!do_des_test("3434343434343434", "3434343434343434", "D254014CB986B3C2")) return false;
      if (!do_des_test("3535353535353535", "3535353535353535", "B256E34BEDB49801")) return false;
      if (!do_des_test("3636363636363636", "3636363636363636", "37F8759EB77E7BFC")) return false;
      if (!do_des_test("3737373737373737", "3737373737373737", "5013CA4F62C9CEA0")) return false;
      if (!do_des_test("3838383838383838", "3838383838383838", "8940F7B3EACA5939")) return false;
      if (!do_des_test("3939393939393939", "3939393939393939", "E22B19A55086774B")) return false;
      if (!do_des_test("3A3A3A3A3A3A3A3A", "3A3A3A3A3A3A3A3A", "B04A2AAC925ABB0B")) return false;
      if (!do_des_test("3B3B3B3B3B3B3B3B", "3B3B3B3B3B3B3B3B", "8D250D58361597FC")) return false;
      if (!do_des_test("3C3C3C3C3C3C3C3C", "3C3C3C3C3C3C3C3C", "51F0114FB6A6CD37")) return false;
      if (!do_des_test("3D3D3D3D3D3D3D3D", "3D3D3D3D3D3D3D3D", "9D0BB4DB830ECB73")) return false;
      if (!do_des_test("3E3E3E3E3E3E3E3E", "3E3E3E3E3E3E3E3E", "E96089D6368F3E1A")) return false;
      if (!do_des_test("3F3F3F3F3F3F3F3F", "3F3F3F3F3F3F3F3F", "5C4CA877A4E1E92D")) return false;
      if (!do_des_test("4040404040404040", "4040404040404040", "6D55DDBC8DEA95FF")) return false;
      if (!do_des_test("4141414141414141", "4141414141414141", "19DF84AC95551003")) return false;
      if (!do_des_test("4242424242424242", "4242424242424242", "724E7332696D08A7")) return false;
      if (!do_des_test("4343434343434343", "4343434343434343", "B91810B8CDC58FE2")) return false;
      if (!do_des_test("4444444444444444", "4444444444444444", "06E23526EDCCD0C4")) return false;
      if (!do_des_test("4545454545454545", "4545454545454545", "EF52491D5468D441")) return false;
      if (!do_des_test("4646464646464646", "4646464646464646", "48019C59E39B90C5")) return false;
      if (!do_des_test("4747474747474747", "4747474747474747", "0544083FB902D8C0")) return false;
      if (!do_des_test("4848484848484848", "4848484848484848", "63B15CADA668CE12")) return false;
      if (!do_des_test("4949494949494949", "4949494949494949", "EACC0C1264171071")) return false;
      if (!do_des_test("4A4A4A4A4A4A4A4A", "4A4A4A4A4A4A4A4A", "9D2B8C0AC605F274")) return false;
      if (!do_des_test("4B4B4B4B4B4B4B4B", "4B4B4B4B4B4B4B4B", "C90F2F4C98A8FB2A")) return false;
      if (!do_des_test("4C4C4C4C4C4C4C4C", "4C4C4C4C4C4C4C4C", "03481B4828FD1D04")) return false;
      if (!do_des_test("4D4D4D4D4D4D4D4D", "4D4D4D4D4D4D4D4D", "C78FC45A1DCEA2E2")) return false;
      if (!do_des_test("4E4E4E4E4E4E4E4E", "4E4E4E4E4E4E4E4E", "DB96D88C3460D801")) return false;
      if (!do_des_test("4F4F4F4F4F4F4F4F", "4F4F4F4F4F4F4F4F", "6C69E720F5105518")) return false;
      if (!do_des_test("5050505050505050", "5050505050505050", "0D262E418BC893F3")) return false;
      if (!do_des_test("5151515151515151", "5151515151515151", "6AD84FD7848A0A5C")) return false;
      if (!do_des_test("5252525252525252", "5252525252525252", "C365CB35B34B6114")) return false;
      if (!do_des_test("5353535353535353", "5353535353535353", "1155392E877F42A9")) return false;
      if (!do_des_test("5454545454545454", "5454545454545454", "531BE5F9405DA715")) return false;
      if (!do_des_test("5555555555555555", "5555555555555555", "3BCDD41E6165A5E8")) return false;
      if (!do_des_test("5656565656565656", "5656565656565656", "2B1FF5610A19270C")) return false;
      if (!do_des_test("5757575757575757", "5757575757575757", "D90772CF3F047CFD")) return false;
      if (!do_des_test("5858585858585858", "5858585858585858", "1BEA27FFB72457B7")) return false;
      if (!do_des_test("5959595959595959", "5959595959595959", "85C3E0C429F34C27")) return false;
      if (!do_des_test("5A5A5A5A5A5A5A5A", "5A5A5A5A5A5A5A5A", "F9038021E37C7618")) return false;
      if (!do_des_test("5B5B5B5B5B5B5B5B", "5B5B5B5B5B5B5B5B", "35BC6FF838DBA32F")) return false;
      if (!do_des_test("5C5C5C5C5C5C5C5C", "5C5C5C5C5C5C5C5C", "4927ACC8CE45ECE7")) return false;
      if (!do_des_test("5D5D5D5D5D5D5D5D", "5D5D5D5D5D5D5D5D", "E812EE6E3572985C")) return false;
      if (!do_des_test("5E5E5E5E5E5E5E5E", "5E5E5E5E5E5E5E5E", "9BB93A89627BF65F")) return false;
      if (!do_des_test("5F5F5F5F5F5F5F5F", "5F5F5F5F5F5F5F5F", "EF12476884CB74CA")) return false;
      if (!do_des_test("6060606060606060", "6060606060606060", "1BF17E00C09E7CBF")) return false;
      if (!do_des_test("6161616161616161", "6161616161616161", "29932350C098DB5D")) return false;
      if (!do_des_test("6262626262626262", "6262626262626262", "B476E6499842AC54")) return false;
      if (!do_des_test("6363636363636363", "6363636363636363", "5C662C29C1E96056")) return false;
      if (!do_des_test("6464646464646464", "6464646464646464", "3AF1703D76442789")) return false;
      if (!do_des_test("6565656565656565", "6565656565656565", "86405D9B425A8C8C")) return false;
      if (!do_des_test("6666666666666666", "6666666666666666", "EBBF4810619C2C55")) return false;
      if (!do_des_test("6767676767676767", "6767676767676767", "F8D1CD7367B21B5D")) return false;
      if (!do_des_test("6868686868686868", "6868686868686868", "9EE703142BF8D7E2")) return false;
      if (!do_des_test("6969696969696969", "6969696969696969", "5FDFFFC3AAAB0CB3")) return false;
      if (!do_des_test("6A6A6A6A6A6A6A6A", "6A6A6A6A6A6A6A6A", "26C940AB13574231")) return false;
      if (!do_des_test("6B6B6B6B6B6B6B6B", "6B6B6B6B6B6B6B6B", "1E2DC77E36A84693")) return false;
      if (!do_des_test("6C6C6C6C6C6C6C6C", "6C6C6C6C6C6C6C6C", "0F4FF4D9BC7E2244")) return false;
      if (!do_des_test("6D6D6D6D6D6D6D6D", "6D6D6D6D6D6D6D6D", "A4C9A0D04D3280CD")) return false;
      if (!do_des_test("6E6E6E6E6E6E6E6E", "6E6E6E6E6E6E6E6E", "9FAF2C96FE84919D")) return false;
      if (!do_des_test("6F6F6F6F6F6F6F6F", "6F6F6F6F6F6F6F6F", "115DBC965E6096C8")) return false;
      if (!do_des_test("7070707070707070", "7070707070707070", "AF531E9520994017")) return false;
      if (!do_des_test("7171717171717171", "7171717171717171", "B971ADE70E5C89EE")) return false;
      if (!do_des_test("7272727272727272", "7272727272727272", "415D81C86AF9C376")) return false;
      if (!do_des_test("7373737373737373", "7373737373737373", "8DFB864FDB3C6811")) return false;
      if (!do_des_test("7474747474747474", "7474747474747474", "10B1C170E3398F91")) return false;
      if (!do_des_test("7575757575757575", "7575757575757575", "CFEF7A1C0218DB1E")) return false;
      if (!do_des_test("7676767676767676", "7676767676767676", "DBAC30A2A40B1B9C")) return false;
      if (!do_des_test("7777777777777777", "7777777777777777", "89D3BF37052162E9")) return false;
      if (!do_des_test("7878787878787878", "7878787878787878", "80D9230BDAEB67DC")) return false;
      if (!do_des_test("7979797979797979", "7979797979797979", "3440911019AD68D7")) return false;
      if (!do_des_test("7A7A7A7A7A7A7A7A", "7A7A7A7A7A7A7A7A", "9626FE57596E199E")) return false;
      if (!do_des_test("7B7B7B7B7B7B7B7B", "7B7B7B7B7B7B7B7B", "DEA0B796624BB5BA")) return false;
      if (!do_des_test("7C7C7C7C7C7C7C7C", "7C7C7C7C7C7C7C7C", "E9E40542BDDB3E9D")) return false;
      if (!do_des_test("7D7D7D7D7D7D7D7D", "7D7D7D7D7D7D7D7D", "8AD99914B354B911")) return false;
      if (!do_des_test("7E7E7E7E7E7E7E7E", "7E7E7E7E7E7E7E7E", "6F85B98DD12CB13B")) return false;
      if (!do_des_test("7F7F7F7F7F7F7F7F", "7F7F7F7F7F7F7F7F", "10130DA3C3A23924")) return false;
      if (!do_des_test("8080808080808080", "8080808080808080", "EFECF25C3C5DC6DB")) return false;
      if (!do_des_test("8181818181818181", "8181818181818181", "907A46722ED34EC4")) return false;
      if (!do_des_test("8282828282828282", "8282828282828282", "752666EB4CAB46EE")) return false;
      if (!do_des_test("8383838383838383", "8383838383838383", "161BFABD4224C162")) return false;
      if (!do_des_test("8484848484848484", "8484848484848484", "215F48699DB44A45")) return false;
      if (!do_des_test("8585858585858585", "8585858585858585", "69D901A8A691E661")) return false;
      if (!do_des_test("8686868686868686", "8686868686868686", "CBBF6EEFE6529728")) return false;
      if (!do_des_test("8787878787878787", "8787878787878787", "7F26DCF425149823")) return false;
      if (!do_des_test("8888888888888888", "8888888888888888", "762C40C8FADE9D16")) return false;
      if (!do_des_test("8989898989898989", "8989898989898989", "2453CF5D5BF4E463")) return false;
      if (!do_des_test("8A8A8A8A8A8A8A8A", "8A8A8A8A8A8A8A8A", "301085E3FDE724E1")) return false;
      if (!do_des_test("8B8B8B8B8B8B8B8B", "8B8B8B8B8B8B8B8B", "EF4E3E8F1CC6706E")) return false;
      if (!do_des_test("8C8C8C8C8C8C8C8C", "8C8C8C8C8C8C8C8C", "720479B024C397EE")) return false;
      if (!do_des_test("8D8D8D8D8D8D8D8D", "8D8D8D8D8D8D8D8D", "BEA27E3795063C89")) return false;
      if (!do_des_test("8E8E8E8E8E8E8E8E", "8E8E8E8E8E8E8E8E", "468E5218F1A37611")) return false;
      if (!do_des_test("8F8F8F8F8F8F8F8F", "8F8F8F8F8F8F8F8F", "50ACE16ADF66BFE8")) return false;
      if (!do_des_test("9090909090909090", "9090909090909090", "EEA24369A19F6937")) return false;
      if (!do_des_test("9191919191919191", "9191919191919191", "6050D369017B6E62")) return false;
      if (!do_des_test("9292929292929292", "9292929292929292", "5B365F2FB2CD7F32")) return false;
      if (!do_des_test("9393939393939393", "9393939393939393", "F0B00B264381DDBB")) return false;
      if (!do_des_test("9494949494949494", "9494949494949494", "E1D23881C957B96C")) return false;
      if (!do_des_test("9595959595959595", "9595959595959595", "D936BF54ECA8BDCE")) return false;
      if (!do_des_test("9696969696969696", "9696969696969696", "A020003C5554F34C")) return false;
      if (!do_des_test("9797979797979797", "9797979797979797", "6118FCEBD407281D")) return false;
      if (!do_des_test("9898989898989898", "9898989898989898", "072E328C984DE4A2")) return false;
      if (!do_des_test("9999999999999999", "9999999999999999", "1440B7EF9E63D3AA")) return false;
      if (!do_des_test("9A9A9A9A9A9A9A9A", "9A9A9A9A9A9A9A9A", "79BFA264BDA57373")) return false;
      if (!do_des_test("9B9B9B9B9B9B9B9B", "9B9B9B9B9B9B9B9B", "C50E8FC289BBD876")) return false;
      if (!do_des_test("9C9C9C9C9C9C9C9C", "9C9C9C9C9C9C9C9C", "A399D3D63E169FA9")) return false;
      if (!do_des_test("9D9D9D9D9D9D9D9D", "9D9D9D9D9D9D9D9D", "4B8919B667BD53AB")) return false;
      if (!do_des_test("9E9E9E9E9E9E9E9E", "9E9E9E9E9E9E9E9E", "D66CDCAF3F6724A2")) return false;
      if (!do_des_test("9F9F9F9F9F9F9F9F", "9F9F9F9F9F9F9F9F", "E40E81FF3F618340")) return false;
      if (!do_des_test("A0A0A0A0A0A0A0A0", "A0A0A0A0A0A0A0A0", "10EDB8977B348B35")) return false;
      if (!do_des_test("A1A1A1A1A1A1A1A1", "A1A1A1A1A1A1A1A1", "6446C5769D8409A0")) return false;
      if (!do_des_test("A2A2A2A2A2A2A2A2", "A2A2A2A2A2A2A2A2", "17ED1191CA8D67A3")) return false;
      if (!do_des_test("A3A3A3A3A3A3A3A3", "A3A3A3A3A3A3A3A3", "B6D8533731BA1318")) return false;
      if (!do_des_test("A4A4A4A4A4A4A4A4", "A4A4A4A4A4A4A4A4", "CA439007C7245CD0")) return false;
      if (!do_des_test("A5A5A5A5A5A5A5A5", "A5A5A5A5A5A5A5A5", "06FC7FDE1C8389E7")) return false;
      if (!do_des_test("A6A6A6A6A6A6A6A6", "A6A6A6A6A6A6A6A6", "7A3C1F3BD60CB3D8")) return false;
      if (!do_des_test("A7A7A7A7A7A7A7A7", "A7A7A7A7A7A7A7A7", "E415D80048DBA848")) return false;
      if (!do_des_test("A8A8A8A8A8A8A8A8", "A8A8A8A8A8A8A8A8", "26F88D30C0FB8302")) return false;
      if (!do_des_test("A9A9A9A9A9A9A9A9", "A9A9A9A9A9A9A9A9", "D4E00A9EF5E6D8F3")) return false;
      if (!do_des_test("AAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA", "C4322BE19E9A5A17")) return false;
      if (!do_des_test("ABABABABABABABAB", "ABABABABABABABAB", "ACE41A06BFA258EA")) return false;
      if (!do_des_test("ACACACACACACACAC", "ACACACACACACACAC", "EEAAC6D17880BD56")) return false;
      if (!do_des_test("ADADADADADADADAD", "ADADADADADADADAD", "3C9A34CA4CB49EEB")) return false;
      if (!do_des_test("AEAEAEAEAEAEAEAE", "AEAEAEAEAEAEAEAE", "9527B0287B75F5A3")) return false;
      if (!do_des_test("AFAFAFAFAFAFAFAF", "AFAFAFAFAFAFAFAF", "F2D9D1BE74376C0C")) return false;
      if (!do_des_test("B0B0B0B0B0B0B0B0", "B0B0B0B0B0B0B0B0", "939618DF0AEFAAE7")) return false;
      if (!do_des_test("B1B1B1B1B1B1B1B1", "B1B1B1B1B1B1B1B1", "24692773CB9F27FE")) return false;
      if (!do_des_test("B2B2B2B2B2B2B2B2", "B2B2B2B2B2B2B2B2", "38703BA5E2315D1D")) return false;
      if (!do_des_test("B3B3B3B3B3B3B3B3", "B3B3B3B3B3B3B3B3", "FCB7E4B7D702E2FB")) return false;
      if (!do_des_test("B4B4B4B4B4B4B4B4", "B4B4B4B4B4B4B4B4", "36F0D0B3675704D5")) return false;
      if (!do_des_test("B5B5B5B5B5B5B5B5", "B5B5B5B5B5B5B5B5", "62D473F539FA0D8B")) return false;
      if (!do_des_test("B6B6B6B6B6B6B6B6", "B6B6B6B6B6B6B6B6", "1533F3ED9BE8EF8E")) return false;
      if (!do_des_test("B7B7B7B7B7B7B7B7", "B7B7B7B7B7B7B7B7", "9C4EA352599731ED")) return false;
      if (!do_des_test("B8B8B8B8B8B8B8B8", "B8B8B8B8B8B8B8B8", "FABBF7C046FD273F")) return false;
      if (!do_des_test("B9B9B9B9B9B9B9B9", "B9B9B9B9B9B9B9B9", "B7FE63A61C646F3A")) return false;
      if (!do_des_test("BABABABABABABABA", "BABABABABABABABA", "10ADB6E2AB972BBE")) return false;
      if (!do_des_test("BBBBBBBBBBBBBBBB", "BBBBBBBBBBBBBBBB", "F91DCAD912332F3B")) return false;
      if (!do_des_test("BCBCBCBCBCBCBCBC", "BCBCBCBCBCBCBCBC", "46E7EF47323A701D")) return false;
      if (!do_des_test("BDBDBDBDBDBDBDBD", "BDBDBDBDBDBDBDBD", "8DB18CCD9692F758")) return false;
      if (!do_des_test("BEBEBEBEBEBEBEBE", "BEBEBEBEBEBEBEBE", "E6207B536AAAEFFC")) return false;
      if (!do_des_test("BFBFBFBFBFBFBFBF", "BFBFBFBFBFBFBFBF", "92AA224372156A00")) return false;
      if (!do_des_test("C0C0C0C0C0C0C0C0", "C0C0C0C0C0C0C0C0", "A3B357885B1E16D2")) return false;
      if (!do_des_test("C1C1C1C1C1C1C1C1", "C1C1C1C1C1C1C1C1", "169F7629C970C1E5")) return false;
      if (!do_des_test("C2C2C2C2C2C2C2C2", "C2C2C2C2C2C2C2C2", "62F44B247CF1348C")) return false;
      if (!do_des_test("C3C3C3C3C3C3C3C3", "C3C3C3C3C3C3C3C3", "AE0FEEB0495932C8")) return false;
      if (!do_des_test("C4C4C4C4C4C4C4C4", "C4C4C4C4C4C4C4C4", "72DAF2A7C9EA6803")) return false;
      if (!do_des_test("C5C5C5C5C5C5C5C5", "C5C5C5C5C5C5C5C5", "4FB5D5536DA544F4")) return false;
      if (!do_des_test("C6C6C6C6C6C6C6C6", "C6C6C6C6C6C6C6C6", "1DD4E65AAF7988B4")) return false;
      if (!do_des_test("C7C7C7C7C7C7C7C7", "C7C7C7C7C7C7C7C7", "76BF084C1535A6C6")) return false;
      if (!do_des_test("C8C8C8C8C8C8C8C8", "C8C8C8C8C8C8C8C8", "AFEC35B09D36315F")) return false;
      if (!do_des_test("C9C9C9C9C9C9C9C9", "C9C9C9C9C9C9C9C9", "C8078A6148818403")) return false;
      if (!do_des_test("CACACACACACACACA", "CACACACACACACACA", "4DA91CB4124B67FE")) return false;
      if (!do_des_test("CBCBCBCBCBCBCBCB", "CBCBCBCBCBCBCBCB", "2DABFEB346794C3D")) return false;
      if (!do_des_test("CCCCCCCCCCCCCCCC", "CCCCCCCCCCCCCCCC", "FBCD12C790D21CD7")) return false;
      if (!do_des_test("CDCDCDCDCDCDCDCD", "CDCDCDCDCDCDCDCD", "536873DB879CC770")) return false;
      if (!do_des_test("CECECECECECECECE", "CECECECECECECECE", "9AA159D7309DA7A0")) return false;
      if (!do_des_test("CFCFCFCFCFCFCFCF", "CFCFCFCFCFCFCFCF", "0B844B9D8C4EA14A")) return false;
      if (!do_des_test("D0D0D0D0D0D0D0D0", "D0D0D0D0D0D0D0D0", "3BBD84CE539E68C4")) return false;
      if (!do_des_test("D1D1D1D1D1D1D1D1", "D1D1D1D1D1D1D1D1", "CF3E4F3E026E2C8E")) return false;
      if (!do_des_test("D2D2D2D2D2D2D2D2", "D2D2D2D2D2D2D2D2", "82F85885D542AF58")) return false;
      if (!do_des_test("D3D3D3D3D3D3D3D3", "D3D3D3D3D3D3D3D3", "22D334D6493B3CB6")) return false;
      if (!do_des_test("D4D4D4D4D4D4D4D4", "D4D4D4D4D4D4D4D4", "47E9CB3E3154D673")) return false;
      if (!do_des_test("D5D5D5D5D5D5D5D5", "D5D5D5D5D5D5D5D5", "2352BCC708ADC7E9")) return false;
      if (!do_des_test("D6D6D6D6D6D6D6D6", "D6D6D6D6D6D6D6D6", "8C0F3BA0C8601980")) return false;
      if (!do_des_test("D7D7D7D7D7D7D7D7", "D7D7D7D7D7D7D7D7", "EE5E9FD70CEF00E9")) return false;
      if (!do_des_test("D8D8D8D8D8D8D8D8", "D8D8D8D8D8D8D8D8", "DEF6BDA6CABF9547")) return false;
      if (!do_des_test("D9D9D9D9D9D9D9D9", "D9D9D9D9D9D9D9D9", "4DADD04A0EA70F20")) return false;
      if (!do_des_test("DADADADADADADADA", "DADADADADADADADA", "C1AA16689EE1B482")) return false;
      if (!do_des_test("DBDBDBDBDBDBDBDB", "DBDBDBDBDBDBDBDB", "F45FC26193E69AEE")) return false;
      if (!do_des_test("DCDCDCDCDCDCDCDC", "DCDCDCDCDCDCDCDC", "D0CFBB937CEDBFB5")) return false;
      if (!do_des_test("DDDDDDDDDDDDDDDD", "DDDDDDDDDDDDDDDD", "F0752004EE23D87B")) return false;
      if (!do_des_test("DEDEDEDEDEDEDEDE", "DEDEDEDEDEDEDEDE", "77A791E28AA464A5")) return false;
      if (!do_des_test("DFDFDFDFDFDFDFDF", "DFDFDFDFDFDFDFDF", "E7562A7F56FF4966")) return false;
      if (!do_des_test("E0E0E0E0E0E0E0E0", "E0E0E0E0E0E0E0E0", "B026913F2CCFB109")) return false;
      if (!do_des_test("E1E1E1E1E1E1E1E1", "E1E1E1E1E1E1E1E1", "0DB572DDCE388AC7")) return false;
      if (!do_des_test("E2E2E2E2E2E2E2E2", "E2E2E2E2E2E2E2E2", "D9FA6595F0C094CA")) return false;
      if (!do_des_test("E3E3E3E3E3E3E3E3", "E3E3E3E3E3E3E3E3", "ADE4804C4BE4486E")) return false;
      if (!do_des_test("E4E4E4E4E4E4E4E4", "E4E4E4E4E4E4E4E4", "007B81F520E6D7DA")) return false;
      if (!do_des_test("E5E5E5E5E5E5E5E5", "E5E5E5E5E5E5E5E5", "961AEB77BFC10B3C")) return false;
      if (!do_des_test("E6E6E6E6E6E6E6E6", "E6E6E6E6E6E6E6E6", "8A8DD870C9B14AF2")) return false;
      if (!do_des_test("E7E7E7E7E7E7E7E7", "E7E7E7E7E7E7E7E7", "3CC02E14B6349B25")) return false;
      if (!do_des_test("E8E8E8E8E8E8E8E8", "E8E8E8E8E8E8E8E8", "BAD3EE68BDDB9607")) return false;
      if (!do_des_test("E9E9E9E9E9E9E9E9", "E9E9E9E9E9E9E9E9", "DFF918E93BDAD292")) return false;
      if (!do_des_test("EAEAEAEAEAEAEAEA", "EAEAEAEAEAEAEAEA", "8FE559C7CD6FA56D")) return false;
      if (!do_des_test("EBEBEBEBEBEBEBEB", "EBEBEBEBEBEBEBEB", "C88480835C1A444C")) return false;
      if (!do_des_test("ECECECECECECECEC", "ECECECECECECECEC", "D6EE30A16B2CC01E")) return false;
      if (!do_des_test("EDEDEDEDEDEDEDED", "EDEDEDEDEDEDEDED", "6932D887B2EA9C1A")) return false;
      if (!do_des_test("EEEEEEEEEEEEEEEE", "EEEEEEEEEEEEEEEE", "0BFC865461F13ACC")) return false;
      if (!do_des_test("EFEFEFEFEFEFEFEF", "EFEFEFEFEFEFEFEF", "228AEA0D403E807A")) return false;
      if (!do_des_test("F0F0F0F0F0F0F0F0", "F0F0F0F0F0F0F0F0", "2A2891F65BB8173C")) return false;
      if (!do_des_test("F1F1F1F1F1F1F1F1", "F1F1F1F1F1F1F1F1", "5D1B8FAF7839494B")) return false;
      if (!do_des_test("F2F2F2F2F2F2F2F2", "F2F2F2F2F2F2F2F2", "1C0A9280EECF5D48")) return false;
      if (!do_des_test("F3F3F3F3F3F3F3F3", "F3F3F3F3F3F3F3F3", "6CBCE951BBC30F74")) return false;
      if (!do_des_test("F4F4F4F4F4F4F4F4", "F4F4F4F4F4F4F4F4", "9CA66E96BD08BC70")) return false;
      if (!do_des_test("F5F5F5F5F5F5F5F5", "F5F5F5F5F5F5F5F5", "F5D779FCFBB28BF3")) return false;
      if (!do_des_test("F6F6F6F6F6F6F6F6", "F6F6F6F6F6F6F6F6", "0FEC6BBF9B859184")) return false;
      if (!do_des_test("F7F7F7F7F7F7F7F7", "F7F7F7F7F7F7F7F7", "EF88D2BF052DBDA8")) return false;
      if (!do_des_test("F8F8F8F8F8F8F8F8", "F8F8F8F8F8F8F8F8", "39ADBDDB7363090D")) return false;
      if (!do_des_test("F9F9F9F9F9F9F9F9", "F9F9F9F9F9F9F9F9", "C0AEAF445F7E2A7A")) return false;
      if (!do_des_test("FAFAFAFAFAFAFAFA", "FAFAFAFAFAFAFAFA", "C66F54067298D4E9")) return false;
      if (!do_des_test("FBFBFBFBFBFBFBFB", "FBFBFBFBFBFBFBFB", "E0BA8F4488AAF97C")) return false;
      if (!do_des_test("FCFCFCFCFCFCFCFC", "FCFCFCFCFCFCFCFC", "67B36E2875D9631C")) return false;
      if (!do_des_test("FDFDFDFDFDFDFDFD", "FDFDFDFDFDFDFDFD", "1ED83D49E267191D")) return false;
      if (!do_des_test("FEFEFEFEFEFEFEFE", "FEFEFEFEFEFEFEFE", "66B2B23EA84693AD")) return false;
      if (!do_des_test("FFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFF", "7359B2163E4EDC58")) return false;
      if (!do_des_test("0001020304050607", "0011223344556677", "3EF0A891CF8ED990")) return false;
      if (!do_des_test("2BD6459F82C5B300", "EA024714AD5C4D84", "126EFE8ED312190A")) return false;
    
      if (!do_des_test("0101010101010101", "95F8A5E5DD31D900", "8000000000000000")) return false;
      if (!do_des_test("0101010101010101", "DD7F121CA5015619", "4000000000000000")) return false;
      if (!do_des_test("0101010101010101", "2E8653104F3834EA", "2000000000000000")) return false;
      if (!do_des_test("0101010101010101", "4BD388FF6CD81D4F", "1000000000000000")) return false;
      if (!do_des_test("0101010101010101", "20B9E767B2FB1456", "0800000000000000")) return false;
      if (!do_des_test("0101010101010101", "55579380D77138EF", "0400000000000000")) return false;
      if (!do_des_test("0101010101010101", "6CC5DEFAAF04512F", "0200000000000000")) return false;
      if (!do_des_test("0101010101010101", "0D9F279BA5D87260", "0100000000000000")) return false;
      if (!do_des_test("0101010101010101", "D9031B0271BD5A0A", "0080000000000000")) return false;
      if (!do_des_test("0101010101010101", "424250B37C3DD951", "0040000000000000")) return false;
      if (!do_des_test("0101010101010101", "B8061B7ECD9A21E5", "0020000000000000")) return false;
      if (!do_des_test("0101010101010101", "F15D0F286B65BD28", "0010000000000000")) return false;
      if (!do_des_test("0101010101010101", "ADD0CC8D6E5DEBA1", "0008000000000000")) return false;
      if (!do_des_test("0101010101010101", "E6D5F82752AD63D1", "0004000000000000")) return false;
      if (!do_des_test("0101010101010101", "ECBFE3BD3F591A5E", "0002000000000000")) return false;
      if (!do_des_test("0101010101010101", "F356834379D165CD", "0001000000000000")) return false;
      if (!do_des_test("0101010101010101", "2B9F982F20037FA9", "0000800000000000")) return false;
      if (!do_des_test("0101010101010101", "889DE068A16F0BE6", "0000400000000000")) return false;
      if (!do_des_test("0101010101010101", "E19E275D846A1298", "0000200000000000")) return false;
      if (!do_des_test("0101010101010101", "329A8ED523D71AEC", "0000100000000000")) return false;
      if (!do_des_test("0101010101010101", "E7FCE22557D23C97", "0000080000000000")) return false;
      if (!do_des_test("0101010101010101", "12A9F5817FF2D65D", "0000040000000000")) return false;
      if (!do_des_test("0101010101010101", "A484C3AD38DC9C19", "0000020000000000")) return false;
      if (!do_des_test("0101010101010101", "FBE00A8A1EF8AD72", "0000010000000000")) return false;
      if (!do_des_test("0101010101010101", "750D079407521363", "0000008000000000")) return false;
      if (!do_des_test("0101010101010101", "64FEED9C724C2FAF", "0000004000000000")) return false;
      if (!do_des_test("0101010101010101", "F02B263B328E2B60", "0000002000000000")) return false;
      if (!do_des_test("0101010101010101", "9D64555A9A10B852", "0000001000000000")) return false;
      if (!do_des_test("0101010101010101", "D106FF0BED5255D7", "0000000800000000")) return false;
      if (!do_des_test("0101010101010101", "E1652C6B138C64A5", "0000000400000000")) return false;
      if (!do_des_test("0101010101010101", "E428581186EC8F46", "0000000200000000")) return false;
      if (!do_des_test("0101010101010101", "AEB5F5EDE22D1A36", "0000000100000000")) return false;
      if (!do_des_test("0101010101010101", "E943D7568AEC0C5C", "0000000080000000")) return false;
      if (!do_des_test("0101010101010101", "DF98C8276F54B04B", "0000000040000000")) return false;
      if (!do_des_test("0101010101010101", "B160E4680F6C696F", "0000000020000000")) return false;
      if (!do_des_test("0101010101010101", "FA0752B07D9C4AB8", "0000000010000000")) return false;
      if (!do_des_test("0101010101010101", "CA3A2B036DBC8502", "0000000008000000")) return false;
      if (!do_des_test("0101010101010101", "5E0905517BB59BCF", "0000000004000000")) return false;
      if (!do_des_test("0101010101010101", "814EEB3B91D90726", "0000000002000000")) return false;
      if (!do_des_test("0101010101010101", "4D49DB1532919C9F", "0000000001000000")) return false;
      if (!do_des_test("0101010101010101", "25EB5FC3F8CF0621", "0000000000800000")) return false;
      if (!do_des_test("0101010101010101", "AB6A20C0620D1C6F", "0000000000400000")) return false;
      if (!do_des_test("0101010101010101", "79E90DBC98F92CCA", "0000000000200000")) return false;
      if (!do_des_test("0101010101010101", "866ECEDD8072BB0E", "0000000000100000")) return false;
      if (!do_des_test("0101010101010101", "8B54536F2F3E64A8", "0000000000080000")) return false;
      if (!do_des_test("0101010101010101", "EA51D3975595B86B", "0000000000040000")) return false;
      if (!do_des_test("0101010101010101", "CAFFC6AC4542DE31", "0000000000020000")) return false;
      if (!do_des_test("0101010101010101", "8DD45A2DDF90796C", "0000000000010000")) return false;
      if (!do_des_test("0101010101010101", "1029D55E880EC2D0", "0000000000008000")) return false;
      if (!do_des_test("0101010101010101", "5D86CB23639DBEA9", "0000000000004000")) return false;
      if (!do_des_test("0101010101010101", "1D1CA853AE7C0C5F", "0000000000002000")) return false;
      if (!do_des_test("0101010101010101", "CE332329248F3228", "0000000000001000")) return false;
      if (!do_des_test("0101010101010101", "8405D1ABE24FB942", "0000000000000800")) return false;
      if (!do_des_test("0101010101010101", "E643D78090CA4207", "0000000000000400")) return false;
      if (!do_des_test("0101010101010101", "48221B9937748A23", "0000000000000200")) return false;
      if (!do_des_test("0101010101010101", "DD7C0BBD61FAFD54", "0000000000000100")) return false;
      if (!do_des_test("0101010101010101", "2FBC291A570DB5C4", "0000000000000080")) return false;
      if (!do_des_test("0101010101010101", "E07C30D7E4E26E12", "0000000000000040")) return false;
      if (!do_des_test("0101010101010101", "0953E2258E8E90A1", "0000000000000020")) return false;
      if (!do_des_test("0101010101010101", "5B711BC4CEEBF2EE", "0000000000000010")) return false;
      if (!do_des_test("0101010101010101", "CC083F1E6D9E85F6", "0000000000000008")) return false;
      if (!do_des_test("0101010101010101", "D2FD8867D50D2DFE", "0000000000000004")) return false;
      if (!do_des_test("0101010101010101", "06E7EA22CE92708F", "0000000000000002")) return false;
      if (!do_des_test("0101010101010101", "166B40B44ABA4BD6", "0000000000000001")) return false;
    
      if (!do_des_test("1046913489980131", "0000000000000000", "88D55E54F54C97B4")) return false;
      if (!do_des_test("1007103489988020", "0000000000000000", "0C0CC00C83EA48FD")) return false;
      if (!do_des_test("10071034C8980120", "0000000000000000", "83BC8EF3A6570183")) return false;
      if (!do_des_test("1046103489988020", "0000000000000000", "DF725DCAD94EA2E9")) return false;
      if (!do_des_test("1086911519190101", "0000000000000000", "E652B53B550BE8B0")) return false;
      if (!do_des_test("1086911519580101", "0000000000000000", "AF527120C485CBB0")) return false;
      if (!do_des_test("5107B01519580101", "0000000000000000", "0F04CE393DB926D5")) return false;
      if (!do_des_test("1007B01519190101", "0000000000000000", "C9F00FFC74079067")) return false;
      if (!do_des_test("3107915498080101", "0000000000000000", "7CFD82A593252B4E")) return false;
      if (!do_des_test("3107919498080101", "0000000000000000", "CB49A2F9E91363E3")) return false;
      if (!do_des_test("10079115B9080140", "0000000000000000", "00B588BE70D23F56")) return false;
      if (!do_des_test("3107911598080140", "0000000000000000", "406A9A6AB43399AE")) return false;
      if (!do_des_test("1007D01589980101", "0000000000000000", "6CB773611DCA9ADA")) return false;
      if (!do_des_test("9107911589980101", "0000000000000000", "67FD21C17DBB5D70")) return false;
      if (!do_des_test("9107D01589190101", "0000000000000000", "9592CB4110430787")) return false;
      if (!do_des_test("1007D01598980120", "0000000000000000", "A6B7FF68A318DDD3")) return false;
      if (!do_des_test("1007940498190101", "0000000000000000", "4D102196C914CA16")) return false;
      if (!do_des_test("0107910491190401", "0000000000000000", "2DFA9F4573594965")) return false;
      if (!do_des_test("0107910491190101", "0000000000000000", "B46604816C0E0774")) return false;
      if (!do_des_test("0107940491190401", "0000000000000000", "6E7E6221A4F34E87")) return false;
      if (!do_des_test("19079210981A0101", "0000000000000000", "AA85E74643233199")) return false;
      if (!do_des_test("1007911998190801", "0000000000000000", "2E5A19DB4D1962D6")) return false;
      if (!do_des_test("10079119981A0801", "0000000000000000", "23A866A809D30894")) return false;
      if (!do_des_test("1007921098190101", "0000000000000000", "D812D961F017D320")) return false;
      if (!do_des_test("100791159819010B", "0000000000000000", "055605816E58608F")) return false;
      if (!do_des_test("1004801598190101", "0000000000000000", "ABD88E8B1B7716F1")) return false;
      if (!do_des_test("1004801598190102", "0000000000000000", "537AC95BE69DA1E1")) return false;
      if (!do_des_test("1004801598190108", "0000000000000000", "AED0F6AE3C25CDD8")) return false;
      if (!do_des_test("1002911598100104", "0000000000000000", "B3E35A5EE53E7B8D")) return false;
      if (!do_des_test("1002911598190104", "0000000000000000", "61C79C71921A2EF8")) return false;
      if (!do_des_test("1002911598100201", "0000000000000000", "E2F5728F0995013C")) return false;
      if (!do_des_test("1002911698100101", "0000000000000000", "1AEAC39A61F0A464")) return false;
    
      if (!do_des_test("8001010101010101", "0000000000000000", "95A8D72813DAA94D")) return false;
      if (!do_des_test("4001010101010101", "0000000000000000", "0EEC1487DD8C26D5")) return false;
      if (!do_des_test("2001010101010101", "0000000000000000", "7AD16FFB79C45926")) return false;
      if (!do_des_test("1001010101010101", "0000000000000000", "D3746294CA6A6CF3")) return false;
      if (!do_des_test("0801010101010101", "0000000000000000", "809F5F873C1FD761")) return false;
      if (!do_des_test("0401010101010101", "0000000000000000", "C02FAFFEC989D1FC")) return false;
      if (!do_des_test("0201010101010101", "0000000000000000", "4615AA1D33E72F10")) return false;
      if (!do_des_test("0180010101010101", "0000000000000000", "2055123350C00858")) return false;
      if (!do_des_test("0140010101010101", "0000000000000000", "DF3B99D6577397C8")) return false;
      if (!do_des_test("0120010101010101", "0000000000000000", "31FE17369B5288C9")) return false;
      if (!do_des_test("0110010101010101", "0000000000000000", "DFDD3CC64DAE1642")) return false;
      if (!do_des_test("0108010101010101", "0000000000000000", "178C83CE2B399D94")) return false;
      if (!do_des_test("0104010101010101", "0000000000000000", "50F636324A9B7F80")) return false;
      if (!do_des_test("0102010101010101", "0000000000000000", "A8468EE3BC18F06D")) return false;
      if (!do_des_test("0101800101010101", "0000000000000000", "A2DC9E92FD3CDE92")) return false;
      if (!do_des_test("0101400101010101", "0000000000000000", "CAC09F797D031287")) return false;
      if (!do_des_test("0101200101010101", "0000000000000000", "90BA680B22AEB525")) return false;
      if (!do_des_test("0101100101010101", "0000000000000000", "CE7A24F350E280B6")) return false;
      if (!do_des_test("0101080101010101", "0000000000000000", "882BFF0AA01A0B87")) return false;
      if (!do_des_test("0101040101010101", "0000000000000000", "25610288924511C2")) return false;
      if (!do_des_test("0101020101010101", "0000000000000000", "C71516C29C75D170")) return false;
      if (!do_des_test("0101018001010101", "0000000000000000", "5199C29A52C9F059")) return false;
      if (!do_des_test("0101014001010101", "0000000000000000", "C22F0A294A71F29F")) return false;
      if (!do_des_test("0101012001010101", "0000000000000000", "EE371483714C02EA")) return false;
      if (!do_des_test("0101011001010101", "0000000000000000", "A81FBD448F9E522F")) return false;
      if (!do_des_test("0101010801010101", "0000000000000000", "4F644C92E192DFED")) return false;
      if (!do_des_test("0101010401010101", "0000000000000000", "1AFA9A66A6DF92AE")) return false;
      if (!do_des_test("0101010201010101", "0000000000000000", "B3C1CC715CB879D8")) return false;
      if (!do_des_test("0101010180010101", "0000000000000000", "19D032E64AB0BD8B")) return false;
      if (!do_des_test("0101010140010101", "0000000000000000", "3CFAA7A7DC8720DC")) return false;
      if (!do_des_test("0101010120010101", "0000000000000000", "B7265F7F447AC6F3")) return false;
      if (!do_des_test("0101010110010101", "0000000000000000", "9DB73B3C0D163F54")) return false;
      if (!do_des_test("0101010108010101", "0000000000000000", "8181B65BABF4A975")) return false;
      if (!do_des_test("0101010104010101", "0000000000000000", "93C9B64042EAA240")) return false;
      if (!do_des_test("0101010102010101", "0000000000000000", "5570530829705592")) return false;
      if (!do_des_test("0101010101800101", "0000000000000000", "8638809E878787A0")) return false;
      if (!do_des_test("0101010101400101", "0000000000000000", "41B9A79AF79AC208")) return false;
      if (!do_des_test("0101010101200101", "0000000000000000", "7A9BE42F2009A892")) return false;
      if (!do_des_test("0101010101100101", "0000000000000000", "29038D56BA6D2745")) return false;
      if (!do_des_test("0101010101080101", "0000000000000000", "5495C6ABF1E5DF51")) return false;
      if (!do_des_test("0101010101040101", "0000000000000000", "AE13DBD561488933")) return false;
      if (!do_des_test("0101010101020101", "0000000000000000", "024D1FFA8904E389")) return false;
      if (!do_des_test("0101010101018001", "0000000000000000", "D1399712F99BF02E")) return false;
      if (!do_des_test("0101010101014001", "0000000000000000", "14C1D7C1CFFEC79E")) return false;
      if (!do_des_test("0101010101012001", "0000000000000000", "1DE5279DAE3BED6F")) return false;
      if (!do_des_test("0101010101011001", "0000000000000000", "E941A33F85501303")) return false;
      if (!do_des_test("0101010101010801", "0000000000000000", "DA99DBBC9A03F379")) return false;
      if (!do_des_test("0101010101010401", "0000000000000000", "B7FC92F91D8E92E9")) return false;
      if (!do_des_test("0101010101010201", "0000000000000000", "AE8E5CAA3CA04E85")) return false;
      if (!do_des_test("0101010101010180", "0000000000000000", "9CC62DF43B6EED74")) return false;
      if (!do_des_test("0101010101010140", "0000000000000000", "D863DBB5C59A91A0")) return false;
      if (!do_des_test("0101010101010120", "0000000000000000", "A1AB2190545B91D7")) return false;
      if (!do_des_test("0101010101010110", "0000000000000000", "0875041E64C570F7")) return false;
      if (!do_des_test("0101010101010108", "0000000000000000", "5A594528BEBEF1CC")) return false;
      if (!do_des_test("0101010101010104", "0000000000000000", "FCDB3291DE21F0C0")) return false;
      if (!do_des_test("0101010101010102", "0000000000000000", "869EFD7F9F265A09")) return false;
    
      if (!do_des_test("7CA110454A1A6E57", "01A1D6D039776742", "690F5B0D9A26939B")) return false;
      if (!do_des_test("0131D9619DC1376E", "5CD54CA83DEF57DA", "7A389D10354BD271")) return false;
      if (!do_des_test("07A1133E4A0B2686", "0248D43806F67172", "868EBB51CAB4599A")) return false;
      if (!do_des_test("3849674C2602319E", "51454B582DDF440A", "7178876E01F19B2A")) return false;
      if (!do_des_test("04B915BA43FEB5B6", "42FD443059577FA2", "AF37FB421F8C4095")) return false;
      if (!do_des_test("0113B970FD34F2CE", "059B5E0851CF143A", "86A560F10EC6D85B")) return false;
      if (!do_des_test("0170F175468FB5E6", "0756D8E0774761D2", "0CD3DA020021DC09")) return false;
      if (!do_des_test("43297FAD38E373FE", "762514B829BF486A", "EA676B2CB7DB2B7A")) return false;
      if (!do_des_test("07A7137045DA2A16", "3BDD119049372802", "DFD64A815CAF1A0F")) return false;
      if (!do_des_test("04689104C2FD3B2F", "26955F6835AF609A", "5C513C9C4886C088")) return false;
      if (!do_des_test("37D06BB516CB7546", "164D5E404F275232", "0A2AEEAE3FF4AB77")) return false;
      if (!do_des_test("1F08260D1AC2465E", "6B056E18759F5CCA", "EF1BF03E5DFA575A")) return false;
      if (!do_des_test("584023641ABA6176", "004BD6EF09176062", "88BF0DB6D70DEE56")) return false;
      if (!do_des_test("025816164629B007", "480D39006EE762F2", "A1F9915541020B56")) return false;
      if (!do_des_test("49793EBC79B3258F", "437540C8698F3CFA", "6FBF1CAFCFFD0556")) return false;
      if (!do_des_test("4FB05E1515AB73A7", "072D43A077075292", "2F22E49BAB7CA1AC")) return false;
      if (!do_des_test("49E95D6D4CA229BF", "02FE55778117F12A", "5A6B612CC26CCE4A")) return false;
      if (!do_des_test("018310DC409B26D6", "1D9D5C5018F728C2", "5F4C038ED12B2E41")) return false;
      if (!do_des_test("1C587F1C13924FEF", "305532286D6F295A", "63FAC0D034D9F793")) return false;
    
      if (!do_tdes_test("80000000000000000000000000000000", "0000000000000000", "FAFD5084374FCE34")) return false;
      if (!do_tdes_test("40000000000000000000000000000000", "0000000000000000", "60CC37B7B537A1DC")) return false;
      if (!do_tdes_test("20000000000000000000000000000000", "0000000000000000", "BE3E7304FE92C2BC")) return false;
      if (!do_tdes_test("10000000000000000000000000000000", "0000000000000000", "49F9E7A60C406DBF")) return false;
      if (!do_tdes_test("08000000000000000000000000000000", "0000000000000000", "794FE1DC2F80CD38")) return false;
      if (!do_tdes_test("04000000000000000000000000000000", "0000000000000000", "15052BCDF21A1F1E")) return false;
      if (!do_tdes_test("02000000000000000000000000000000", "0000000000000000", "3A830D0BDA044EBB")) return false;
      if (!do_tdes_test("01000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00800000000000000000000000000000", "0000000000000000", "0C1971C6874548E2")) return false;
      if (!do_tdes_test("00400000000000000000000000000000", "0000000000000000", "52C2F3FF100668BC")) return false;
      if (!do_tdes_test("00200000000000000000000000000000", "0000000000000000", "7B1C09D39C205B7B")) return false;
      if (!do_tdes_test("00100000000000000000000000000000", "0000000000000000", "7C940466050ADBAE")) return false;
      if (!do_tdes_test("00080000000000000000000000000000", "0000000000000000", "7B6456C45945CCA3")) return false;
      if (!do_tdes_test("00040000000000000000000000000000", "0000000000000000", "076B2C8A7ADDFE68")) return false;
      if (!do_tdes_test("00020000000000000000000000000000", "0000000000000000", "1885BEE3774FF50B")) return false;
      if (!do_tdes_test("00010000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00008000000000000000000000000000", "0000000000000000", "A286DE6C7ABCE306")) return false;
      if (!do_tdes_test("00004000000000000000000000000000", "0000000000000000", "A19DB1122136903C")) return false;
      if (!do_tdes_test("00002000000000000000000000000000", "0000000000000000", "A77F2F3085DC2D16")) return false;
      if (!do_tdes_test("00001000000000000000000000000000", "0000000000000000", "B39C1E6C3C65E45A")) return false;
      if (!do_tdes_test("00000800000000000000000000000000", "0000000000000000", "E90963FB7F2B1193")) return false;
      if (!do_tdes_test("00000400000000000000000000000000", "0000000000000000", "743C3DBD464ABE66")) return false;
      if (!do_tdes_test("00000200000000000000000000000000", "0000000000000000", "E954174CC0C75C5D")) return false;
      if (!do_tdes_test("00000100000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000080000000000000000000000000", "0000000000000000", "E788FF69D915395A")) return false;
      if (!do_tdes_test("00000040000000000000000000000000", "0000000000000000", "DA518384A7F98F8F")) return false;
      if (!do_tdes_test("00000020000000000000000000000000", "0000000000000000", "71986C565B7A4697")) return false;
      if (!do_tdes_test("00000010000000000000000000000000", "0000000000000000", "5A015BF03B8FF6D2")) return false;
      if (!do_tdes_test("00000008000000000000000000000000", "0000000000000000", "DD311EB7A3202393")) return false;
      if (!do_tdes_test("00000004000000000000000000000000", "0000000000000000", "0DC6A2C01EADE617")) return false;
      if (!do_tdes_test("00000002000000000000000000000000", "0000000000000000", "D1EAE0F689C433DE")) return false;
      if (!do_tdes_test("00000001000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000800000000000000000000000", "0000000000000000", "833803AFBCE49177")) return false;
      if (!do_tdes_test("00000000400000000000000000000000", "0000000000000000", "94EBB684C7C41EF5")) return false;
      if (!do_tdes_test("00000000200000000000000000000000", "0000000000000000", "D42EF0A1B9BC4392")) return false;
      if (!do_tdes_test("00000000100000000000000000000000", "0000000000000000", "9E1D42F406FE0387")) return false;
      if (!do_tdes_test("00000000080000000000000000000000", "0000000000000000", "8DB9EE4A1773C8FE")) return false;
      if (!do_tdes_test("00000000040000000000000000000000", "0000000000000000", "8195C0ED7D066F6B")) return false;
      if (!do_tdes_test("00000000020000000000000000000000", "0000000000000000", "FB3B39E43C76D53D")) return false;
      if (!do_tdes_test("00000000010000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000008000000000000000000000", "0000000000000000", "E21113D2C6870FBE")) return false;
      if (!do_tdes_test("00000000004000000000000000000000", "0000000000000000", "D1CF3B57F6294D0E")) return false;
      if (!do_tdes_test("00000000002000000000000000000000", "0000000000000000", "8990AAB2362CCE0F")) return false;
      if (!do_tdes_test("00000000001000000000000000000000", "0000000000000000", "198774D2FC7A641B")) return false;
      if (!do_tdes_test("00000000000800000000000000000000", "0000000000000000", "F3AC68FDC060AE6E")) return false;
      if (!do_tdes_test("00000000000400000000000000000000", "0000000000000000", "A854715C1EE8B311")) return false;
      if (!do_tdes_test("00000000000200000000000000000000", "0000000000000000", "D140934E0D5171DB")) return false;
      if (!do_tdes_test("00000000000100000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000080000000000000000000", "0000000000000000", "F3B2D1D19B852861")) return false;
      if (!do_tdes_test("00000000000040000000000000000000", "0000000000000000", "EE8DC918A74545F1")) return false;
      if (!do_tdes_test("00000000000020000000000000000000", "0000000000000000", "99B2175DCE3D348C")) return false;
      if (!do_tdes_test("00000000000010000000000000000000", "0000000000000000", "73AE9A4A6376637E")) return false;
      if (!do_tdes_test("00000000000008000000000000000000", "0000000000000000", "C55C05072C072CBE")) return false;
      if (!do_tdes_test("00000000000004000000000000000000", "0000000000000000", "FB4808530D49FFD3")) return false;
      if (!do_tdes_test("00000000000002000000000000000000", "0000000000000000", "3C1B66BD5170F2A1")) return false;
      if (!do_tdes_test("00000000000001000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000800000000000000000", "0000000000000000", "A38DC58A5AEF3CAA")) return false;
      if (!do_tdes_test("00000000000000400000000000000000", "0000000000000000", "4F29AB3449FBA969")) return false;
      if (!do_tdes_test("00000000000000200000000000000000", "0000000000000000", "F75ACF1692C115D2")) return false;
      if (!do_tdes_test("00000000000000100000000000000000", "0000000000000000", "5A448A95522AF894")) return false;
      if (!do_tdes_test("00000000000000080000000000000000", "0000000000000000", "FEEA19D1125CEB53")) return false;
      if (!do_tdes_test("00000000000000040000000000000000", "0000000000000000", "7A7907DEB712DD81")) return false;
      if (!do_tdes_test("00000000000000020000000000000000", "0000000000000000", "41792F90E798B8E2")) return false;
      if (!do_tdes_test("00000000000000010000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000008000000000000000", "0000000000000000", "C2A4DD96151453C2")) return false;
      if (!do_tdes_test("00000000000000004000000000000000", "0000000000000000", "5E87809F6B8A7ED5")) return false;
      if (!do_tdes_test("00000000000000002000000000000000", "0000000000000000", "81B838A1E9CD59B3")) return false;
      if (!do_tdes_test("00000000000000001000000000000000", "0000000000000000", "DED028F0C1F5A774")) return false;
      if (!do_tdes_test("00000000000000000800000000000000", "0000000000000000", "48C983815809FC87")) return false;
      if (!do_tdes_test("00000000000000000400000000000000", "0000000000000000", "C1A75845F22BE951")) return false;
      if (!do_tdes_test("00000000000000000200000000000000", "0000000000000000", "C60F823E8E994489")) return false;
      if (!do_tdes_test("00000000000000000100000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000080000000000000", "0000000000000000", "709F8FCB044172FE")) return false;
      if (!do_tdes_test("00000000000000000040000000000000", "0000000000000000", "26BC2DE634BFFFD4")) return false;
      if (!do_tdes_test("00000000000000000020000000000000", "0000000000000000", "D98126355C2E03E6")) return false;
      if (!do_tdes_test("00000000000000000010000000000000", "0000000000000000", "49AAA91B49345137")) return false;
      if (!do_tdes_test("00000000000000000008000000000000", "0000000000000000", "A59854DCE009126D")) return false;
      if (!do_tdes_test("00000000000000000004000000000000", "0000000000000000", "21C46B9FDE5CD36B")) return false;
      if (!do_tdes_test("00000000000000000002000000000000", "0000000000000000", "DEB4AE36E07BC053")) return false;
      if (!do_tdes_test("00000000000000000001000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000800000000000", "0000000000000000", "D47ADF8B94CACA7A")) return false;
      if (!do_tdes_test("00000000000000000000400000000000", "0000000000000000", "D26D9656F91A1EE2")) return false;
      if (!do_tdes_test("00000000000000000000200000000000", "0000000000000000", "EE31B8E767C9B337")) return false;
      if (!do_tdes_test("00000000000000000000100000000000", "0000000000000000", "D19BA61DD59CE9A1")) return false;
      if (!do_tdes_test("00000000000000000000080000000000", "0000000000000000", "482863934D17804B")) return false;
      if (!do_tdes_test("00000000000000000000040000000000", "0000000000000000", "78C8CBCAC3B7FD35")) return false;
      if (!do_tdes_test("00000000000000000000020000000000", "0000000000000000", "7B8B051E6C8AA8B6")) return false;
      if (!do_tdes_test("00000000000000000000010000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000008000000000", "0000000000000000", "8CCFCD2418E85750")) return false;
      if (!do_tdes_test("00000000000000000000004000000000", "0000000000000000", "E74CA11808ED17A3")) return false;
      if (!do_tdes_test("00000000000000000000002000000000", "0000000000000000", "0A634C7A69897F35")) return false;
      if (!do_tdes_test("00000000000000000000001000000000", "0000000000000000", "6C2C0F27E973CE29")) return false;
      if (!do_tdes_test("00000000000000000000000800000000", "0000000000000000", "AD5F11ED913E918C")) return false;
      if (!do_tdes_test("00000000000000000000000400000000", "0000000000000000", "3CE4B119BC1FC701")) return false;
      if (!do_tdes_test("00000000000000000000000200000000", "0000000000000000", "7E6C8995AA52D298")) return false;
      if (!do_tdes_test("00000000000000000000000100000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000000080000000", "0000000000000000", "A9FE6341C8621918")) return false;
      if (!do_tdes_test("00000000000000000000000040000000", "0000000000000000", "CE99FD5D50B22CEF")) return false;
      if (!do_tdes_test("00000000000000000000000020000000", "0000000000000000", "83E55C4A19ABCB56")) return false;
      if (!do_tdes_test("00000000000000000000000010000000", "0000000000000000", "96E6A993443B9DD4")) return false;
      if (!do_tdes_test("00000000000000000000000008000000", "0000000000000000", "6781B65D74A6B9FB")) return false;
      if (!do_tdes_test("00000000000000000000000004000000", "0000000000000000", "D9EF04E272D1A78A")) return false;
      if (!do_tdes_test("00000000000000000000000002000000", "0000000000000000", "AC8B09EC3153D57B")) return false;
      if (!do_tdes_test("00000000000000000000000001000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000000000800000", "0000000000000000", "60B4B8E3A8F5CBEC")) return false;
      if (!do_tdes_test("00000000000000000000000000400000", "0000000000000000", "A5AB6F6EB66057A9")) return false;
      if (!do_tdes_test("00000000000000000000000000200000", "0000000000000000", "FF7B0E870FB1FD0B")) return false;
      if (!do_tdes_test("00000000000000000000000000100000", "0000000000000000", "7497A098AA651D00")) return false;
      if (!do_tdes_test("00000000000000000000000000080000", "0000000000000000", "270A943BEABEA8EC")) return false;
      if (!do_tdes_test("00000000000000000000000000040000", "0000000000000000", "67DB327ED5DF89E3")) return false;
      if (!do_tdes_test("00000000000000000000000000020000", "0000000000000000", "4871C3B7436121DE")) return false;
      if (!do_tdes_test("00000000000000000000000000010000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000000000008000", "0000000000000000", "41BBC8EF36654838")) return false;
      if (!do_tdes_test("00000000000000000000000000004000", "0000000000000000", "FCBD166CA0EA87E2")) return false;
      if (!do_tdes_test("00000000000000000000000000002000", "0000000000000000", "9DFFC6EE9751B5CF")) return false;
      if (!do_tdes_test("00000000000000000000000000001000", "0000000000000000", "C01B7878EBCE8DD3")) return false;
      if (!do_tdes_test("00000000000000000000000000000800", "0000000000000000", "357E5A4DC162D715")) return false;
      if (!do_tdes_test("00000000000000000000000000000400", "0000000000000000", "268F93CAEB248E2E")) return false;
      if (!do_tdes_test("00000000000000000000000000000200", "0000000000000000", "A5D4174744B84E7D")) return false;
      if (!do_tdes_test("00000000000000000000000000000100", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000000000000080", "0000000000000000", "46F5E7077CB869A8")) return false;
      if (!do_tdes_test("00000000000000000000000000000040", "0000000000000000", "502CD2BF4FC0B793")) return false;
      if (!do_tdes_test("00000000000000000000000000000020", "0000000000000000", "C0278007230589E4")) return false;
      if (!do_tdes_test("00000000000000000000000000000010", "0000000000000000", "52710C55818FAF52")) return false;
      if (!do_tdes_test("00000000000000000000000000000008", "0000000000000000", "DF4A77123610F2B1")) return false;
      if (!do_tdes_test("00000000000000000000000000000004", "0000000000000000", "EF840B00DA448234")) return false;
      if (!do_tdes_test("00000000000000000000000000000002", "0000000000000000", "FFCCC32A699CB7C5")) return false;
      if (!do_tdes_test("00000000000000000000000000000001", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "8000000000000000", "95F8A5E5DD31D900")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "4000000000000000", "DD7F121CA5015619")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "2000000000000000", "2E8653104F3834EA")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "1000000000000000", "4BD388FF6CD81D4F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0800000000000000", "20B9E767B2FB1456")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0400000000000000", "55579380D77138EF")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0200000000000000", "6CC5DEFAAF04512F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0100000000000000", "0D9F279BA5D87260")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0080000000000000", "D9031B0271BD5A0A")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0040000000000000", "424250B37C3DD951")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0020000000000000", "B8061B7ECD9A21E5")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0010000000000000", "F15D0F286B65BD28")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0008000000000000", "ADD0CC8D6E5DEBA1")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0004000000000000", "E6D5F82752AD63D1")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0002000000000000", "ECBFE3BD3F591A5E")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0001000000000000", "F356834379D165CD")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000800000000000", "2B9F982F20037FA9")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000400000000000", "889DE068A16F0BE6")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000200000000000", "E19E275D846A1298")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000100000000000", "329A8ED523D71AEC")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000080000000000", "E7FCE22557D23C97")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000040000000000", "12A9F5817FF2D65D")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000020000000000", "A484C3AD38DC9C19")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000010000000000", "FBE00A8A1EF8AD72")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000008000000000", "750D079407521363")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000004000000000", "64FEED9C724C2FAF")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000002000000000", "F02B263B328E2B60")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000001000000000", "9D64555A9A10B852")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000800000000", "D106FF0BED5255D7")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000400000000", "E1652C6B138C64A5")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000200000000", "E428581186EC8F46")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000100000000", "AEB5F5EDE22D1A36")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000080000000", "E943D7568AEC0C5C")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000040000000", "DF98C8276F54B04B")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000020000000", "B160E4680F6C696F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000010000000", "FA0752B07D9C4AB8")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000008000000", "CA3A2B036DBC8502")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000004000000", "5E0905517BB59BCF")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000002000000", "814EEB3B91D90726")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000001000000", "4D49DB1532919C9F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000800000", "25EB5FC3F8CF0621")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000400000", "AB6A20C0620D1C6F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000200000", "79E90DBC98F92CCA")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000100000", "866ECEDD8072BB0E")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000080000", "8B54536F2F3E64A8")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000040000", "EA51D3975595B86B")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000020000", "CAFFC6AC4542DE31")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000010000", "8DD45A2DDF90796C")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000008000", "1029D55E880EC2D0")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000004000", "5D86CB23639DBEA9")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000002000", "1D1CA853AE7C0C5F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000001000", "CE332329248F3228")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000800", "8405D1ABE24FB942")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000400", "E643D78090CA4207")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000200", "48221B9937748A23")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000100", "DD7C0BBD61FAFD54")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000080", "2FBC291A570DB5C4")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000040", "E07C30D7E4E26E12")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000020", "0953E2258E8E90A1")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000010", "5B711BC4CEEBF2EE")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000008", "CC083F1E6D9E85F6")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000004", "D2FD8867D50D2DFE")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000002", "06E7EA22CE92708F")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000001", "166B40B44ABA4BD6")) return false;
      if (!do_tdes_test("00000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("01010101010101010101010101010101", "0101010101010101", "994D4DC157B96C52")) return false;
      if (!do_tdes_test("02020202020202020202020202020202", "0202020202020202", "E127C2B61D98E6E2")) return false;
      if (!do_tdes_test("03030303030303030303030303030303", "0303030303030303", "984C91D78A269CE3")) return false;
      if (!do_tdes_test("04040404040404040404040404040404", "0404040404040404", "1F4570BB77550683")) return false;
      if (!do_tdes_test("05050505050505050505050505050505", "0505050505050505", "3990ABF98D672B16")) return false;
      if (!do_tdes_test("06060606060606060606060606060606", "0606060606060606", "3F5150BBA081D585")) return false;
      if (!do_tdes_test("07070707070707070707070707070707", "0707070707070707", "C65242248C9CF6F2")) return false;
      if (!do_tdes_test("08080808080808080808080808080808", "0808080808080808", "10772D40FAD24257")) return false;
      if (!do_tdes_test("09090909090909090909090909090909", "0909090909090909", "F0139440647A6E7B")) return false;
      if (!do_tdes_test("0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A", "0A0A0A0A0A0A0A0A", "0A288603044D740C")) return false;
      if (!do_tdes_test("0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B", "0B0B0B0B0B0B0B0B", "6359916942F7438F")) return false;
      if (!do_tdes_test("0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C", "0C0C0C0C0C0C0C0C", "934316AE443CF08B")) return false;
      if (!do_tdes_test("0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D", "0D0D0D0D0D0D0D0D", "E3F56D7F1130A2B7")) return false;
      if (!do_tdes_test("0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E", "0E0E0E0E0E0E0E0E", "A2E4705087C6B6B4")) return false;
      if (!do_tdes_test("0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F", "0F0F0F0F0F0F0F0F", "D5D76E09A447E8C3")) return false;
      if (!do_tdes_test("10101010101010101010101010101010", "1010101010101010", "DD7515F2BFC17F85")) return false;
      if (!do_tdes_test("11111111111111111111111111111111", "1111111111111111", "F40379AB9E0EC533")) return false;
      if (!do_tdes_test("12121212121212121212121212121212", "1212121212121212", "96CD27784D1563E5")) return false;
      if (!do_tdes_test("13131313131313131313131313131313", "1313131313131313", "2911CF5E94D33FE1")) return false;
      if (!do_tdes_test("14141414141414141414141414141414", "1414141414141414", "377B7F7CA3E5BBB3")) return false;
      if (!do_tdes_test("15151515151515151515151515151515", "1515151515151515", "701AA63832905A92")) return false;
      if (!do_tdes_test("16161616161616161616161616161616", "1616161616161616", "2006E716C4252D6D")) return false;
      if (!do_tdes_test("17171717171717171717171717171717", "1717171717171717", "452C1197422469F8")) return false;
      if (!do_tdes_test("18181818181818181818181818181818", "1818181818181818", "C33FD1EB49CB64DA")) return false;
      if (!do_tdes_test("19191919191919191919191919191919", "1919191919191919", "7572278F364EB50D")) return false;
      if (!do_tdes_test("1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A", "1A1A1A1A1A1A1A1A", "69E51488403EF4C3")) return false;
      if (!do_tdes_test("1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B", "1B1B1B1B1B1B1B1B", "FF847E0ADF192825")) return false;
      if (!do_tdes_test("1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C", "1C1C1C1C1C1C1C1C", "521B7FB3B41BB791")) return false;
      if (!do_tdes_test("1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D", "1D1D1D1D1D1D1D1D", "26059A6A0F3F6B35")) return false;
      if (!do_tdes_test("1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E", "1E1E1E1E1E1E1E1E", "F24A8D2231C77538")) return false;
      if (!do_tdes_test("1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F", "1F1F1F1F1F1F1F1F", "4FD96EC0D3304EF6")) return false;
      if (!do_tdes_test("20202020202020202020202020202020", "2020202020202020", "18A9D580A900B699")) return false;
      if (!do_tdes_test("21212121212121212121212121212121", "2121212121212121", "88586E1D755B9B5A")) return false;
      if (!do_tdes_test("22222222222222222222222222222222", "2222222222222222", "0F8ADFFB11DC2784")) return false;
      if (!do_tdes_test("23232323232323232323232323232323", "2323232323232323", "2F30446C8312404A")) return false;
      if (!do_tdes_test("24242424242424242424242424242424", "2424242424242424", "0BA03D9E6C196511")) return false;
      if (!do_tdes_test("25252525252525252525252525252525", "2525252525252525", "3E55E997611E4B7D")) return false;
      if (!do_tdes_test("26262626262626262626262626262626", "2626262626262626", "B2522FB5F158F0DF")) return false;
      if (!do_tdes_test("27272727272727272727272727272727", "2727272727272727", "2109425935406AB8")) return false;
      if (!do_tdes_test("28282828282828282828282828282828", "2828282828282828", "11A16028F310FF16")) return false;
      if (!do_tdes_test("29292929292929292929292929292929", "2929292929292929", "73F0C45F379FE67F")) return false;
      if (!do_tdes_test("2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A", "2A2A2A2A2A2A2A2A", "DCAD4338F7523816")) return false;
      if (!do_tdes_test("2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B", "2B2B2B2B2B2B2B2B", "B81634C1CEAB298C")) return false;
      if (!do_tdes_test("2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C", "2C2C2C2C2C2C2C2C", "DD2CCB29B6C4C349")) return false;
      if (!do_tdes_test("2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D", "2D2D2D2D2D2D2D2D", "7D07A77A2ABD50A7")) return false;
      if (!do_tdes_test("2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E", "2E2E2E2E2E2E2E2E", "30C1B0C1FD91D371")) return false;
      if (!do_tdes_test("2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F", "2F2F2F2F2F2F2F2F", "C4427B31AC61973B")) return false;
      if (!do_tdes_test("30303030303030303030303030303030", "3030303030303030", "F47BB46273B15EB5")) return false;
      if (!do_tdes_test("31313131313131313131313131313131", "3131313131313131", "655EA628CF62585F")) return false;
      if (!do_tdes_test("32323232323232323232323232323232", "3232323232323232", "AC978C247863388F")) return false;
      if (!do_tdes_test("33333333333333333333333333333333", "3333333333333333", "0432ED386F2DE328")) return false;
      if (!do_tdes_test("34343434343434343434343434343434", "3434343434343434", "D254014CB986B3C2")) return false;
      if (!do_tdes_test("35353535353535353535353535353535", "3535353535353535", "B256E34BEDB49801")) return false;
      if (!do_tdes_test("36363636363636363636363636363636", "3636363636363636", "37F8759EB77E7BFC")) return false;
      if (!do_tdes_test("37373737373737373737373737373737", "3737373737373737", "5013CA4F62C9CEA0")) return false;
      if (!do_tdes_test("38383838383838383838383838383838", "3838383838383838", "8940F7B3EACA5939")) return false;
      if (!do_tdes_test("39393939393939393939393939393939", "3939393939393939", "E22B19A55086774B")) return false;
      if (!do_tdes_test("3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A", "3A3A3A3A3A3A3A3A", "B04A2AAC925ABB0B")) return false;
      if (!do_tdes_test("3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B", "3B3B3B3B3B3B3B3B", "8D250D58361597FC")) return false;
      if (!do_tdes_test("3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C", "3C3C3C3C3C3C3C3C", "51F0114FB6A6CD37")) return false;
      if (!do_tdes_test("3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D", "3D3D3D3D3D3D3D3D", "9D0BB4DB830ECB73")) return false;
      if (!do_tdes_test("3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E", "3E3E3E3E3E3E3E3E", "E96089D6368F3E1A")) return false;
      if (!do_tdes_test("3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F", "3F3F3F3F3F3F3F3F", "5C4CA877A4E1E92D")) return false;
      if (!do_tdes_test("40404040404040404040404040404040", "4040404040404040", "6D55DDBC8DEA95FF")) return false;
      if (!do_tdes_test("41414141414141414141414141414141", "4141414141414141", "19DF84AC95551003")) return false;
      if (!do_tdes_test("42424242424242424242424242424242", "4242424242424242", "724E7332696D08A7")) return false;
      if (!do_tdes_test("43434343434343434343434343434343", "4343434343434343", "B91810B8CDC58FE2")) return false;
      if (!do_tdes_test("44444444444444444444444444444444", "4444444444444444", "06E23526EDCCD0C4")) return false;
      if (!do_tdes_test("45454545454545454545454545454545", "4545454545454545", "EF52491D5468D441")) return false;
      if (!do_tdes_test("46464646464646464646464646464646", "4646464646464646", "48019C59E39B90C5")) return false;
      if (!do_tdes_test("47474747474747474747474747474747", "4747474747474747", "0544083FB902D8C0")) return false;
      if (!do_tdes_test("48484848484848484848484848484848", "4848484848484848", "63B15CADA668CE12")) return false;
      if (!do_tdes_test("49494949494949494949494949494949", "4949494949494949", "EACC0C1264171071")) return false;
      if (!do_tdes_test("4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A", "4A4A4A4A4A4A4A4A", "9D2B8C0AC605F274")) return false;
      if (!do_tdes_test("4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B", "4B4B4B4B4B4B4B4B", "C90F2F4C98A8FB2A")) return false;
      if (!do_tdes_test("4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C", "4C4C4C4C4C4C4C4C", "03481B4828FD1D04")) return false;
      if (!do_tdes_test("4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D", "4D4D4D4D4D4D4D4D", "C78FC45A1DCEA2E2")) return false;
      if (!do_tdes_test("4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E", "4E4E4E4E4E4E4E4E", "DB96D88C3460D801")) return false;
      if (!do_tdes_test("4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F", "4F4F4F4F4F4F4F4F", "6C69E720F5105518")) return false;
      if (!do_tdes_test("50505050505050505050505050505050", "5050505050505050", "0D262E418BC893F3")) return false;
      if (!do_tdes_test("51515151515151515151515151515151", "5151515151515151", "6AD84FD7848A0A5C")) return false;
      if (!do_tdes_test("52525252525252525252525252525252", "5252525252525252", "C365CB35B34B6114")) return false;
      if (!do_tdes_test("53535353535353535353535353535353", "5353535353535353", "1155392E877F42A9")) return false;
      if (!do_tdes_test("54545454545454545454545454545454", "5454545454545454", "531BE5F9405DA715")) return false;
      if (!do_tdes_test("55555555555555555555555555555555", "5555555555555555", "3BCDD41E6165A5E8")) return false;
      if (!do_tdes_test("56565656565656565656565656565656", "5656565656565656", "2B1FF5610A19270C")) return false;
      if (!do_tdes_test("57575757575757575757575757575757", "5757575757575757", "D90772CF3F047CFD")) return false;
      if (!do_tdes_test("58585858585858585858585858585858", "5858585858585858", "1BEA27FFB72457B7")) return false;
      if (!do_tdes_test("59595959595959595959595959595959", "5959595959595959", "85C3E0C429F34C27")) return false;
      if (!do_tdes_test("5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A", "5A5A5A5A5A5A5A5A", "F9038021E37C7618")) return false;
      if (!do_tdes_test("5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B", "5B5B5B5B5B5B5B5B", "35BC6FF838DBA32F")) return false;
      if (!do_tdes_test("5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C", "5C5C5C5C5C5C5C5C", "4927ACC8CE45ECE7")) return false;
      if (!do_tdes_test("5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D", "5D5D5D5D5D5D5D5D", "E812EE6E3572985C")) return false;
      if (!do_tdes_test("5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E", "5E5E5E5E5E5E5E5E", "9BB93A89627BF65F")) return false;
      if (!do_tdes_test("5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F", "5F5F5F5F5F5F5F5F", "EF12476884CB74CA")) return false;
      if (!do_tdes_test("60606060606060606060606060606060", "6060606060606060", "1BF17E00C09E7CBF")) return false;
      if (!do_tdes_test("61616161616161616161616161616161", "6161616161616161", "29932350C098DB5D")) return false;
      if (!do_tdes_test("62626262626262626262626262626262", "6262626262626262", "B476E6499842AC54")) return false;
      if (!do_tdes_test("63636363636363636363636363636363", "6363636363636363", "5C662C29C1E96056")) return false;
      if (!do_tdes_test("64646464646464646464646464646464", "6464646464646464", "3AF1703D76442789")) return false;
      if (!do_tdes_test("65656565656565656565656565656565", "6565656565656565", "86405D9B425A8C8C")) return false;
      if (!do_tdes_test("66666666666666666666666666666666", "6666666666666666", "EBBF4810619C2C55")) return false;
      if (!do_tdes_test("67676767676767676767676767676767", "6767676767676767", "F8D1CD7367B21B5D")) return false;
      if (!do_tdes_test("68686868686868686868686868686868", "6868686868686868", "9EE703142BF8D7E2")) return false;
      if (!do_tdes_test("69696969696969696969696969696969", "6969696969696969", "5FDFFFC3AAAB0CB3")) return false;
      if (!do_tdes_test("6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A", "6A6A6A6A6A6A6A6A", "26C940AB13574231")) return false;
      if (!do_tdes_test("6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B", "6B6B6B6B6B6B6B6B", "1E2DC77E36A84693")) return false;
      if (!do_tdes_test("6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C", "6C6C6C6C6C6C6C6C", "0F4FF4D9BC7E2244")) return false;
      if (!do_tdes_test("6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D", "6D6D6D6D6D6D6D6D", "A4C9A0D04D3280CD")) return false;
      if (!do_tdes_test("6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E", "6E6E6E6E6E6E6E6E", "9FAF2C96FE84919D")) return false;
      if (!do_tdes_test("6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F", "6F6F6F6F6F6F6F6F", "115DBC965E6096C8")) return false;
      if (!do_tdes_test("70707070707070707070707070707070", "7070707070707070", "AF531E9520994017")) return false;
      if (!do_tdes_test("71717171717171717171717171717171", "7171717171717171", "B971ADE70E5C89EE")) return false;
      if (!do_tdes_test("72727272727272727272727272727272", "7272727272727272", "415D81C86AF9C376")) return false;
      if (!do_tdes_test("73737373737373737373737373737373", "7373737373737373", "8DFB864FDB3C6811")) return false;
      if (!do_tdes_test("74747474747474747474747474747474", "7474747474747474", "10B1C170E3398F91")) return false;
      if (!do_tdes_test("75757575757575757575757575757575", "7575757575757575", "CFEF7A1C0218DB1E")) return false;
      if (!do_tdes_test("76767676767676767676767676767676", "7676767676767676", "DBAC30A2A40B1B9C")) return false;
      if (!do_tdes_test("77777777777777777777777777777777", "7777777777777777", "89D3BF37052162E9")) return false;
      if (!do_tdes_test("78787878787878787878787878787878", "7878787878787878", "80D9230BDAEB67DC")) return false;
      if (!do_tdes_test("79797979797979797979797979797979", "7979797979797979", "3440911019AD68D7")) return false;
      if (!do_tdes_test("7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A", "7A7A7A7A7A7A7A7A", "9626FE57596E199E")) return false;
      if (!do_tdes_test("7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B", "7B7B7B7B7B7B7B7B", "DEA0B796624BB5BA")) return false;
      if (!do_tdes_test("7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C", "7C7C7C7C7C7C7C7C", "E9E40542BDDB3E9D")) return false;
      if (!do_tdes_test("7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D", "7D7D7D7D7D7D7D7D", "8AD99914B354B911")) return false;
      if (!do_tdes_test("7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E", "7E7E7E7E7E7E7E7E", "6F85B98DD12CB13B")) return false;
      if (!do_tdes_test("7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F", "7F7F7F7F7F7F7F7F", "10130DA3C3A23924")) return false;
      if (!do_tdes_test("80808080808080808080808080808080", "8080808080808080", "EFECF25C3C5DC6DB")) return false;
      if (!do_tdes_test("81818181818181818181818181818181", "8181818181818181", "907A46722ED34EC4")) return false;
      if (!do_tdes_test("82828282828282828282828282828282", "8282828282828282", "752666EB4CAB46EE")) return false;
      if (!do_tdes_test("83838383838383838383838383838383", "8383838383838383", "161BFABD4224C162")) return false;
      if (!do_tdes_test("84848484848484848484848484848484", "8484848484848484", "215F48699DB44A45")) return false;
      if (!do_tdes_test("85858585858585858585858585858585", "8585858585858585", "69D901A8A691E661")) return false;
      if (!do_tdes_test("86868686868686868686868686868686", "8686868686868686", "CBBF6EEFE6529728")) return false;
      if (!do_tdes_test("87878787878787878787878787878787", "8787878787878787", "7F26DCF425149823")) return false;
      if (!do_tdes_test("88888888888888888888888888888888", "8888888888888888", "762C40C8FADE9D16")) return false;
      if (!do_tdes_test("89898989898989898989898989898989", "8989898989898989", "2453CF5D5BF4E463")) return false;
      if (!do_tdes_test("8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A", "8A8A8A8A8A8A8A8A", "301085E3FDE724E1")) return false;
      if (!do_tdes_test("8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B", "8B8B8B8B8B8B8B8B", "EF4E3E8F1CC6706E")) return false;
      if (!do_tdes_test("8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C", "8C8C8C8C8C8C8C8C", "720479B024C397EE")) return false;
      if (!do_tdes_test("8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D", "8D8D8D8D8D8D8D8D", "BEA27E3795063C89")) return false;
      if (!do_tdes_test("8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E", "8E8E8E8E8E8E8E8E", "468E5218F1A37611")) return false;
      if (!do_tdes_test("8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F", "8F8F8F8F8F8F8F8F", "50ACE16ADF66BFE8")) return false;
      if (!do_tdes_test("90909090909090909090909090909090", "9090909090909090", "EEA24369A19F6937")) return false;
      if (!do_tdes_test("91919191919191919191919191919191", "9191919191919191", "6050D369017B6E62")) return false;
      if (!do_tdes_test("92929292929292929292929292929292", "9292929292929292", "5B365F2FB2CD7F32")) return false;
      if (!do_tdes_test("93939393939393939393939393939393", "9393939393939393", "F0B00B264381DDBB")) return false;
      if (!do_tdes_test("94949494949494949494949494949494", "9494949494949494", "E1D23881C957B96C")) return false;
      if (!do_tdes_test("95959595959595959595959595959595", "9595959595959595", "D936BF54ECA8BDCE")) return false;
      if (!do_tdes_test("96969696969696969696969696969696", "9696969696969696", "A020003C5554F34C")) return false;
      if (!do_tdes_test("97979797979797979797979797979797", "9797979797979797", "6118FCEBD407281D")) return false;
      if (!do_tdes_test("98989898989898989898989898989898", "9898989898989898", "072E328C984DE4A2")) return false;
      if (!do_tdes_test("99999999999999999999999999999999", "9999999999999999", "1440B7EF9E63D3AA")) return false;
      if (!do_tdes_test("9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A", "9A9A9A9A9A9A9A9A", "79BFA264BDA57373")) return false;
      if (!do_tdes_test("9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B", "9B9B9B9B9B9B9B9B", "C50E8FC289BBD876")) return false;
      if (!do_tdes_test("9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C", "9C9C9C9C9C9C9C9C", "A399D3D63E169FA9")) return false;
      if (!do_tdes_test("9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D", "9D9D9D9D9D9D9D9D", "4B8919B667BD53AB")) return false;
      if (!do_tdes_test("9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E", "9E9E9E9E9E9E9E9E", "D66CDCAF3F6724A2")) return false;
      if (!do_tdes_test("9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F", "9F9F9F9F9F9F9F9F", "E40E81FF3F618340")) return false;
      if (!do_tdes_test("A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0", "A0A0A0A0A0A0A0A0", "10EDB8977B348B35")) return false;
      if (!do_tdes_test("A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1", "A1A1A1A1A1A1A1A1", "6446C5769D8409A0")) return false;
      if (!do_tdes_test("A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2", "A2A2A2A2A2A2A2A2", "17ED1191CA8D67A3")) return false;
      if (!do_tdes_test("A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3", "A3A3A3A3A3A3A3A3", "B6D8533731BA1318")) return false;
      if (!do_tdes_test("A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4", "A4A4A4A4A4A4A4A4", "CA439007C7245CD0")) return false;
      if (!do_tdes_test("A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5", "A5A5A5A5A5A5A5A5", "06FC7FDE1C8389E7")) return false;
      if (!do_tdes_test("A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6", "A6A6A6A6A6A6A6A6", "7A3C1F3BD60CB3D8")) return false;
      if (!do_tdes_test("A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7", "A7A7A7A7A7A7A7A7", "E415D80048DBA848")) return false;
      if (!do_tdes_test("A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8", "A8A8A8A8A8A8A8A8", "26F88D30C0FB8302")) return false;
      if (!do_tdes_test("A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9", "A9A9A9A9A9A9A9A9", "D4E00A9EF5E6D8F3")) return false;
      if (!do_tdes_test("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA", "C4322BE19E9A5A17")) return false;
      if (!do_tdes_test("ABABABABABABABABABABABABABABABAB", "ABABABABABABABAB", "ACE41A06BFA258EA")) return false;
      if (!do_tdes_test("ACACACACACACACACACACACACACACACAC", "ACACACACACACACAC", "EEAAC6D17880BD56")) return false;
      if (!do_tdes_test("ADADADADADADADADADADADADADADADAD", "ADADADADADADADAD", "3C9A34CA4CB49EEB")) return false;
      if (!do_tdes_test("AEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAE", "AEAEAEAEAEAEAEAE", "9527B0287B75F5A3")) return false;
      if (!do_tdes_test("AFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAF", "AFAFAFAFAFAFAFAF", "F2D9D1BE74376C0C")) return false;
      if (!do_tdes_test("B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0", "B0B0B0B0B0B0B0B0", "939618DF0AEFAAE7")) return false;
      if (!do_tdes_test("B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1", "B1B1B1B1B1B1B1B1", "24692773CB9F27FE")) return false;
      if (!do_tdes_test("B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2", "B2B2B2B2B2B2B2B2", "38703BA5E2315D1D")) return false;
      if (!do_tdes_test("B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3", "B3B3B3B3B3B3B3B3", "FCB7E4B7D702E2FB")) return false;
      if (!do_tdes_test("B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4", "B4B4B4B4B4B4B4B4", "36F0D0B3675704D5")) return false;
      if (!do_tdes_test("B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5", "B5B5B5B5B5B5B5B5", "62D473F539FA0D8B")) return false;
      if (!do_tdes_test("B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6", "B6B6B6B6B6B6B6B6", "1533F3ED9BE8EF8E")) return false;
      if (!do_tdes_test("B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7", "B7B7B7B7B7B7B7B7", "9C4EA352599731ED")) return false;
      if (!do_tdes_test("B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8", "B8B8B8B8B8B8B8B8", "FABBF7C046FD273F")) return false;
      if (!do_tdes_test("B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9", "B9B9B9B9B9B9B9B9", "B7FE63A61C646F3A")) return false;
      if (!do_tdes_test("BABABABABABABABABABABABABABABABA", "BABABABABABABABA", "10ADB6E2AB972BBE")) return false;
      if (!do_tdes_test("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "BBBBBBBBBBBBBBBB", "F91DCAD912332F3B")) return false;
      if (!do_tdes_test("BCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBC", "BCBCBCBCBCBCBCBC", "46E7EF47323A701D")) return false;
      if (!do_tdes_test("BDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBD", "BDBDBDBDBDBDBDBD", "8DB18CCD9692F758")) return false;
      if (!do_tdes_test("BEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBE", "BEBEBEBEBEBEBEBE", "E6207B536AAAEFFC")) return false;
      if (!do_tdes_test("BFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBF", "BFBFBFBFBFBFBFBF", "92AA224372156A00")) return false;
      if (!do_tdes_test("C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0", "C0C0C0C0C0C0C0C0", "A3B357885B1E16D2")) return false;
      if (!do_tdes_test("C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1", "C1C1C1C1C1C1C1C1", "169F7629C970C1E5")) return false;
      if (!do_tdes_test("C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2", "C2C2C2C2C2C2C2C2", "62F44B247CF1348C")) return false;
      if (!do_tdes_test("C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3", "C3C3C3C3C3C3C3C3", "AE0FEEB0495932C8")) return false;
      if (!do_tdes_test("C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4", "C4C4C4C4C4C4C4C4", "72DAF2A7C9EA6803")) return false;
      if (!do_tdes_test("C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5", "C5C5C5C5C5C5C5C5", "4FB5D5536DA544F4")) return false;
      if (!do_tdes_test("C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6", "C6C6C6C6C6C6C6C6", "1DD4E65AAF7988B4")) return false;
      if (!do_tdes_test("C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7", "C7C7C7C7C7C7C7C7", "76BF084C1535A6C6")) return false;
      if (!do_tdes_test("C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8", "C8C8C8C8C8C8C8C8", "AFEC35B09D36315F")) return false;
      if (!do_tdes_test("C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9", "C9C9C9C9C9C9C9C9", "C8078A6148818403")) return false;
      if (!do_tdes_test("CACACACACACACACACACACACACACACACA", "CACACACACACACACA", "4DA91CB4124B67FE")) return false;
      if (!do_tdes_test("CBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCB", "CBCBCBCBCBCBCBCB", "2DABFEB346794C3D")) return false;
      if (!do_tdes_test("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "CCCCCCCCCCCCCCCC", "FBCD12C790D21CD7")) return false;
      if (!do_tdes_test("CDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCD", "CDCDCDCDCDCDCDCD", "536873DB879CC770")) return false;
      if (!do_tdes_test("CECECECECECECECECECECECECECECECE", "CECECECECECECECE", "9AA159D7309DA7A0")) return false;
      if (!do_tdes_test("CFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCF", "CFCFCFCFCFCFCFCF", "0B844B9D8C4EA14A")) return false;
      if (!do_tdes_test("D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0", "D0D0D0D0D0D0D0D0", "3BBD84CE539E68C4")) return false;
      if (!do_tdes_test("D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1", "D1D1D1D1D1D1D1D1", "CF3E4F3E026E2C8E")) return false;
      if (!do_tdes_test("D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2", "D2D2D2D2D2D2D2D2", "82F85885D542AF58")) return false;
      if (!do_tdes_test("D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3", "D3D3D3D3D3D3D3D3", "22D334D6493B3CB6")) return false;
      if (!do_tdes_test("D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4", "D4D4D4D4D4D4D4D4", "47E9CB3E3154D673")) return false;
      if (!do_tdes_test("D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5", "D5D5D5D5D5D5D5D5", "2352BCC708ADC7E9")) return false;
      if (!do_tdes_test("D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6", "D6D6D6D6D6D6D6D6", "8C0F3BA0C8601980")) return false;
      if (!do_tdes_test("D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7", "D7D7D7D7D7D7D7D7", "EE5E9FD70CEF00E9")) return false;
      if (!do_tdes_test("D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8", "D8D8D8D8D8D8D8D8", "DEF6BDA6CABF9547")) return false;
      if (!do_tdes_test("D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9", "D9D9D9D9D9D9D9D9", "4DADD04A0EA70F20")) return false;
      if (!do_tdes_test("DADADADADADADADADADADADADADADADA", "DADADADADADADADA", "C1AA16689EE1B482")) return false;
      if (!do_tdes_test("DBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDB", "DBDBDBDBDBDBDBDB", "F45FC26193E69AEE")) return false;
      if (!do_tdes_test("DCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDC", "DCDCDCDCDCDCDCDC", "D0CFBB937CEDBFB5")) return false;
      if (!do_tdes_test("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", "DDDDDDDDDDDDDDDD", "F0752004EE23D87B")) return false;
      if (!do_tdes_test("DEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDE", "DEDEDEDEDEDEDEDE", "77A791E28AA464A5")) return false;
      if (!do_tdes_test("DFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDF", "DFDFDFDFDFDFDFDF", "E7562A7F56FF4966")) return false;
      if (!do_tdes_test("E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0", "E0E0E0E0E0E0E0E0", "B026913F2CCFB109")) return false;
      if (!do_tdes_test("E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1", "E1E1E1E1E1E1E1E1", "0DB572DDCE388AC7")) return false;
      if (!do_tdes_test("E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2", "E2E2E2E2E2E2E2E2", "D9FA6595F0C094CA")) return false;
      if (!do_tdes_test("E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3", "E3E3E3E3E3E3E3E3", "ADE4804C4BE4486E")) return false;
      if (!do_tdes_test("E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4", "E4E4E4E4E4E4E4E4", "007B81F520E6D7DA")) return false;
      if (!do_tdes_test("E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5", "E5E5E5E5E5E5E5E5", "961AEB77BFC10B3C")) return false;
      if (!do_tdes_test("E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6", "E6E6E6E6E6E6E6E6", "8A8DD870C9B14AF2")) return false;
      if (!do_tdes_test("E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7", "E7E7E7E7E7E7E7E7", "3CC02E14B6349B25")) return false;
      if (!do_tdes_test("E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8", "E8E8E8E8E8E8E8E8", "BAD3EE68BDDB9607")) return false;
      if (!do_tdes_test("E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9", "E9E9E9E9E9E9E9E9", "DFF918E93BDAD292")) return false;
      if (!do_tdes_test("EAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEA", "EAEAEAEAEAEAEAEA", "8FE559C7CD6FA56D")) return false;
      if (!do_tdes_test("EBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEB", "EBEBEBEBEBEBEBEB", "C88480835C1A444C")) return false;
      if (!do_tdes_test("ECECECECECECECECECECECECECECECEC", "ECECECECECECECEC", "D6EE30A16B2CC01E")) return false;
      if (!do_tdes_test("EDEDEDEDEDEDEDEDEDEDEDEDEDEDEDED", "EDEDEDEDEDEDEDED", "6932D887B2EA9C1A")) return false;
      if (!do_tdes_test("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", "EEEEEEEEEEEEEEEE", "0BFC865461F13ACC")) return false;
      if (!do_tdes_test("EFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEF", "EFEFEFEFEFEFEFEF", "228AEA0D403E807A")) return false;
      if (!do_tdes_test("F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0", "F0F0F0F0F0F0F0F0", "2A2891F65BB8173C")) return false;
      if (!do_tdes_test("F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1", "F1F1F1F1F1F1F1F1", "5D1B8FAF7839494B")) return false;
      if (!do_tdes_test("F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2", "F2F2F2F2F2F2F2F2", "1C0A9280EECF5D48")) return false;
      if (!do_tdes_test("F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3", "F3F3F3F3F3F3F3F3", "6CBCE951BBC30F74")) return false;
      if (!do_tdes_test("F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4", "F4F4F4F4F4F4F4F4", "9CA66E96BD08BC70")) return false;
      if (!do_tdes_test("F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5", "F5F5F5F5F5F5F5F5", "F5D779FCFBB28BF3")) return false;
      if (!do_tdes_test("F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6", "F6F6F6F6F6F6F6F6", "0FEC6BBF9B859184")) return false;
      if (!do_tdes_test("F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7", "F7F7F7F7F7F7F7F7", "EF88D2BF052DBDA8")) return false;
      if (!do_tdes_test("F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8", "F8F8F8F8F8F8F8F8", "39ADBDDB7363090D")) return false;
      if (!do_tdes_test("F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9", "F9F9F9F9F9F9F9F9", "C0AEAF445F7E2A7A")) return false;
      if (!do_tdes_test("FAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFA", "FAFAFAFAFAFAFAFA", "C66F54067298D4E9")) return false;
      if (!do_tdes_test("FBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFB", "FBFBFBFBFBFBFBFB", "E0BA8F4488AAF97C")) return false;
      if (!do_tdes_test("FCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFC", "FCFCFCFCFCFCFCFC", "67B36E2875D9631C")) return false;
      if (!do_tdes_test("FDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFD", "FDFDFDFDFDFDFDFD", "1ED83D49E267191D")) return false;
      if (!do_tdes_test("FEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFE", "FEFEFEFEFEFEFEFE", "66B2B23EA84693AD")) return false;
      if (!do_tdes_test("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFF", "7359B2163E4EDC58")) return false;
      if (!do_tdes_test("000102030405060708090A0B0C0D0E0F", "0011223344556677", "D117BD6373549FAA")) return false;
      if (!do_tdes_test("2BD6459F82C5B300952C49104881FF48", "EA024714AD5C4D84", "C616ACE843958247")) return false;
    
      if (!do_tdes_test("800000000000000000000000000000000000000000000000", "0000000000000000", "95A8D72813DAA94D")) return false;
      if (!do_tdes_test("400000000000000000000000000000000000000000000000", "0000000000000000", "0EEC1487DD8C26D5")) return false;
      if (!do_tdes_test("200000000000000000000000000000000000000000000000", "0000000000000000", "7AD16FFB79C45926")) return false;
      if (!do_tdes_test("100000000000000000000000000000000000000000000000", "0000000000000000", "D3746294CA6A6CF3")) return false;
      if (!do_tdes_test("080000000000000000000000000000000000000000000000", "0000000000000000", "809F5F873C1FD761")) return false;
      if (!do_tdes_test("040000000000000000000000000000000000000000000000", "0000000000000000", "C02FAFFEC989D1FC")) return false;
      if (!do_tdes_test("020000000000000000000000000000000000000000000000", "0000000000000000", "4615AA1D33E72F10")) return false;
      if (!do_tdes_test("010000000000000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("008000000000000000000000000000000000000000000000", "0000000000000000", "2055123350C00858")) return false;
      if (!do_tdes_test("004000000000000000000000000000000000000000000000", "0000000000000000", "DF3B99D6577397C8")) return false;
      if (!do_tdes_test("002000000000000000000000000000000000000000000000", "0000000000000000", "31FE17369B5288C9")) return false;
      if (!do_tdes_test("001000000000000000000000000000000000000000000000", "0000000000000000", "DFDD3CC64DAE1642")) return false;
      if (!do_tdes_test("000800000000000000000000000000000000000000000000", "0000000000000000", "178C83CE2B399D94")) return false;
      if (!do_tdes_test("000400000000000000000000000000000000000000000000", "0000000000000000", "50F636324A9B7F80")) return false;
      if (!do_tdes_test("000200000000000000000000000000000000000000000000", "0000000000000000", "A8468EE3BC18F06D")) return false;
      if (!do_tdes_test("000100000000000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000080000000000000000000000000000000000000000000", "0000000000000000", "A2DC9E92FD3CDE92")) return false;
      if (!do_tdes_test("000040000000000000000000000000000000000000000000", "0000000000000000", "CAC09F797D031287")) return false;
      if (!do_tdes_test("000020000000000000000000000000000000000000000000", "0000000000000000", "90BA680B22AEB525")) return false;
      if (!do_tdes_test("000010000000000000000000000000000000000000000000", "0000000000000000", "CE7A24F350E280B6")) return false;
      if (!do_tdes_test("000008000000000000000000000000000000000000000000", "0000000000000000", "882BFF0AA01A0B87")) return false;
      if (!do_tdes_test("000004000000000000000000000000000000000000000000", "0000000000000000", "25610288924511C2")) return false;
      if (!do_tdes_test("000002000000000000000000000000000000000000000000", "0000000000000000", "C71516C29C75D170")) return false;
      if (!do_tdes_test("000001000000000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000800000000000000000000000000000000000000000", "0000000000000000", "5199C29A52C9F059")) return false;
      if (!do_tdes_test("000000400000000000000000000000000000000000000000", "0000000000000000", "C22F0A294A71F29F")) return false;
      if (!do_tdes_test("000000200000000000000000000000000000000000000000", "0000000000000000", "EE371483714C02EA")) return false;
      if (!do_tdes_test("000000100000000000000000000000000000000000000000", "0000000000000000", "A81FBD448F9E522F")) return false;
      if (!do_tdes_test("000000080000000000000000000000000000000000000000", "0000000000000000", "4F644C92E192DFED")) return false;
      if (!do_tdes_test("000000040000000000000000000000000000000000000000", "0000000000000000", "1AFA9A66A6DF92AE")) return false;
      if (!do_tdes_test("000000020000000000000000000000000000000000000000", "0000000000000000", "B3C1CC715CB879D8")) return false;
      if (!do_tdes_test("000000010000000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000008000000000000000000000000000000000000000", "0000000000000000", "19D032E64AB0BD8B")) return false;
      if (!do_tdes_test("000000004000000000000000000000000000000000000000", "0000000000000000", "3CFAA7A7DC8720DC")) return false;
      if (!do_tdes_test("000000002000000000000000000000000000000000000000", "0000000000000000", "B7265F7F447AC6F3")) return false;
      if (!do_tdes_test("000000001000000000000000000000000000000000000000", "0000000000000000", "9DB73B3C0D163F54")) return false;
      if (!do_tdes_test("000000000800000000000000000000000000000000000000", "0000000000000000", "8181B65BABF4A975")) return false;
      if (!do_tdes_test("000000000400000000000000000000000000000000000000", "0000000000000000", "93C9B64042EAA240")) return false;
      if (!do_tdes_test("000000000200000000000000000000000000000000000000", "0000000000000000", "5570530829705592")) return false;
      if (!do_tdes_test("000000000100000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000080000000000000000000000000000000000000", "0000000000000000", "8638809E878787A0")) return false;
      if (!do_tdes_test("000000000040000000000000000000000000000000000000", "0000000000000000", "41B9A79AF79AC208")) return false;
      if (!do_tdes_test("000000000020000000000000000000000000000000000000", "0000000000000000", "7A9BE42F2009A892")) return false;
      if (!do_tdes_test("000000000010000000000000000000000000000000000000", "0000000000000000", "29038D56BA6D2745")) return false;
      if (!do_tdes_test("000000000008000000000000000000000000000000000000", "0000000000000000", "5495C6ABF1E5DF51")) return false;
      if (!do_tdes_test("000000000004000000000000000000000000000000000000", "0000000000000000", "AE13DBD561488933")) return false;
      if (!do_tdes_test("000000000002000000000000000000000000000000000000", "0000000000000000", "024D1FFA8904E389")) return false;
      if (!do_tdes_test("000000000001000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000800000000000000000000000000000000000", "0000000000000000", "D1399712F99BF02E")) return false;
      if (!do_tdes_test("000000000000400000000000000000000000000000000000", "0000000000000000", "14C1D7C1CFFEC79E")) return false;
      if (!do_tdes_test("000000000000200000000000000000000000000000000000", "0000000000000000", "1DE5279DAE3BED6F")) return false;
      if (!do_tdes_test("000000000000100000000000000000000000000000000000", "0000000000000000", "E941A33F85501303")) return false;
      if (!do_tdes_test("000000000000080000000000000000000000000000000000", "0000000000000000", "DA99DBBC9A03F379")) return false;
      if (!do_tdes_test("000000000000040000000000000000000000000000000000", "0000000000000000", "B7FC92F91D8E92E9")) return false;
      if (!do_tdes_test("000000000000020000000000000000000000000000000000", "0000000000000000", "AE8E5CAA3CA04E85")) return false;
      if (!do_tdes_test("000000000000010000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000008000000000000000000000000000000000", "0000000000000000", "9CC62DF43B6EED74")) return false;
      if (!do_tdes_test("000000000000004000000000000000000000000000000000", "0000000000000000", "D863DBB5C59A91A0")) return false;
      if (!do_tdes_test("000000000000002000000000000000000000000000000000", "0000000000000000", "A1AB2190545B91D7")) return false;
      if (!do_tdes_test("000000000000001000000000000000000000000000000000", "0000000000000000", "0875041E64C570F7")) return false;
      if (!do_tdes_test("000000000000000800000000000000000000000000000000", "0000000000000000", "5A594528BEBEF1CC")) return false;
      if (!do_tdes_test("000000000000000400000000000000000000000000000000", "0000000000000000", "FCDB3291DE21F0C0")) return false;
      if (!do_tdes_test("000000000000000200000000000000000000000000000000", "0000000000000000", "869EFD7F9F265A09")) return false;
      if (!do_tdes_test("000000000000000100000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000080000000000000000000000000000000", "0000000000000000", "C2A4DD96151453C2")) return false;
      if (!do_tdes_test("000000000000000040000000000000000000000000000000", "0000000000000000", "5E87809F6B8A7ED5")) return false;
      if (!do_tdes_test("000000000000000020000000000000000000000000000000", "0000000000000000", "81B838A1E9CD59B3")) return false;
      if (!do_tdes_test("000000000000000010000000000000000000000000000000", "0000000000000000", "DED028F0C1F5A774")) return false;
      if (!do_tdes_test("000000000000000008000000000000000000000000000000", "0000000000000000", "48C983815809FC87")) return false;
      if (!do_tdes_test("000000000000000004000000000000000000000000000000", "0000000000000000", "C1A75845F22BE951")) return false;
      if (!do_tdes_test("000000000000000002000000000000000000000000000000", "0000000000000000", "C60F823E8E994489")) return false;
      if (!do_tdes_test("000000000000000001000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000800000000000000000000000000000", "0000000000000000", "709F8FCB044172FE")) return false;
      if (!do_tdes_test("000000000000000000400000000000000000000000000000", "0000000000000000", "26BC2DE634BFFFD4")) return false;
      if (!do_tdes_test("000000000000000000200000000000000000000000000000", "0000000000000000", "D98126355C2E03E6")) return false;
      if (!do_tdes_test("000000000000000000100000000000000000000000000000", "0000000000000000", "49AAA91B49345137")) return false;
      if (!do_tdes_test("000000000000000000080000000000000000000000000000", "0000000000000000", "A59854DCE009126D")) return false;
      if (!do_tdes_test("000000000000000000040000000000000000000000000000", "0000000000000000", "21C46B9FDE5CD36B")) return false;
      if (!do_tdes_test("000000000000000000020000000000000000000000000000", "0000000000000000", "DEB4AE36E07BC053")) return false;
      if (!do_tdes_test("000000000000000000010000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000008000000000000000000000000000", "0000000000000000", "D47ADF8B94CACA7A")) return false;
      if (!do_tdes_test("000000000000000000004000000000000000000000000000", "0000000000000000", "D26D9656F91A1EE2")) return false;
      if (!do_tdes_test("000000000000000000002000000000000000000000000000", "0000000000000000", "EE31B8E767C9B337")) return false;
      if (!do_tdes_test("000000000000000000001000000000000000000000000000", "0000000000000000", "D19BA61DD59CE9A1")) return false;
      if (!do_tdes_test("000000000000000000000800000000000000000000000000", "0000000000000000", "482863934D17804B")) return false;
      if (!do_tdes_test("000000000000000000000400000000000000000000000000", "0000000000000000", "78C8CBCAC3B7FD35")) return false;
      if (!do_tdes_test("000000000000000000000200000000000000000000000000", "0000000000000000", "7B8B051E6C8AA8B6")) return false;
      if (!do_tdes_test("000000000000000000000100000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000080000000000000000000000000", "0000000000000000", "8CCFCD2418E85750")) return false;
      if (!do_tdes_test("000000000000000000000040000000000000000000000000", "0000000000000000", "E74CA11808ED17A3")) return false;
      if (!do_tdes_test("000000000000000000000020000000000000000000000000", "0000000000000000", "0A634C7A69897F35")) return false;
      if (!do_tdes_test("000000000000000000000010000000000000000000000000", "0000000000000000", "6C2C0F27E973CE29")) return false;
      if (!do_tdes_test("000000000000000000000008000000000000000000000000", "0000000000000000", "AD5F11ED913E918C")) return false;
      if (!do_tdes_test("000000000000000000000004000000000000000000000000", "0000000000000000", "3CE4B119BC1FC701")) return false;
      if (!do_tdes_test("000000000000000000000002000000000000000000000000", "0000000000000000", "7E6C8995AA52D298")) return false;
      if (!do_tdes_test("000000000000000000000001000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000800000000000000000000000", "0000000000000000", "A9FE6341C8621918")) return false;
      if (!do_tdes_test("000000000000000000000000400000000000000000000000", "0000000000000000", "CE99FD5D50B22CEF")) return false;
      if (!do_tdes_test("000000000000000000000000200000000000000000000000", "0000000000000000", "83E55C4A19ABCB56")) return false;
      if (!do_tdes_test("000000000000000000000000100000000000000000000000", "0000000000000000", "96E6A993443B9DD4")) return false;
      if (!do_tdes_test("000000000000000000000000080000000000000000000000", "0000000000000000", "6781B65D74A6B9FB")) return false;
      if (!do_tdes_test("000000000000000000000000040000000000000000000000", "0000000000000000", "D9EF04E272D1A78A")) return false;
      if (!do_tdes_test("000000000000000000000000020000000000000000000000", "0000000000000000", "AC8B09EC3153D57B")) return false;
      if (!do_tdes_test("000000000000000000000000010000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000008000000000000000000000", "0000000000000000", "60B4B8E3A8F5CBEC")) return false;
      if (!do_tdes_test("000000000000000000000000004000000000000000000000", "0000000000000000", "A5AB6F6EB66057A9")) return false;
      if (!do_tdes_test("000000000000000000000000002000000000000000000000", "0000000000000000", "FF7B0E870FB1FD0B")) return false;
      if (!do_tdes_test("000000000000000000000000001000000000000000000000", "0000000000000000", "7497A098AA651D00")) return false;
      if (!do_tdes_test("000000000000000000000000000800000000000000000000", "0000000000000000", "270A943BEABEA8EC")) return false;
      if (!do_tdes_test("000000000000000000000000000400000000000000000000", "0000000000000000", "67DB327ED5DF89E3")) return false;
      if (!do_tdes_test("000000000000000000000000000200000000000000000000", "0000000000000000", "4871C3B7436121DE")) return false;
      if (!do_tdes_test("000000000000000000000000000100000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000080000000000000000000", "0000000000000000", "41BBC8EF36654838")) return false;
      if (!do_tdes_test("000000000000000000000000000040000000000000000000", "0000000000000000", "FCBD166CA0EA87E2")) return false;
      if (!do_tdes_test("000000000000000000000000000020000000000000000000", "0000000000000000", "9DFFC6EE9751B5CF")) return false;
      if (!do_tdes_test("000000000000000000000000000010000000000000000000", "0000000000000000", "C01B7878EBCE8DD3")) return false;
      if (!do_tdes_test("000000000000000000000000000008000000000000000000", "0000000000000000", "357E5A4DC162D715")) return false;
      if (!do_tdes_test("000000000000000000000000000004000000000000000000", "0000000000000000", "268F93CAEB248E2E")) return false;
      if (!do_tdes_test("000000000000000000000000000002000000000000000000", "0000000000000000", "A5D4174744B84E7D")) return false;
      if (!do_tdes_test("000000000000000000000000000001000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000800000000000000000", "0000000000000000", "46F5E7077CB869A8")) return false;
      if (!do_tdes_test("000000000000000000000000000000400000000000000000", "0000000000000000", "502CD2BF4FC0B793")) return false;
      if (!do_tdes_test("000000000000000000000000000000200000000000000000", "0000000000000000", "C0278007230589E4")) return false;
      if (!do_tdes_test("000000000000000000000000000000100000000000000000", "0000000000000000", "52710C55818FAF52")) return false;
      if (!do_tdes_test("000000000000000000000000000000080000000000000000", "0000000000000000", "DF4A77123610F2B1")) return false;
      if (!do_tdes_test("000000000000000000000000000000040000000000000000", "0000000000000000", "EF840B00DA448234")) return false;
      if (!do_tdes_test("000000000000000000000000000000020000000000000000", "0000000000000000", "FFCCC32A699CB7C5")) return false;
      if (!do_tdes_test("000000000000000000000000000000010000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000008000000000000000", "0000000000000000", "95A8D72813DAA94D")) return false;
      if (!do_tdes_test("000000000000000000000000000000004000000000000000", "0000000000000000", "0EEC1487DD8C26D5")) return false;
      if (!do_tdes_test("000000000000000000000000000000002000000000000000", "0000000000000000", "7AD16FFB79C45926")) return false;
      if (!do_tdes_test("000000000000000000000000000000001000000000000000", "0000000000000000", "D3746294CA6A6CF3")) return false;
      if (!do_tdes_test("000000000000000000000000000000000800000000000000", "0000000000000000", "809F5F873C1FD761")) return false;
      if (!do_tdes_test("000000000000000000000000000000000400000000000000", "0000000000000000", "C02FAFFEC989D1FC")) return false;
      if (!do_tdes_test("000000000000000000000000000000000200000000000000", "0000000000000000", "4615AA1D33E72F10")) return false;
      if (!do_tdes_test("000000000000000000000000000000000100000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000080000000000000", "0000000000000000", "2055123350C00858")) return false;
      if (!do_tdes_test("000000000000000000000000000000000040000000000000", "0000000000000000", "DF3B99D6577397C8")) return false;
      if (!do_tdes_test("000000000000000000000000000000000020000000000000", "0000000000000000", "31FE17369B5288C9")) return false;
      if (!do_tdes_test("000000000000000000000000000000000010000000000000", "0000000000000000", "DFDD3CC64DAE1642")) return false;
      if (!do_tdes_test("000000000000000000000000000000000008000000000000", "0000000000000000", "178C83CE2B399D94")) return false;
      if (!do_tdes_test("000000000000000000000000000000000004000000000000", "0000000000000000", "50F636324A9B7F80")) return false;
      if (!do_tdes_test("000000000000000000000000000000000002000000000000", "0000000000000000", "A8468EE3BC18F06D")) return false;
      if (!do_tdes_test("000000000000000000000000000000000001000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000800000000000", "0000000000000000", "A2DC9E92FD3CDE92")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000400000000000", "0000000000000000", "CAC09F797D031287")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000200000000000", "0000000000000000", "90BA680B22AEB525")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000100000000000", "0000000000000000", "CE7A24F350E280B6")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000080000000000", "0000000000000000", "882BFF0AA01A0B87")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000040000000000", "0000000000000000", "25610288924511C2")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000020000000000", "0000000000000000", "C71516C29C75D170")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000010000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000008000000000", "0000000000000000", "5199C29A52C9F059")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000004000000000", "0000000000000000", "C22F0A294A71F29F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000002000000000", "0000000000000000", "EE371483714C02EA")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000001000000000", "0000000000000000", "A81FBD448F9E522F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000800000000", "0000000000000000", "4F644C92E192DFED")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000400000000", "0000000000000000", "1AFA9A66A6DF92AE")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000200000000", "0000000000000000", "B3C1CC715CB879D8")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000100000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000080000000", "0000000000000000", "19D032E64AB0BD8B")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000040000000", "0000000000000000", "3CFAA7A7DC8720DC")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000020000000", "0000000000000000", "B7265F7F447AC6F3")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000010000000", "0000000000000000", "9DB73B3C0D163F54")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000008000000", "0000000000000000", "8181B65BABF4A975")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000004000000", "0000000000000000", "93C9B64042EAA240")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000002000000", "0000000000000000", "5570530829705592")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000001000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000800000", "0000000000000000", "8638809E878787A0")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000400000", "0000000000000000", "41B9A79AF79AC208")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000200000", "0000000000000000", "7A9BE42F2009A892")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000100000", "0000000000000000", "29038D56BA6D2745")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000080000", "0000000000000000", "5495C6ABF1E5DF51")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000040000", "0000000000000000", "AE13DBD561488933")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000020000", "0000000000000000", "024D1FFA8904E389")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000010000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000008000", "0000000000000000", "D1399712F99BF02E")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000004000", "0000000000000000", "14C1D7C1CFFEC79E")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000002000", "0000000000000000", "1DE5279DAE3BED6F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000001000", "0000000000000000", "E941A33F85501303")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000800", "0000000000000000", "DA99DBBC9A03F379")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000400", "0000000000000000", "B7FC92F91D8E92E9")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000200", "0000000000000000", "AE8E5CAA3CA04E85")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000100", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000080", "0000000000000000", "9CC62DF43B6EED74")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000040", "0000000000000000", "D863DBB5C59A91A0")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000020", "0000000000000000", "A1AB2190545B91D7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000010", "0000000000000000", "0875041E64C570F7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000008", "0000000000000000", "5A594528BEBEF1CC")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000004", "0000000000000000", "FCDB3291DE21F0C0")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000002", "0000000000000000", "869EFD7F9F265A09")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000001", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "8000000000000000", "95F8A5E5DD31D900")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "4000000000000000", "DD7F121CA5015619")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "2000000000000000", "2E8653104F3834EA")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "1000000000000000", "4BD388FF6CD81D4F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0800000000000000", "20B9E767B2FB1456")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0400000000000000", "55579380D77138EF")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0200000000000000", "6CC5DEFAAF04512F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0100000000000000", "0D9F279BA5D87260")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0080000000000000", "D9031B0271BD5A0A")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0040000000000000", "424250B37C3DD951")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0020000000000000", "B8061B7ECD9A21E5")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0010000000000000", "F15D0F286B65BD28")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0008000000000000", "ADD0CC8D6E5DEBA1")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0004000000000000", "E6D5F82752AD63D1")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0002000000000000", "ECBFE3BD3F591A5E")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0001000000000000", "F356834379D165CD")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000800000000000", "2B9F982F20037FA9")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000400000000000", "889DE068A16F0BE6")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000200000000000", "E19E275D846A1298")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000100000000000", "329A8ED523D71AEC")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000080000000000", "E7FCE22557D23C97")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000040000000000", "12A9F5817FF2D65D")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000020000000000", "A484C3AD38DC9C19")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000010000000000", "FBE00A8A1EF8AD72")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000008000000000", "750D079407521363")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000004000000000", "64FEED9C724C2FAF")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000002000000000", "F02B263B328E2B60")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000001000000000", "9D64555A9A10B852")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000800000000", "D106FF0BED5255D7")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000400000000", "E1652C6B138C64A5")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000200000000", "E428581186EC8F46")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000100000000", "AEB5F5EDE22D1A36")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000080000000", "E943D7568AEC0C5C")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000040000000", "DF98C8276F54B04B")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000020000000", "B160E4680F6C696F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000010000000", "FA0752B07D9C4AB8")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000008000000", "CA3A2B036DBC8502")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000004000000", "5E0905517BB59BCF")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000002000000", "814EEB3B91D90726")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000001000000", "4D49DB1532919C9F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000800000", "25EB5FC3F8CF0621")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000400000", "AB6A20C0620D1C6F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000200000", "79E90DBC98F92CCA")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000100000", "866ECEDD8072BB0E")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000080000", "8B54536F2F3E64A8")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000040000", "EA51D3975595B86B")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000020000", "CAFFC6AC4542DE31")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000010000", "8DD45A2DDF90796C")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000008000", "1029D55E880EC2D0")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000004000", "5D86CB23639DBEA9")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000002000", "1D1CA853AE7C0C5F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000001000", "CE332329248F3228")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000800", "8405D1ABE24FB942")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000400", "E643D78090CA4207")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000200", "48221B9937748A23")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000100", "DD7C0BBD61FAFD54")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000080", "2FBC291A570DB5C4")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000040", "E07C30D7E4E26E12")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000020", "0953E2258E8E90A1")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000010", "5B711BC4CEEBF2EE")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000008", "CC083F1E6D9E85F6")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000004", "D2FD8867D50D2DFE")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000002", "06E7EA22CE92708F")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000001", "166B40B44ABA4BD6")) return false;
      if (!do_tdes_test("000000000000000000000000000000000000000000000000", "0000000000000000", "8CA64DE9C1B123A7")) return false;
      if (!do_tdes_test("010101010101010101010101010101010101010101010101", "0101010101010101", "994D4DC157B96C52")) return false;
      if (!do_tdes_test("020202020202020202020202020202020202020202020202", "0202020202020202", "E127C2B61D98E6E2")) return false;
      if (!do_tdes_test("030303030303030303030303030303030303030303030303", "0303030303030303", "984C91D78A269CE3")) return false;
      if (!do_tdes_test("040404040404040404040404040404040404040404040404", "0404040404040404", "1F4570BB77550683")) return false;
      if (!do_tdes_test("050505050505050505050505050505050505050505050505", "0505050505050505", "3990ABF98D672B16")) return false;
      if (!do_tdes_test("060606060606060606060606060606060606060606060606", "0606060606060606", "3F5150BBA081D585")) return false;
      if (!do_tdes_test("070707070707070707070707070707070707070707070707", "0707070707070707", "C65242248C9CF6F2")) return false;
      if (!do_tdes_test("080808080808080808080808080808080808080808080808", "0808080808080808", "10772D40FAD24257")) return false;
      if (!do_tdes_test("090909090909090909090909090909090909090909090909", "0909090909090909", "F0139440647A6E7B")) return false;
      if (!do_tdes_test("0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A", "0A0A0A0A0A0A0A0A", "0A288603044D740C")) return false;
      if (!do_tdes_test("0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B", "0B0B0B0B0B0B0B0B", "6359916942F7438F")) return false;
      if (!do_tdes_test("0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C", "0C0C0C0C0C0C0C0C", "934316AE443CF08B")) return false;
      if (!do_tdes_test("0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D", "0D0D0D0D0D0D0D0D", "E3F56D7F1130A2B7")) return false;
      if (!do_tdes_test("0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E", "0E0E0E0E0E0E0E0E", "A2E4705087C6B6B4")) return false;
      if (!do_tdes_test("0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F", "0F0F0F0F0F0F0F0F", "D5D76E09A447E8C3")) return false;
      if (!do_tdes_test("101010101010101010101010101010101010101010101010", "1010101010101010", "DD7515F2BFC17F85")) return false;
      if (!do_tdes_test("111111111111111111111111111111111111111111111111", "1111111111111111", "F40379AB9E0EC533")) return false;
      if (!do_tdes_test("121212121212121212121212121212121212121212121212", "1212121212121212", "96CD27784D1563E5")) return false;
      if (!do_tdes_test("131313131313131313131313131313131313131313131313", "1313131313131313", "2911CF5E94D33FE1")) return false;
      if (!do_tdes_test("141414141414141414141414141414141414141414141414", "1414141414141414", "377B7F7CA3E5BBB3")) return false;
      if (!do_tdes_test("151515151515151515151515151515151515151515151515", "1515151515151515", "701AA63832905A92")) return false;
      if (!do_tdes_test("161616161616161616161616161616161616161616161616", "1616161616161616", "2006E716C4252D6D")) return false;
      if (!do_tdes_test("171717171717171717171717171717171717171717171717", "1717171717171717", "452C1197422469F8")) return false;
      if (!do_tdes_test("181818181818181818181818181818181818181818181818", "1818181818181818", "C33FD1EB49CB64DA")) return false;
      if (!do_tdes_test("191919191919191919191919191919191919191919191919", "1919191919191919", "7572278F364EB50D")) return false;
      if (!do_tdes_test("1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A", "1A1A1A1A1A1A1A1A", "69E51488403EF4C3")) return false;
      if (!do_tdes_test("1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B", "1B1B1B1B1B1B1B1B", "FF847E0ADF192825")) return false;
      if (!do_tdes_test("1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C", "1C1C1C1C1C1C1C1C", "521B7FB3B41BB791")) return false;
      if (!do_tdes_test("1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D", "1D1D1D1D1D1D1D1D", "26059A6A0F3F6B35")) return false;
      if (!do_tdes_test("1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E", "1E1E1E1E1E1E1E1E", "F24A8D2231C77538")) return false;
      if (!do_tdes_test("1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F", "1F1F1F1F1F1F1F1F", "4FD96EC0D3304EF6")) return false;
      if (!do_tdes_test("202020202020202020202020202020202020202020202020", "2020202020202020", "18A9D580A900B699")) return false;
      if (!do_tdes_test("212121212121212121212121212121212121212121212121", "2121212121212121", "88586E1D755B9B5A")) return false;
      if (!do_tdes_test("222222222222222222222222222222222222222222222222", "2222222222222222", "0F8ADFFB11DC2784")) return false;
      if (!do_tdes_test("232323232323232323232323232323232323232323232323", "2323232323232323", "2F30446C8312404A")) return false;
      if (!do_tdes_test("242424242424242424242424242424242424242424242424", "2424242424242424", "0BA03D9E6C196511")) return false;
      if (!do_tdes_test("252525252525252525252525252525252525252525252525", "2525252525252525", "3E55E997611E4B7D")) return false;
      if (!do_tdes_test("262626262626262626262626262626262626262626262626", "2626262626262626", "B2522FB5F158F0DF")) return false;
      if (!do_tdes_test("272727272727272727272727272727272727272727272727", "2727272727272727", "2109425935406AB8")) return false;
      if (!do_tdes_test("282828282828282828282828282828282828282828282828", "2828282828282828", "11A16028F310FF16")) return false;
      if (!do_tdes_test("292929292929292929292929292929292929292929292929", "2929292929292929", "73F0C45F379FE67F")) return false;
      if (!do_tdes_test("2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A", "2A2A2A2A2A2A2A2A", "DCAD4338F7523816")) return false;
      if (!do_tdes_test("2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B", "2B2B2B2B2B2B2B2B", "B81634C1CEAB298C")) return false;
      if (!do_tdes_test("2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C", "2C2C2C2C2C2C2C2C", "DD2CCB29B6C4C349")) return false;
      if (!do_tdes_test("2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D", "2D2D2D2D2D2D2D2D", "7D07A77A2ABD50A7")) return false;
      if (!do_tdes_test("2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E", "2E2E2E2E2E2E2E2E", "30C1B0C1FD91D371")) return false;
      if (!do_tdes_test("2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F", "2F2F2F2F2F2F2F2F", "C4427B31AC61973B")) return false;
      if (!do_tdes_test("303030303030303030303030303030303030303030303030", "3030303030303030", "F47BB46273B15EB5")) return false;
      if (!do_tdes_test("313131313131313131313131313131313131313131313131", "3131313131313131", "655EA628CF62585F")) return false;
      if (!do_tdes_test("323232323232323232323232323232323232323232323232", "3232323232323232", "AC978C247863388F")) return false;
      if (!do_tdes_test("333333333333333333333333333333333333333333333333", "3333333333333333", "0432ED386F2DE328")) return false;
      if (!do_tdes_test("343434343434343434343434343434343434343434343434", "3434343434343434", "D254014CB986B3C2")) return false;
      if (!do_tdes_test("353535353535353535353535353535353535353535353535", "3535353535353535", "B256E34BEDB49801")) return false;
      if (!do_tdes_test("363636363636363636363636363636363636363636363636", "3636363636363636", "37F8759EB77E7BFC")) return false;
      if (!do_tdes_test("373737373737373737373737373737373737373737373737", "3737373737373737", "5013CA4F62C9CEA0")) return false;
      if (!do_tdes_test("383838383838383838383838383838383838383838383838", "3838383838383838", "8940F7B3EACA5939")) return false;
      if (!do_tdes_test("393939393939393939393939393939393939393939393939", "3939393939393939", "E22B19A55086774B")) return false;
      if (!do_tdes_test("3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A", "3A3A3A3A3A3A3A3A", "B04A2AAC925ABB0B")) return false;
      if (!do_tdes_test("3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B", "3B3B3B3B3B3B3B3B", "8D250D58361597FC")) return false;
      if (!do_tdes_test("3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C", "3C3C3C3C3C3C3C3C", "51F0114FB6A6CD37")) return false;
      if (!do_tdes_test("3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D", "3D3D3D3D3D3D3D3D", "9D0BB4DB830ECB73")) return false;
      if (!do_tdes_test("3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E", "3E3E3E3E3E3E3E3E", "E96089D6368F3E1A")) return false;
      if (!do_tdes_test("3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F", "3F3F3F3F3F3F3F3F", "5C4CA877A4E1E92D")) return false;
      if (!do_tdes_test("404040404040404040404040404040404040404040404040", "4040404040404040", "6D55DDBC8DEA95FF")) return false;
      if (!do_tdes_test("414141414141414141414141414141414141414141414141", "4141414141414141", "19DF84AC95551003")) return false;
      if (!do_tdes_test("424242424242424242424242424242424242424242424242", "4242424242424242", "724E7332696D08A7")) return false;
      if (!do_tdes_test("434343434343434343434343434343434343434343434343", "4343434343434343", "B91810B8CDC58FE2")) return false;
      if (!do_tdes_test("444444444444444444444444444444444444444444444444", "4444444444444444", "06E23526EDCCD0C4")) return false;
      if (!do_tdes_test("454545454545454545454545454545454545454545454545", "4545454545454545", "EF52491D5468D441")) return false;
      if (!do_tdes_test("464646464646464646464646464646464646464646464646", "4646464646464646", "48019C59E39B90C5")) return false;
      if (!do_tdes_test("474747474747474747474747474747474747474747474747", "4747474747474747", "0544083FB902D8C0")) return false;
      if (!do_tdes_test("484848484848484848484848484848484848484848484848", "4848484848484848", "63B15CADA668CE12")) return false;
      if (!do_tdes_test("494949494949494949494949494949494949494949494949", "4949494949494949", "EACC0C1264171071")) return false;
      if (!do_tdes_test("4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A", "4A4A4A4A4A4A4A4A", "9D2B8C0AC605F274")) return false;
      if (!do_tdes_test("4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B", "4B4B4B4B4B4B4B4B", "C90F2F4C98A8FB2A")) return false;
      if (!do_tdes_test("4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C", "4C4C4C4C4C4C4C4C", "03481B4828FD1D04")) return false;
      if (!do_tdes_test("4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D", "4D4D4D4D4D4D4D4D", "C78FC45A1DCEA2E2")) return false;
      if (!do_tdes_test("4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E", "4E4E4E4E4E4E4E4E", "DB96D88C3460D801")) return false;
      if (!do_tdes_test("4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F", "4F4F4F4F4F4F4F4F", "6C69E720F5105518")) return false;
      if (!do_tdes_test("505050505050505050505050505050505050505050505050", "5050505050505050", "0D262E418BC893F3")) return false;
      if (!do_tdes_test("515151515151515151515151515151515151515151515151", "5151515151515151", "6AD84FD7848A0A5C")) return false;
      if (!do_tdes_test("525252525252525252525252525252525252525252525252", "5252525252525252", "C365CB35B34B6114")) return false;
      if (!do_tdes_test("535353535353535353535353535353535353535353535353", "5353535353535353", "1155392E877F42A9")) return false;
      if (!do_tdes_test("545454545454545454545454545454545454545454545454", "5454545454545454", "531BE5F9405DA715")) return false;
      if (!do_tdes_test("555555555555555555555555555555555555555555555555", "5555555555555555", "3BCDD41E6165A5E8")) return false;
      if (!do_tdes_test("565656565656565656565656565656565656565656565656", "5656565656565656", "2B1FF5610A19270C")) return false;
      if (!do_tdes_test("575757575757575757575757575757575757575757575757", "5757575757575757", "D90772CF3F047CFD")) return false;
      if (!do_tdes_test("585858585858585858585858585858585858585858585858", "5858585858585858", "1BEA27FFB72457B7")) return false;
      if (!do_tdes_test("595959595959595959595959595959595959595959595959", "5959595959595959", "85C3E0C429F34C27")) return false;
      if (!do_tdes_test("5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A", "5A5A5A5A5A5A5A5A", "F9038021E37C7618")) return false;
      if (!do_tdes_test("5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B", "5B5B5B5B5B5B5B5B", "35BC6FF838DBA32F")) return false;
      if (!do_tdes_test("5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C", "5C5C5C5C5C5C5C5C", "4927ACC8CE45ECE7")) return false;
      if (!do_tdes_test("5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D", "5D5D5D5D5D5D5D5D", "E812EE6E3572985C")) return false;
      if (!do_tdes_test("5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E", "5E5E5E5E5E5E5E5E", "9BB93A89627BF65F")) return false;
      if (!do_tdes_test("5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F", "5F5F5F5F5F5F5F5F", "EF12476884CB74CA")) return false;
      if (!do_tdes_test("606060606060606060606060606060606060606060606060", "6060606060606060", "1BF17E00C09E7CBF")) return false;
      if (!do_tdes_test("616161616161616161616161616161616161616161616161", "6161616161616161", "29932350C098DB5D")) return false;
      if (!do_tdes_test("626262626262626262626262626262626262626262626262", "6262626262626262", "B476E6499842AC54")) return false;
      if (!do_tdes_test("636363636363636363636363636363636363636363636363", "6363636363636363", "5C662C29C1E96056")) return false;
      if (!do_tdes_test("646464646464646464646464646464646464646464646464", "6464646464646464", "3AF1703D76442789")) return false;
      if (!do_tdes_test("656565656565656565656565656565656565656565656565", "6565656565656565", "86405D9B425A8C8C")) return false;
      if (!do_tdes_test("666666666666666666666666666666666666666666666666", "6666666666666666", "EBBF4810619C2C55")) return false;
      if (!do_tdes_test("676767676767676767676767676767676767676767676767", "6767676767676767", "F8D1CD7367B21B5D")) return false;
      if (!do_tdes_test("686868686868686868686868686868686868686868686868", "6868686868686868", "9EE703142BF8D7E2")) return false;
      if (!do_tdes_test("696969696969696969696969696969696969696969696969", "6969696969696969", "5FDFFFC3AAAB0CB3")) return false;
      if (!do_tdes_test("6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A", "6A6A6A6A6A6A6A6A", "26C940AB13574231")) return false;
      if (!do_tdes_test("6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B", "6B6B6B6B6B6B6B6B", "1E2DC77E36A84693")) return false;
      if (!do_tdes_test("6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C", "6C6C6C6C6C6C6C6C", "0F4FF4D9BC7E2244")) return false;
      if (!do_tdes_test("6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D", "6D6D6D6D6D6D6D6D", "A4C9A0D04D3280CD")) return false;
      if (!do_tdes_test("6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E", "6E6E6E6E6E6E6E6E", "9FAF2C96FE84919D")) return false;
      if (!do_tdes_test("6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F", "6F6F6F6F6F6F6F6F", "115DBC965E6096C8")) return false;
      if (!do_tdes_test("707070707070707070707070707070707070707070707070", "7070707070707070", "AF531E9520994017")) return false;
      if (!do_tdes_test("717171717171717171717171717171717171717171717171", "7171717171717171", "B971ADE70E5C89EE")) return false;
      if (!do_tdes_test("727272727272727272727272727272727272727272727272", "7272727272727272", "415D81C86AF9C376")) return false;
      if (!do_tdes_test("737373737373737373737373737373737373737373737373", "7373737373737373", "8DFB864FDB3C6811")) return false;
      if (!do_tdes_test("747474747474747474747474747474747474747474747474", "7474747474747474", "10B1C170E3398F91")) return false;
      if (!do_tdes_test("757575757575757575757575757575757575757575757575", "7575757575757575", "CFEF7A1C0218DB1E")) return false;
      if (!do_tdes_test("767676767676767676767676767676767676767676767676", "7676767676767676", "DBAC30A2A40B1B9C")) return false;
      if (!do_tdes_test("777777777777777777777777777777777777777777777777", "7777777777777777", "89D3BF37052162E9")) return false;
      if (!do_tdes_test("787878787878787878787878787878787878787878787878", "7878787878787878", "80D9230BDAEB67DC")) return false;
      if (!do_tdes_test("797979797979797979797979797979797979797979797979", "7979797979797979", "3440911019AD68D7")) return false;
      if (!do_tdes_test("7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A", "7A7A7A7A7A7A7A7A", "9626FE57596E199E")) return false;
      if (!do_tdes_test("7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B", "7B7B7B7B7B7B7B7B", "DEA0B796624BB5BA")) return false;
      if (!do_tdes_test("7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C", "7C7C7C7C7C7C7C7C", "E9E40542BDDB3E9D")) return false;
      if (!do_tdes_test("7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D", "7D7D7D7D7D7D7D7D", "8AD99914B354B911")) return false;
      if (!do_tdes_test("7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E", "7E7E7E7E7E7E7E7E", "6F85B98DD12CB13B")) return false;
      if (!do_tdes_test("7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F", "7F7F7F7F7F7F7F7F", "10130DA3C3A23924")) return false;
      if (!do_tdes_test("808080808080808080808080808080808080808080808080", "8080808080808080", "EFECF25C3C5DC6DB")) return false;
      if (!do_tdes_test("818181818181818181818181818181818181818181818181", "8181818181818181", "907A46722ED34EC4")) return false;
      if (!do_tdes_test("828282828282828282828282828282828282828282828282", "8282828282828282", "752666EB4CAB46EE")) return false;
      if (!do_tdes_test("838383838383838383838383838383838383838383838383", "8383838383838383", "161BFABD4224C162")) return false;
      if (!do_tdes_test("848484848484848484848484848484848484848484848484", "8484848484848484", "215F48699DB44A45")) return false;
      if (!do_tdes_test("858585858585858585858585858585858585858585858585", "8585858585858585", "69D901A8A691E661")) return false;
      if (!do_tdes_test("868686868686868686868686868686868686868686868686", "8686868686868686", "CBBF6EEFE6529728")) return false;
      if (!do_tdes_test("878787878787878787878787878787878787878787878787", "8787878787878787", "7F26DCF425149823")) return false;
      if (!do_tdes_test("888888888888888888888888888888888888888888888888", "8888888888888888", "762C40C8FADE9D16")) return false;
      if (!do_tdes_test("898989898989898989898989898989898989898989898989", "8989898989898989", "2453CF5D5BF4E463")) return false;
      if (!do_tdes_test("8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A", "8A8A8A8A8A8A8A8A", "301085E3FDE724E1")) return false;
      if (!do_tdes_test("8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B", "8B8B8B8B8B8B8B8B", "EF4E3E8F1CC6706E")) return false;
      if (!do_tdes_test("8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C", "8C8C8C8C8C8C8C8C", "720479B024C397EE")) return false;
      if (!do_tdes_test("8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D", "8D8D8D8D8D8D8D8D", "BEA27E3795063C89")) return false;
      if (!do_tdes_test("8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E", "8E8E8E8E8E8E8E8E", "468E5218F1A37611")) return false;
      if (!do_tdes_test("8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F", "8F8F8F8F8F8F8F8F", "50ACE16ADF66BFE8")) return false;
      if (!do_tdes_test("909090909090909090909090909090909090909090909090", "9090909090909090", "EEA24369A19F6937")) return false;
      if (!do_tdes_test("919191919191919191919191919191919191919191919191", "9191919191919191", "6050D369017B6E62")) return false;
      if (!do_tdes_test("929292929292929292929292929292929292929292929292", "9292929292929292", "5B365F2FB2CD7F32")) return false;
      if (!do_tdes_test("939393939393939393939393939393939393939393939393", "9393939393939393", "F0B00B264381DDBB")) return false;
      if (!do_tdes_test("949494949494949494949494949494949494949494949494", "9494949494949494", "E1D23881C957B96C")) return false;
      if (!do_tdes_test("959595959595959595959595959595959595959595959595", "9595959595959595", "D936BF54ECA8BDCE")) return false;
      if (!do_tdes_test("969696969696969696969696969696969696969696969696", "9696969696969696", "A020003C5554F34C")) return false;
      if (!do_tdes_test("979797979797979797979797979797979797979797979797", "9797979797979797", "6118FCEBD407281D")) return false;
      if (!do_tdes_test("989898989898989898989898989898989898989898989898", "9898989898989898", "072E328C984DE4A2")) return false;
      if (!do_tdes_test("999999999999999999999999999999999999999999999999", "9999999999999999", "1440B7EF9E63D3AA")) return false;
      if (!do_tdes_test("9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A", "9A9A9A9A9A9A9A9A", "79BFA264BDA57373")) return false;
      if (!do_tdes_test("9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B", "9B9B9B9B9B9B9B9B", "C50E8FC289BBD876")) return false;
      if (!do_tdes_test("9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C", "9C9C9C9C9C9C9C9C", "A399D3D63E169FA9")) return false;
      if (!do_tdes_test("9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D", "9D9D9D9D9D9D9D9D", "4B8919B667BD53AB")) return false;
      if (!do_tdes_test("9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E", "9E9E9E9E9E9E9E9E", "D66CDCAF3F6724A2")) return false;
      if (!do_tdes_test("9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F", "9F9F9F9F9F9F9F9F", "E40E81FF3F618340")) return false;
      if (!do_tdes_test("A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0A0", "A0A0A0A0A0A0A0A0", "10EDB8977B348B35")) return false;
      if (!do_tdes_test("A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1", "A1A1A1A1A1A1A1A1", "6446C5769D8409A0")) return false;
      if (!do_tdes_test("A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2A2", "A2A2A2A2A2A2A2A2", "17ED1191CA8D67A3")) return false;
      if (!do_tdes_test("A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3A3", "A3A3A3A3A3A3A3A3", "B6D8533731BA1318")) return false;
      if (!do_tdes_test("A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4", "A4A4A4A4A4A4A4A4", "CA439007C7245CD0")) return false;
      if (!do_tdes_test("A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5A5", "A5A5A5A5A5A5A5A5", "06FC7FDE1C8389E7")) return false;
      if (!do_tdes_test("A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6A6", "A6A6A6A6A6A6A6A6", "7A3C1F3BD60CB3D8")) return false;
      if (!do_tdes_test("A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7A7", "A7A7A7A7A7A7A7A7", "E415D80048DBA848")) return false;
      if (!do_tdes_test("A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8A8", "A8A8A8A8A8A8A8A8", "26F88D30C0FB8302")) return false;
      if (!do_tdes_test("A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9A9", "A9A9A9A9A9A9A9A9", "D4E00A9EF5E6D8F3")) return false;
      if (!do_tdes_test("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAAAAAAAAAAAAA", "C4322BE19E9A5A17")) return false;
      if (!do_tdes_test("ABABABABABABABABABABABABABABABABABABABABABABABAB", "ABABABABABABABAB", "ACE41A06BFA258EA")) return false;
      if (!do_tdes_test("ACACACACACACACACACACACACACACACACACACACACACACACAC", "ACACACACACACACAC", "EEAAC6D17880BD56")) return false;
      if (!do_tdes_test("ADADADADADADADADADADADADADADADADADADADADADADADAD", "ADADADADADADADAD", "3C9A34CA4CB49EEB")) return false;
      if (!do_tdes_test("AEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAE", "AEAEAEAEAEAEAEAE", "9527B0287B75F5A3")) return false;
      if (!do_tdes_test("AFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAF", "AFAFAFAFAFAFAFAF", "F2D9D1BE74376C0C")) return false;
      if (!do_tdes_test("B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0B0", "B0B0B0B0B0B0B0B0", "939618DF0AEFAAE7")) return false;
      if (!do_tdes_test("B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1B1", "B1B1B1B1B1B1B1B1", "24692773CB9F27FE")) return false;
      if (!do_tdes_test("B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2", "B2B2B2B2B2B2B2B2", "38703BA5E2315D1D")) return false;
      if (!do_tdes_test("B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3B3", "B3B3B3B3B3B3B3B3", "FCB7E4B7D702E2FB")) return false;
      if (!do_tdes_test("B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4B4", "B4B4B4B4B4B4B4B4", "36F0D0B3675704D5")) return false;
      if (!do_tdes_test("B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5B5", "B5B5B5B5B5B5B5B5", "62D473F539FA0D8B")) return false;
      if (!do_tdes_test("B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6B6", "B6B6B6B6B6B6B6B6", "1533F3ED9BE8EF8E")) return false;
      if (!do_tdes_test("B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7B7", "B7B7B7B7B7B7B7B7", "9C4EA352599731ED")) return false;
      if (!do_tdes_test("B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8B8", "B8B8B8B8B8B8B8B8", "FABBF7C046FD273F")) return false;
      if (!do_tdes_test("B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9B9", "B9B9B9B9B9B9B9B9", "B7FE63A61C646F3A")) return false;
      if (!do_tdes_test("BABABABABABABABABABABABABABABABABABABABABABABABA", "BABABABABABABABA", "10ADB6E2AB972BBE")) return false;
      if (!do_tdes_test("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "BBBBBBBBBBBBBBBB", "F91DCAD912332F3B")) return false;
      if (!do_tdes_test("BCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBC", "BCBCBCBCBCBCBCBC", "46E7EF47323A701D")) return false;
      if (!do_tdes_test("BDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBD", "BDBDBDBDBDBDBDBD", "8DB18CCD9692F758")) return false;
      if (!do_tdes_test("BEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBE", "BEBEBEBEBEBEBEBE", "E6207B536AAAEFFC")) return false;
      if (!do_tdes_test("BFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBF", "BFBFBFBFBFBFBFBF", "92AA224372156A00")) return false;
      if (!do_tdes_test("C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0", "C0C0C0C0C0C0C0C0", "A3B357885B1E16D2")) return false;
      if (!do_tdes_test("C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1C1", "C1C1C1C1C1C1C1C1", "169F7629C970C1E5")) return false;
      if (!do_tdes_test("C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2C2", "C2C2C2C2C2C2C2C2", "62F44B247CF1348C")) return false;
      if (!do_tdes_test("C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3C3", "C3C3C3C3C3C3C3C3", "AE0FEEB0495932C8")) return false;
      if (!do_tdes_test("C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4", "C4C4C4C4C4C4C4C4", "72DAF2A7C9EA6803")) return false;
      if (!do_tdes_test("C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5C5", "C5C5C5C5C5C5C5C5", "4FB5D5536DA544F4")) return false;
      if (!do_tdes_test("C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6C6", "C6C6C6C6C6C6C6C6", "1DD4E65AAF7988B4")) return false;
      if (!do_tdes_test("C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7C7", "C7C7C7C7C7C7C7C7", "76BF084C1535A6C6")) return false;
      if (!do_tdes_test("C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8C8", "C8C8C8C8C8C8C8C8", "AFEC35B09D36315F")) return false;
      if (!do_tdes_test("C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9C9", "C9C9C9C9C9C9C9C9", "C8078A6148818403")) return false;
      if (!do_tdes_test("CACACACACACACACACACACACACACACACACACACACACACACACA", "CACACACACACACACA", "4DA91CB4124B67FE")) return false;
      if (!do_tdes_test("CBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCBCB", "CBCBCBCBCBCBCBCB", "2DABFEB346794C3D")) return false;
      if (!do_tdes_test("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "CCCCCCCCCCCCCCCC", "FBCD12C790D21CD7")) return false;
      if (!do_tdes_test("CDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCD", "CDCDCDCDCDCDCDCD", "536873DB879CC770")) return false;
      if (!do_tdes_test("CECECECECECECECECECECECECECECECECECECECECECECECE", "CECECECECECECECE", "9AA159D7309DA7A0")) return false;
      if (!do_tdes_test("CFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCF", "CFCFCFCFCFCFCFCF", "0B844B9D8C4EA14A")) return false;
      if (!do_tdes_test("D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0D0", "D0D0D0D0D0D0D0D0", "3BBD84CE539E68C4")) return false;
      if (!do_tdes_test("D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1D1", "D1D1D1D1D1D1D1D1", "CF3E4F3E026E2C8E")) return false;
      if (!do_tdes_test("D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2D2", "D2D2D2D2D2D2D2D2", "82F85885D542AF58")) return false;
      if (!do_tdes_test("D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3D3", "D3D3D3D3D3D3D3D3", "22D334D6493B3CB6")) return false;
      if (!do_tdes_test("D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4D4", "D4D4D4D4D4D4D4D4", "47E9CB3E3154D673")) return false;
      if (!do_tdes_test("D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5D5", "D5D5D5D5D5D5D5D5", "2352BCC708ADC7E9")) return false;
      if (!do_tdes_test("D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6D6", "D6D6D6D6D6D6D6D6", "8C0F3BA0C8601980")) return false;
      if (!do_tdes_test("D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7D7", "D7D7D7D7D7D7D7D7", "EE5E9FD70CEF00E9")) return false;
      if (!do_tdes_test("D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8D8", "D8D8D8D8D8D8D8D8", "DEF6BDA6CABF9547")) return false;
      if (!do_tdes_test("D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9D9", "D9D9D9D9D9D9D9D9", "4DADD04A0EA70F20")) return false;
      if (!do_tdes_test("DADADADADADADADADADADADADADADADADADADADADADADADA", "DADADADADADADADA", "C1AA16689EE1B482")) return false;
      if (!do_tdes_test("DBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDBDB", "DBDBDBDBDBDBDBDB", "F45FC26193E69AEE")) return false;
      if (!do_tdes_test("DCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDC", "DCDCDCDCDCDCDCDC", "D0CFBB937CEDBFB5")) return false;
      if (!do_tdes_test("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", "DDDDDDDDDDDDDDDD", "F0752004EE23D87B")) return false;
      if (!do_tdes_test("DEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDE", "DEDEDEDEDEDEDEDE", "77A791E28AA464A5")) return false;
      if (!do_tdes_test("DFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDF", "DFDFDFDFDFDFDFDF", "E7562A7F56FF4966")) return false;
      if (!do_tdes_test("E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0E0", "E0E0E0E0E0E0E0E0", "B026913F2CCFB109")) return false;
      if (!do_tdes_test("E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1E1", "E1E1E1E1E1E1E1E1", "0DB572DDCE388AC7")) return false;
      if (!do_tdes_test("E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2E2", "E2E2E2E2E2E2E2E2", "D9FA6595F0C094CA")) return false;
      if (!do_tdes_test("E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3E3", "E3E3E3E3E3E3E3E3", "ADE4804C4BE4486E")) return false;
      if (!do_tdes_test("E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4E4", "E4E4E4E4E4E4E4E4", "007B81F520E6D7DA")) return false;
      if (!do_tdes_test("E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5E5", "E5E5E5E5E5E5E5E5", "961AEB77BFC10B3C")) return false;
      if (!do_tdes_test("E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6E6", "E6E6E6E6E6E6E6E6", "8A8DD870C9B14AF2")) return false;
      if (!do_tdes_test("E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7E7", "E7E7E7E7E7E7E7E7", "3CC02E14B6349B25")) return false;
      if (!do_tdes_test("E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8E8", "E8E8E8E8E8E8E8E8", "BAD3EE68BDDB9607")) return false;
      if (!do_tdes_test("E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9E9", "E9E9E9E9E9E9E9E9", "DFF918E93BDAD292")) return false;
      if (!do_tdes_test("EAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEAEA", "EAEAEAEAEAEAEAEA", "8FE559C7CD6FA56D")) return false;
      if (!do_tdes_test("EBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEBEB", "EBEBEBEBEBEBEBEB", "C88480835C1A444C")) return false;
      if (!do_tdes_test("ECECECECECECECECECECECECECECECECECECECECECECECEC", "ECECECECECECECEC", "D6EE30A16B2CC01E")) return false;
      if (!do_tdes_test("EDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDEDED", "EDEDEDEDEDEDEDED", "6932D887B2EA9C1A")) return false;
      if (!do_tdes_test("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", "EEEEEEEEEEEEEEEE", "0BFC865461F13ACC")) return false;
      if (!do_tdes_test("EFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEF", "EFEFEFEFEFEFEFEF", "228AEA0D403E807A")) return false;
      if (!do_tdes_test("F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0F0", "F0F0F0F0F0F0F0F0", "2A2891F65BB8173C")) return false;
      if (!do_tdes_test("F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1F1", "F1F1F1F1F1F1F1F1", "5D1B8FAF7839494B")) return false;
      if (!do_tdes_test("F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2F2", "F2F2F2F2F2F2F2F2", "1C0A9280EECF5D48")) return false;
      if (!do_tdes_test("F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3F3", "F3F3F3F3F3F3F3F3", "6CBCE951BBC30F74")) return false;
      if (!do_tdes_test("F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4F4", "F4F4F4F4F4F4F4F4", "9CA66E96BD08BC70")) return false;
      if (!do_tdes_test("F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5F5", "F5F5F5F5F5F5F5F5", "F5D779FCFBB28BF3")) return false;
      if (!do_tdes_test("F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6F6", "F6F6F6F6F6F6F6F6", "0FEC6BBF9B859184")) return false;
      if (!do_tdes_test("F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7F7", "F7F7F7F7F7F7F7F7", "EF88D2BF052DBDA8")) return false;
      if (!do_tdes_test("F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8F8", "F8F8F8F8F8F8F8F8", "39ADBDDB7363090D")) return false;
      if (!do_tdes_test("F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9F9", "F9F9F9F9F9F9F9F9", "C0AEAF445F7E2A7A")) return false;
      if (!do_tdes_test("FAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFAFA", "FAFAFAFAFAFAFAFA", "C66F54067298D4E9")) return false;
      if (!do_tdes_test("FBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFBFB", "FBFBFBFBFBFBFBFB", "E0BA8F4488AAF97C")) return false;
      if (!do_tdes_test("FCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFCFC", "FCFCFCFCFCFCFCFC", "67B36E2875D9631C")) return false;
      if (!do_tdes_test("FDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFDFD", "FDFDFDFDFDFDFDFD", "1ED83D49E267191D")) return false;
      if (!do_tdes_test("FEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFEFE", "FEFEFEFEFEFEFEFE", "66B2B23EA84693AD")) return false;
      if (!do_tdes_test("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFF", "7359B2163E4EDC58")) return false;
      if (!do_tdes_test("000102030405060708090A0B0C0D0E0F1011121314151617", "0011223344556677", "97A25BA82B564F4C")) return false;
      if (!do_tdes_test("2BD6459F82C5B300952C49104881FF482BD6459F82C5B300", "EA024714AD5C4D84", "C616ACE843958247")) return false;
    
      if (!do_tdes_test("010101010101010101010101010101010101010101010101", "95F8A5E5DD31D900", "8000000000000000")) return false;
      if (!do_tdes_test("010101010101010101010101010101010101010101010101", "9D64555A9A10B852", "0000001000000000")) return false;
      if (!do_tdes_test("3849674C2602319E3849674C2602319E3849674C2602319E", "51454B582DDF440A", "7178876E01F19B2A")) return false;
      if (!do_tdes_test("04B915BA43FEB5B604B915BA43FEB5B604B915BA43FEB5B6", "42FD443059577FA2", "AF37FB421F8C4095")) return false;
      if (!do_tdes_test("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF", "736F6D6564617461", "3D124FE2198BA318")) return false;
      if (!do_tdes_test("0123456789ABCDEF55555555555555550123456789ABCDEF", "736F6D6564617461", "FBABA1FF9D05E9B1")) return false;
      if (!do_tdes_test("0123456789ABCDEF5555555555555555FEDCBA9876543210", "736F6D6564617461", "18d748e563620572")) return false;
      if (!do_tdes_test("0352020767208217860287665908219864056ABDFEA93457", "7371756967676C65", "c07d2a0fa566fa30")) return false;
      if (!do_tdes_test("010101010101010180010101010101010101010101010102", "0000000000000000", "e6e6dd5b7e722974")) return false;
      if (!do_tdes_test("10461034899880209107D0158919010119079210981A0101", "0000000000000000", "e1ef62c332fe825b")) return false;
    
      return true;
	  }
	  
	  public byte[] ComputeMAC(byte[] data)
	  {
	    byte[] buffer;
	    
#if _DEBUG_MAC
      Logger.Trace("MAC(" + (new CardBuffer(data)).AsString() + ")");
#endif
	    
	    if ((data.Length % 8) != 0)
	    {
	      int l = data.Length;	      
	      while ((l % 8) != 0) l++;
	      buffer = new byte[l];
	      for (int i=0; i<data.Length; i++)
	        buffer[i] = data[i];
	    } else
	    {
	      buffer = data;
	    }

#if _DEBUG_MAC
      Logger.Trace("    " + (new CardBuffer(buffer)).AsString());
#endif

	    byte[] carry = new byte[8];
	    
	    for (int i=0; i<buffer.Length; i+=8)
	    {
	      byte[] t = new byte[8];
	      for (int j=0; j<8; j++)
	      {
	        t[j]  = buffer[i + j];
	        t[j] ^= carry[j];
	      }
	      
				core.Encrypt(ref t);
				
				for (int j=0; j<8; j++)
				  carry[j] = t[j];
	    }
	    
	    byte[] mac = new byte[4];
	    for (int i=0; i<4; i++)
	      mac[i] = carry[i];
	    
	    return mac;	    
	  }
	  
	  public byte[] AppendMAC(byte[] data)
	  {    
	    byte[] mac = ComputeMAC(data);

	    byte[] result = new byte[data.Length + mac.Length];
	    
	    for (int i=0; i<data.Length; i++)
	      result[i] = data[i];
	    
	    for (int i=0; i<mac.Length; i++)
	      result[data.Length + i] = mac[i];
	    
	    return result;
	  }

	  public bool CheckMAC(byte[] buffer)
	  {    
	    if (buffer.Length < 4)
	      return false;
	    
	    byte[] data = new byte[buffer.Length - 4];
	    for (int i=0; i<data.Length; i++)
	      data[i] = buffer[i];
	    
	    byte[] mac = ComputeMAC(data);

	    for (int i=0; i<4; i++)
	      if (mac[i] != buffer[data.Length + i])
	        return false;

	    return true;
	  }

	  private void UpdateCRC16(byte ch, ref ushort lpwCrc)
    {
	    ch = (byte) (ch ^ (byte) ((lpwCrc) & 0x00FF));
    
	    ch = (byte) (ch ^ ((ch * 0x10) & 0xF0));
    
	    lpwCrc = (ushort) ((lpwCrc / 0x0100) ^ ((ushort) ch * 0x0100) ^ ((ushort) ch * 0x0008) ^ ((ushort) ch / 0x0010));
    }

	  private ushort ComputeCRC(byte[] data)
	  {
      ushort wCrc = 0x6363; /* ITU-V.41 */
      
      if (data != null)
      {
        for (int i=0; i<data.Length; i++)
          UpdateCRC16(data[i], ref wCrc);
      }

      return wCrc;
	  }
	  
	  public bool AppendCRC(ref byte[] data)
	  {    
	    ushort crc = ComputeCRC(data);

	    if (data != null)
	    {
  	    byte[] result = new byte[data.Length + 2];
  	    
  	    for (int i=0; i<data.Length; i++)
  	      result[i] = data[i];
  	    
  	    result[data.Length + 0] = (byte) (crc % 0x0100);
  	    result[data.Length + 1] = (byte) (crc / 0x0100);
  	    
  	    data = result;

	    } else
	    {
	      byte[] result = new byte[2];

	      result[0] = (byte) (crc % 0x0100);
  	    result[1] = (byte) (crc / 0x0100);
  	    
  	    data = result;
	    }
	    
	    return true;
	  }

	}
}