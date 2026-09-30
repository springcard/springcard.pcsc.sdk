/**
 *
 * \ingroup AppleVAS
 * 
 */
using Org.BouncyCastle.Crypto.Parameters;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.AppleVas
{

    abstract internal class PrivateKey
    {
        public ECPublicKeyParameters PublicPart { get; protected set; }
        public uint KeyId { get; protected set; }
        public abstract bool IsInitialized();
    }

    internal class PrivateKeySecureElement : PrivateKey
    {
        public byte KeyIndex;
        public ICardApduTransmitter CardChannel;
        bool _initialized;

        public override bool IsInitialized()
        {
            return _initialized;
        }

        public PrivateKeySecureElement(byte keyIndex) : base()
        {
            KeyIndex = keyIndex;
        }

        public bool Initialize(ICardApduTransmitter cardChannel)
        {
            CardChannel = cardChannel;

            if (cardChannel == null)
            {
                Logger.Error("CardChannel has not benn initialized");
                return false;
            }

            if (!AppleVasCrypto.ECC.ExtractPublicKey(KeyIndex, cardChannel, out ECPublicKeyParameters publicPart))
            {
                Logger.Error("Failed to extract public key");
                return false;
            }

            PublicPart = publicPart;
            KeyId = AppleVasCrypto.ECC.ComputeKeyId(PublicPart);
            _initialized = true;
            return true;
        }

        public static PrivateKeySecureElement Create(ICardApduTransmitter cardChannel, byte keyIndex)
        {
            PrivateKeySecureElement result = new PrivateKeySecureElement(keyIndex);
            if (!result.Initialize(cardChannel))
                return null;
            return result;
        }
    }

    internal class PrivateKeySoftware : PrivateKey
    {
        public ECPrivateKeyParameters PrivatePart;

        public override bool IsInitialized()
        {
            return (PrivatePart != null) && (PublicPart != null);
        }

        /**
		 * \brief Verify that a private key is valid, store its ID and return ist associated public key
		 **/
        public static PrivateKeySoftware Create(ECPrivateKeyParameters privateKey)
        {
            PrivateKeySoftware result = new PrivateKeySoftware();
            result.PrivatePart = privateKey;
            result.PublicPart = AppleVasCrypto.ECC.ExtractPublicKey(privateKey);
            result.KeyId = AppleVasCrypto.ECC.ComputeKeyId(result.PublicPart);
            return result;
        }

        public static PrivateKeySoftware Create(byte[] encodedPrivateKey)
        {
            ECPrivateKeyParameters privateKey = AppleVasCrypto.ECC.ImportPrivateKey(encodedPrivateKey);
            if (privateKey == null)
                return null;
            return Create(privateKey);
        }
    }

    /**
	 * \brief Encapsulate the Apple VAS Merchant's data: its Merchant ID and its Private key(s)
	 **/
    public class AppleVasConfig
    { 
        internal List<PrivateKey> privateKeys = new List<PrivateKey>();

        /**
		 * \brief The Merchant ID
		 **/
        public byte[] Id
        {
            get;
            private set;
        }

        /**
		 * \brief The Merchant URL
		 **/
        public string Url;

        private void Init(byte[] Id, byte[] encodedPrivateKey, out byte[] encodedPublicKey)
        {
            this.Id = Id;
            if (encodedPrivateKey != null)
                AddPrivateKey(encodedPrivateKey, out encodedPublicKey);
            else
                encodedPublicKey = null;
        }

        internal AppleVasConfig()
        {

        }

        /**
		 * \brief Create a new Merchant object for the given Merchant name (Merchant ID = SHA256 of Merchant name).
         * The Private Key will be provided later on.
		 **/
        public AppleVasConfig(string Name)
        {
            byte[] Id = ComputeId(Name);
            Init(Id, null, out byte[] dummy);
        }

        /**
		 * \brief Create a new Merchant object for the given Merchant name (Merchant ID = SHA256 of Merchant name).
         * Specify the Merchant's Private Key right now.
		 **/
        public AppleVasConfig(string Name, byte[] encodedPrivateKey)
        {
            byte[] Id = ComputeId(Name);
            Init(Id, encodedPrivateKey, out byte[] dummy);
        }

        /**
		 * \brief Create a new Merchant object for the given Merchant name (Merchant ID = SHA256 of Merchant name)
         * Specify the Merchant's Private Key right now (and retrieve the corresponding Public Key).
		 **/
        public AppleVasConfig(string Name, byte[] encodedPrivateKey, out byte[] encodedPublicKey)
        {
            byte[] Id = ComputeId(Name);
            Init(Id, encodedPrivateKey, out encodedPublicKey);
        }

        /**
		 * \brief Create a new Merchant object for the given Merchant ID
         * The Private Key will be provided later on.
		 **/
        public AppleVasConfig(byte[] Id)
        {
            Init(Id, null, out byte[] dummy);
        }

        /**
		 * \brief Create a new Merchant object for the given Merchant ID
         * Specify the Merchant's Private Key right now.
		 **/
        public AppleVasConfig(byte[] Id, byte[] encodedPrivateKey)
        {
            Init(Id, encodedPrivateKey, out byte[] dummy);
        }

        /**
		 * \brief Create a new Merchant object for the given Merchant ID
         * Specify the Merchant's Private Key right now (and retrieve the corresponding Public Key).
		 **/
        public AppleVasConfig(byte[] Id, byte[] encodedPrivateKey, out byte[] encodedPublicKey)
        {
            Init(Id, encodedPrivateKey, out encodedPublicKey);
        }

        /**
		 * \brief Compute a Merchant ID from a Merchant name (this is the SHA256 function)
		 **/
        public static byte[] ComputeId(string Name)
        {
            return AppleVasCrypto.SHA256(Encoding.UTF8.GetBytes(Name));
        }

        /**
		 * \brief Reset the Merchant's keyset
		 **/
        public void ClearPrivateKeys()
        {
            privateKeys.Clear();
        }

        /**
         * \brief Add the index of the private key in the Secure Element to the Merchant's keyset.
         * The function returns always true, the validation will be done later when we will have an handle on the device.
         **/
        public bool AddPrivateKeyIndex(byte index)
        {
            Logger.Debug("Merchant: adding private key with Index={0}, KeyId will be retrived later", index);
            privateKeys.Add(new PrivateKeySecureElement(index));
            return true;
        }

        /**
		 * \brief Add a new private key to the Merchant's keyset and return the public key.
         * The function returns false if the private key is invalid.
		 **/
        public bool AddPrivateKey(byte[] encodedPrivateKey, out byte[] encodedPublicKey)
        {
            PrivateKeySoftware t = PrivateKeySoftware.Create(encodedPrivateKey);

            if (t == null)
            {
                Logger.Error("Merchant's PrivateKey is invalid");
                encodedPublicKey = null;
                return false;
            }

            Logger.Debug("Adding Merchant' PrivateKey with ID={0}", BinConvert.ToHex(t.KeyId));
            encodedPublicKey = AppleVasCrypto.ECC.EncodePublic(t.PublicPart);
            privateKeys.Add(t);
            return true;
        }

        /**
		 * \brief Add a new private key to the Merchant's keyset.
         * The function returns false if the private key is invalid.
		 **/
        public bool AddPrivateKey(byte[] encodedPrivateKey)
        {
            return AddPrivateKey(encodedPrivateKey, out byte[] dummy);
        }

        /**
           * \brief Verify that a private key is valid
          **/
        public static bool ValidatePrivateKey(byte[] encodedPrivateKey)
        {
            uint dummy;
            return ValidatePrivateKey(encodedPrivateKey, out dummy);
        }

        /**
		 * \brief Verify that a private key is valid, and return its ID
		 **/
        public static bool ValidatePrivateKey(byte[] encodedPrivateKey, out uint keyId)
        {
            ECPrivateKeyParameters dummyPrivate;
            ECPublicKeyParameters dummyPublic;
            return GetKeyData(encodedPrivateKey, out keyId, out dummyPrivate, out dummyPublic);
        }

        /**
		 * \brief Verify that a private key is valid, and return its ID and associated public key
		 **/
        public static bool ValidatePrivateKey(byte[] encodedPrivateKey, out uint keyId, out byte[] encodedPublicKey)
        {
            ECPrivateKeyParameters dummyPrivate;
            ECPublicKeyParameters publicKey;
            if (GetKeyData(encodedPrivateKey, out keyId, out dummyPrivate, out publicKey))
            {
                encodedPublicKey = AppleVasCrypto.ECC.EncodePublic(publicKey);
                return true;
            }
            encodedPublicKey = null;
            return false;
        }

        private static bool GetKeyData(byte[] encodedPrivateKey, out uint keyId, out ECPrivateKeyParameters privateKey, out ECPublicKeyParameters publicKey)
        {
            PrivateKeySoftware key = PrivateKeySoftware.Create(encodedPrivateKey);
            if (key == null)
            {
                keyId = 0;
                privateKey = null;
                publicKey = null;
            }
            keyId = key.KeyId;
            privateKey = key.PrivatePart;
            publicKey = key.PublicPart;
            return true;
        }

        public static bool GetKeyData(ECPrivateKeyParameters privateKey, out uint keyId, out ECPublicKeyParameters publicKey)
        {
            PrivateKeySoftware key = PrivateKeySoftware.Create(privateKey);
            if (key == null)
            {
                keyId = 0;
                publicKey = null;
            }
            keyId = key.KeyId;
            publicKey = key.PublicPart;
            return true;
        }

        public static bool GetPrivateKeyData(ECPrivateKeyParameters privateKey, out uint keyId, out byte[] encodedPublicKey)
        {
            keyId = 0;
            encodedPublicKey = null;

            if (privateKey == null)
                return false;

            PrivateKeySoftware key = PrivateKeySoftware.Create(privateKey);
            if (key == null)
                return false;

            keyId = key.KeyId;
            encodedPublicKey = AppleVasCrypto.ECC.EncodePublic(key.PublicPart);
            return true;
        }


        /**
		 * \brief List the IDs of the private keys that have been added
         * 
         * WARNING! PrivateKeyIds will containes 0s if some keys 
         * used are stored in the device's secure element and we
         * have not run a transaction yet.
         * This is because we need to access to the device to get
         * the public key that will be used to compute the KeyId.
		 **/
        public uint[] PrivateKeyIds
        {
            get
            {
                List<uint> r = new List<uint>();
                foreach (PrivateKey entry in privateKeys)
                    r.Add(entry.KeyId);
                return r.ToArray();
            }
        }

        internal PrivateKey GetPrivateKey(uint keyId)
        {
            foreach (PrivateKey entry in privateKeys)
            {
                /* return 1st element which match keyId */ 
                if(entry.KeyId == keyId)
                {
                    return entry;
                }
            }
            Logger.Debug("Merchant: no private key with ID={0}", BinConvert.ToHex(keyId));
            return null;
        }

        /**
         * \brief Import a merchant configuration form a JSON object
         **/
        public static AppleVasConfig LoadFromJson(JSONObject jsonData)
        {
            string strName = jsonData.GetString("Name");
            string strId = jsonData.GetString("Id");
            string strPrivateKey = jsonData.GetString("PrivateKey");
            string strUrl = jsonData.GetString("Url");

            if (string.IsNullOrEmpty(strId) && string.IsNullOrEmpty(strName))
                throw new InvalidDataException("You must specify either the Name or the Id of the merchant, or an URL");

            if (!string.IsNullOrEmpty(strId) && !string.IsNullOrEmpty(strName))
                throw new InvalidDataException("You may specify either the Name or the Id of the merchant, not both");

            AppleVasConfig result = null;

            if (!string.IsNullOrEmpty(strId))
            {
                Logger.Debug("Adding Merchant Id: {0}", strId);
                result = new AppleVasConfig(BinConvert.HexToBytes(strId));
            }

            if (!string.IsNullOrEmpty(strName))
            {
                Logger.Debug("Adding Merchant Name: {0}", strName);
                result = new AppleVasConfig(strName);
            }

            if (!string.IsNullOrEmpty(strPrivateKey))
            {

                if(strPrivateKey.Length < 3)
                {
                    if (byte.TryParse(strPrivateKey, out byte j))
                    {
                        if(j>16)
                        {
                            Logger.Error("Index of the key too high: {0}", j);
                            throw new InvalidDataException("Index of the key too high");
                        }
                        Logger.Debug("Adding Merchant private key");
                        result.AddPrivateKeyIndex(j);
                    }
                    else
                    {

                        Logger.Error("Could not parse key version: {0}", strPrivateKey);
                        throw new InvalidDataException("Could not parse key version:");
                    }
                }
                else
                {
                    Logger.Debug("Adding Merchant private key");
                    //result.AddPrivateKeyIndex(0x0C); // If we want to use default key at index 0x0C of the Secure element
                    result.AddPrivateKey(StrUtils.Base64Decode(strPrivateKey));
                }
  
            }

            if (!string.IsNullOrEmpty(strUrl))
            {
                Logger.Debug("Adding Merchant URL: {0}", strUrl);
                result.Url = strUrl;
            }

            if (result == null)
                throw new InvalidDataException("Empty merchant config");

            return result;
        }

        /**
		 * \brief Export a public key in PEM format
		 **/
        public static byte[] EncodePublicPem(byte[] buffer)
        {
            Asn1Tlv result = new Asn1Tlv(0x30,
                new Asn1Tlv(0x30,
                    new Asn1Tlv(0x06, BinConvert.HexToBytes("2A8648CE3D0201")),
                    new Asn1Tlv(0x06, BinConvert.HexToBytes("2A8648CE3D030107"))
                ),
                new Asn1Tlv(0x03, BinUtils.Concat(0x00, buffer))
            );
            return result.Serialize();
        }

        public static AppleVasConfig SpringCardDemo()
        {
            AppleVasConfig result = new AppleVasConfig(
                "pass.com.springcard.springblue.generic",
                StrUtils.Base64Decode("MHcCAQEEICp+PT7K8FQSOi2HED1Ar5RqxxN2EkiKJMCSfaL4htYNoAoGCCqGSM49AwEHoUQDQgAE9RCZaHxXUIjQFQnwKmq6+cVqFBNO6ZKQmekosMQRZmutPs8szUsiLokILdaiT/7F5qUl8qSfEvlocYy6z98jIw=="));
            return result;
        }
    }
}
