/**
 *
 * \defgroup Desfire
 *
 * \brief Desfire library (.NET only, no native depedency)
 *
 * \copyright
 *   Copyright (c) 2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace SpringCard.PCSC.CardHelpers
{
	/// <summary>
	/// Description of DESFire.
	/// </summary>
	public partial class Desfire
	{
		byte isoWrapping;
    
		UInt32 current_aid;
    
		byte session_type;
		byte[] session_key;
		int session_key_id;
    
		UInt32 xfer_length;
		byte[] xfer_buffer;
    
		byte[] init_vector;
		byte[] cmac_subkey_1;
		byte[] cmac_subkey_2;
    
		ICardTransmitter transmitter;

        /* export current context */
        public bool ExportContext(ref byte TisoWrapping, ref UInt32 Tcurrent_aid, ref byte Tsession_type, out byte[] Tsession_key, ref int Tsession_key_id, out byte[] Tinit_vector, out byte[] Tcmac_subkey_1, out byte[] Tcmac_subkey_2)
        {
            bool rc = true;
            
            TisoWrapping = isoWrapping;
            Tcurrent_aid = current_aid;
            Tsession_type = session_type;            
            Tsession_key_id = session_key_id;

            if (init_vector == null)
            {
                rc = false;                
            }

            /* create new array copy */
            Tsession_key = session_key;
            Tinit_vector = init_vector;
            Tcmac_subkey_1 = cmac_subkey_1;
            Tcmac_subkey_2 = cmac_subkey_2; 

            return rc;
        }

        /* Import current context */
        public bool ImportContext(byte TisoWrapping, UInt32 Tcurrent_aid, byte Tsession_type, byte[] Tsession_key, int Tsession_key_id, byte[] Tinit_vector, byte[] Tcmac_subkey_1, byte[] Tcmac_subkey_2)
        {
            isoWrapping = TisoWrapping;
            current_aid = Tcurrent_aid;
            session_type = Tsession_type;
            if (Tsession_key != null)
            {
                session_key = new byte[Tsession_key.Length];
                session_key = Tsession_key;
            }
            
            session_key_id = Tsession_key_id;

            if (Tinit_vector != null)
            {
                init_vector = new byte[Tinit_vector.Length];
                init_vector = Tinit_vector;
            }

            if (Tcmac_subkey_1 != null)
            {
                cmac_subkey_1 = new byte[Tcmac_subkey_1.Length];
                cmac_subkey_1 = Tcmac_subkey_1;
            }

            if (Tcmac_subkey_2 != null)
            {
                cmac_subkey_2 = new byte[Tcmac_subkey_2.Length];
                cmac_subkey_2 = Tcmac_subkey_2;
            }
            
            return true;
        }

        public void IsoWrapping(byte mode)
		{
			isoWrapping = mode;
		}

		/**
		 * \brief Instanciate a Desfire card object over a card channel. The channel must already be connected.
		 */
		public Desfire(ICardTransmitter transmitter, byte isoWrapping = DF_ISO_WRAPPING_OFF)
		{
			this.transmitter = transmitter;
			this.isoWrapping = isoWrapping;
			xfer_buffer = new byte[64];
			init_vector = new byte[16];
		}
	}
}
