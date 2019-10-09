using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace SpringCard.LibCs.Crypto
{
	public class ECC
	{
		private string Curve;
		private string Algorithm = "ECDSA";
		
		public ECC(string Curve = "secp128r1")
		{
			this.Curve = Curve;
		}
		
		private bool Verify(byte[] hash, byte[] signature, ECPublicKeyParameters pubKey)
		{
			ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");

			signer.Init(false, pubKey);
			signer.BlockUpdate(hash, 0, hash.Length);
			
			signature = derEncodeSignature(signature);
			if (signer.VerifySignature(signature))
			{
				System.Console.WriteLine("Signature is OK!");
				return true;
			}
			else
			{
				System.Console.WriteLine("Not Verified Signature");
				return false;
			}
		}
		
		public bool Verify(byte[] hash, byte[] signature, byte[] pubKey)
		{			
			return Verify(hash, signature, ImportPublicKey(pubKey));
		}
		
		private byte[] Sign(byte[] hash, ECPrivateKeyParameters privKey, byte[] seed = null)
		{
			ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");

			if (seed != null)
			{
                SecureRandom random = SecureRandom.GetInstance("SHA256PRNG", true);
				signer.Init(true, new ParametersWithRandom(privKey, random));
			}
			else
			{
				signer.Init(true, privKey);
			}
			signer.BlockUpdate(hash, 0, hash.Length);

			byte[] result = signer.GenerateSignature();
			
			result = derDecodeSignature(result);
			return result;
		}

		public byte[] Sign(byte[] hash, byte[] privKey, byte[] seed = null)
		{
			return Sign(hash, ImportPrivateKey(privKey), seed);
		}
		
		private class ECKeyPair
		{
			public ECPublicKeyParameters pub;
			public ECPrivateKeyParameters priv;
		}
		
		private ECKeyPair GenerateKeyPair()
		{
			X9ECParameters curve = SecNamedCurves.GetByName(Curve);
			ECKeyPairGenerator generator = new ECKeyPairGenerator(Algorithm);
			
			SecureRandom random = new SecureRandom();
			KeyGenerationParameters parameters = new KeyGenerationParameters(random, 128);
			
			generator.Init(parameters);
			AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
			
			ECKeyPair result = new ECKeyPair();
			
			result.pub = (ECPublicKeyParameters) keyPair.Public;
			result.priv = (ECPrivateKeyParameters) keyPair.Private;
			
			return result;
		}
		
		private byte[] ExportPublicKey(ECPublicKeyParameters pubKey)
		{
			byte[] result = pubKey.Q.GetEncoded();
			if ((result.Length == 33) && (result[0] == 0x04))
			{
				result = result.RangeSubset(1, 32);
			}
			
			return result;
		}
		
		private ECPublicKeyParameters ImportPublicKey(byte[] pubKey)
		{
			if (pubKey.Length == 32)
			{
				byte[] completePubKey = new byte[33];
				completePubKey[0] = 0x04;
				Array.Copy(pubKey, 0, completePubKey, 1, 32);
				pubKey = completePubKey;
			}
			
			X9ECParameters curve = SecNamedCurves.GetByName(Curve);
			ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
			ECPublicKeyParameters result = new ECPublicKeyParameters(Algorithm, curve.Curve.DecodePoint(pubKey), curveSpec);			
			return result;
		}
		
		private byte[] ExportPrivateKey(ECPrivateKeyParameters privKey)
		{
			byte[] result = privKey.D.ToByteArray();
			return result;
		}

		private ECPrivateKeyParameters ImportPrivateKey(byte[] privKey)
		{
			X9ECParameters curve = SecNamedCurves.GetByName(Curve);
			ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
			ECPrivateKeyParameters result = new ECPrivateKeyParameters(Algorithm, new BigInteger(privKey), curveSpec);
			return result;
		}
		
		public static bool SelfTest()
		{
			ECC ecc = new ECC();
			
			{
				Console.WriteLine("Test with locally created keys...");
				ECKeyPair keyPair = ecc.GenerateKeyPair();

				byte[] pubKey = ecc.ExportPublicKey(keyPair.pub);				
				byte[] privKey = ecc.ExportPrivateKey(keyPair.priv);
				
				byte[] hash = new byte[] { 0x1, 0x2, 0x3, 0x4 };
				byte[] signature = ecc.Sign(hash, privKey);
				
				if (!ecc.Verify(hash, signature, pubKey)) return false;
				
				Console.WriteLine("pub= " + BinConvert.ToHex(pubKey));
				Console.WriteLine("crc32= " + BinConvert.ToHex(CRC32.Hash(pubKey)));				
				Console.WriteLine("priv= " + BinConvert.ToHex(privKey));
			}

			{
				Console.WriteLine("Test with transport key...");

				byte[] pubKey = Hex.Decode("0e74d0da506108d39bcd8af1f53a66b00492736cdc43ac78e8478e00daa27dbb");
				byte[] privKey = Hex.Decode("2C1CEF2D329F05B74DAF9AFCE5991C67");

				Console.WriteLine("pub= " + BinConvert.ToHex(pubKey));
				Console.WriteLine("crc32= " + BinConvert.ToHex(CRC32.Hash(pubKey)));				
				Console.WriteLine("priv= " + BinConvert.ToHex(privKey));
				
				byte[] hash = new byte[] { 0x1, 0x2, 0x3, 0x4 };
				byte[] signature = ecc.Sign(hash, privKey);
				if (!ecc.Verify(hash, signature, pubKey)) return false;
			}
			
			{
				Console.WriteLine("Sample keys from BouncyCastle...");
				byte[] hash = new byte[] { 0x2B, 0xA1, 0x41, 0x00 };
				string pubkey = "4F6D3F294DEA5737F0F46FFEE88A356EED95695DD7E0C27A591E6F6F65962BAF";
				string signature = "AAD03D3D38CE53B673CF8F1C016C8D3B67EA98CBCF72627788368C7C54AA2FC4";
				if (!ecc.Verify(hash, Hex.Decode(signature), Hex.Decode(pubkey))) return false;
			}
			
			{
				Console.WriteLine("NXP Mifare UL EV1 authenticity...");
				byte[] hash =  { 0x04, 0xcf, 0xd7, 0x1a, 0x08, 0x40, 0x80};
				byte[] signature =
				{	0x1b, 0x27, 0xe9, 0x4b, 0x9b, 0xd4, 0xaf, 0x3b,
					0xdb, 0x77, 0xd2, 0x35, 0x9f, 0x3f, 0x99, 0x97,
					0xc2, 0xc9, 0x9d, 0x04, 0xac, 0xa6, 0x18, 0x96,
					0xcc, 0x19, 0x9e, 0x1f, 0x42, 0x34, 0xf9, 0xf1 };
				byte[] pubkey =
				{	0x04,
					0x90, 0x93, 0x3B, 0xDC, 0xD6, 0xE9, 0x9B, 0x4E,
					0x25, 0x5E, 0x3D, 0xA5, 0x53, 0x89, 0xA8, 0x27,
					0x56, 0x4E, 0x11, 0x71, 0x8E, 0x01, 0x72, 0x92,
					0xFA, 0xF2, 0x32, 0x26, 0xA9, 0x66, 0x14, 0xB8 };
				if (!ecc.Verify(hash, signature, pubkey)) return false;
			}
			
			return true;
		}
		
		private static byte[] derEncodeSignature(byte[] signature)
		{
			byte[] r = signature.RangeSubset(0, (signature.Length / 2));
			byte[] s = signature.RangeSubset((signature.Length / 2), (signature.Length / 2));

			MemoryStream stream = new MemoryStream();
			DerOutputStream der = new DerOutputStream(stream);

			Asn1EncodableVector v = new Asn1EncodableVector();
			v.Add(new DerInteger(new BigInteger(1, r)));
			v.Add(new DerInteger(new BigInteger(1, s)));
			der.WriteObject(new DerSequence(v));

			return stream.ToArray();
		}
		
		private static byte[] derDecodeSignature(byte[] signature)
		{
			Asn1InputStream asnInputStream = new Asn1InputStream(signature);			
			Asn1Object asnObject = asnInputStream.ReadObject();
			
			if (asnObject is DerSequence)
			{
				DerSequence derSequence = (DerSequence) asnObject;
				if (derSequence.Count == 2)
				{
					if ((derSequence[0] is DerInteger) && (derSequence[1] is DerInteger))
					{
						byte[] r = ((DerInteger) derSequence[0]).Value.ToByteArray();
						while (r[0] == 0x00)
							r = r.RangeSubset(1, r.Length-1);
						byte[] s = ((DerInteger) derSequence[1]).Value.ToByteArray();
						while (s[0] == 0x00)
							s = s.RangeSubset(1, s.Length-1);						
						
						int l = r.Length;
						if (s.Length > l) l = s.Length;
						byte[] result = new byte[2 * l];
						
						Array.Copy(r, 0, result, 0 + l - r.Length, r.Length);
						Array.Copy(s, 0, result, l + l - s.Length, s.Length);
						return result;
					}
				}
			}

			return null;
		}
		
	}
	
	internal static class ArrayUtilities
	{
		// create a subset from a range of indices
		public static T[] RangeSubset<T>(this T[] array, int startIndex, int length)
		{
			T[] subset = new T[length];
			Array.Copy(array, startIndex, subset, 0, length);
			return subset;
		}

		// create a subset from a specific list of indices
		public static T[] Subset<T>(this T[] array, params int[] indices)
		{
			T[] subset = new T[indices.Length];
			for (int i = 0; i < indices.Length; i++)
			{
				subset[i] = array[indices[i]];
			}
			return subset;
		}
	}
	
}
