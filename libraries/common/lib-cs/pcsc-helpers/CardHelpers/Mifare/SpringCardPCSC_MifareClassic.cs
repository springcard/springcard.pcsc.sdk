/**
 *
 * \defgroup MifareClassic
 *
 * \brief Mifare Classic library (.NET only, no native depedency)
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardHelpers.Mifare
{
	public class MifareClassic : CardHelperMifare
    {
		/**
		 * \brief Instanciate a Mifare UltraLight card object over a card channel. The channel must already be connected.
		 */
		public MifareClassic(ICardApduTransmitter Channel) : base(Channel)
		{

		}

        private static List<byte[]> _WellKnownKeys = null;
        public static List<byte[]> WellKnownKeys()
        {
            if (_WellKnownKeys == null)
            {
                _WellKnownKeys = new List<byte[]>();
                _WellKnownKeys.Add(new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); /* Transport */
                _WellKnownKeys.Add(new byte[6] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 }); /* MAD base A */
                _WellKnownKeys.Add(new byte[6] { 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5 }); /* MAD base B */
                _WellKnownKeys.Add(new byte[6] { 0x31, 0x4B, 0x49, 0x47, 0x49, 0x56 }); /* VIGIK */
                _WellKnownKeys.Add(new byte[6] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 }); /* NFC Forum */

                foreach (byte[] key in _WellKnownKeys)
                {
                    Logger.Debug("Adding Mifare Classic well-known-key {0}", BinConvert.ToHex(key));
                }
            }

            return _WellKnownKeys;
        }

        public byte[] ReadWithDeviceKeys(byte BlockNumber, byte ReadLength = 0)
        {
            if (ReadLength == 0)
            {
                /* Default is to read 1 block */
                ReadLength = 16;
                /* Are we aligned on a sector boundary? */
                if (BlockNumber < 32)
                {
                    if ((BlockNumber % 4) == 0)
                    {
                        /* Yes! Small sector */
                        ReadLength = 48;
                    }
                }
                else
                {
                    if (((BlockNumber - 32) % 16) == 0)
                    {
                        /* Yes! Large sector */
                        ReadLength = 240;
                    }
                }
            }

            switch (ReadLength)
            {
                case 16:
                case 48:
                case 240:
                    break;
                default:
                    throw new ArgumentException("ReadLength must be either 16, 48 or 240");
            }

            CAPDU capdu = new CAPDU(
                0xFF, // Embedded command interpreter
                0xF3, // Read Mifare Classic
                0x00,
                BlockNumber,
                ReadLength
            );

            RAPDU rapdu = Channel.Transmit(capdu);

            if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == ReadLength))
                return rapdu.data.GetBytes();

            return null;
        }

        public byte[] ReadWithKey(byte BlockNumber, byte[] AuthKey, byte ReadLength = 0)
        {
            if ((AuthKey == null) || (AuthKey.Length != 6))
                throw new ArgumentException("AuthKey must be a 6-B array");

            if (ReadLength == 0)
            {
                /* Default is to read 1 block */
                ReadLength = 16;
                /* Are we aligned on a sector boundary? */
                if (BlockNumber < 32)
                {
                    if ((BlockNumber % 4) == 0)
                    {
                        /* Yes! Small sector */
                        ReadLength = 48;
                    }
                }
                else
                {
                    if (((BlockNumber - 32) % 16) == 0)
                    {
                        /* Yes! Large sector */
                        ReadLength = 240;
                    }
                }
            }

            switch (ReadLength)
            {
                case 16:
                case 48:
                case 240:
                    break;
                default:
                    throw new ArgumentException("ReadLength must be either 16, 48 or 240");
            }

            CAPDU capdu = new CAPDU(
                0xFF, // Embedded command interpreter
                0xF3, // Read Mifare Classic
                0x00,
                BlockNumber,
                AuthKey,
                ReadLength
            );

            RAPDU rapdu = Channel.Transmit(capdu);

            if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == ReadLength))
                return rapdu.data.GetBytes();

            return null;
        }

        public byte[] ReadWithKeyList(byte BlockNumber, List<byte[]>AuthKeys, out int AuthKeyIndex, byte ReadLength = 0)
        {
            if (AuthKeys == null)
                throw new ArgumentException("AuthKeys can't be null");

            for (AuthKeyIndex = 0; AuthKeyIndex < AuthKeys.Count; AuthKeyIndex++)
            {
                Logger.Debug("Trying key {0}/{1}", AuthKeyIndex, AuthKeys.Count);
                byte[] data = ReadWithKey(BlockNumber, AuthKeys[AuthKeyIndex], ReadLength);
                if (data != null)
                    return data;
            }

            AuthKeyIndex = -1;
            return null;
        }

        public byte[] ReadWithKeyList(byte BlockNumber, List<byte[]> AuthKeys, int SuggestedAuthKeyIndex, out int ActualAuthKeyIndex, byte ReadLength = 0)
        {
            if (AuthKeys == null)
                throw new ArgumentException("AuthKeys can't be null");

            if ((SuggestedAuthKeyIndex >= 0) && (SuggestedAuthKeyIndex < AuthKeys.Count))
            {
                Logger.Debug("Trying key {0}/{1}", SuggestedAuthKeyIndex, AuthKeys.Count);
                byte[] data = ReadWithKey(BlockNumber, AuthKeys[SuggestedAuthKeyIndex], ReadLength);
                if (data != null)
                {
                    ActualAuthKeyIndex = SuggestedAuthKeyIndex;
                    return data;
                }
            }

            for (ActualAuthKeyIndex = 0; ActualAuthKeyIndex < AuthKeys.Count; ActualAuthKeyIndex++)
            {
                if (ActualAuthKeyIndex == SuggestedAuthKeyIndex)
                    continue; /* Already tried earlier */

                Logger.Debug("Trying key {0}/{1}", ActualAuthKeyIndex, AuthKeys.Count);
                byte[] data = ReadWithKey(BlockNumber, AuthKeys[ActualAuthKeyIndex], ReadLength);
                if (data != null)
                    return data;
            }

            ActualAuthKeyIndex = -1;
            return null;
        }

        public override bool SelfTest()
		{
			return true;
		}
	}
}

