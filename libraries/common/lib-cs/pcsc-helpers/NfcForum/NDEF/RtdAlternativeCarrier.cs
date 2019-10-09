/**h* SpringCard.NfcForum.Ndef/RtdAlternativeCarrier
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Alternative Carrier class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2013
 *   See LICENSE.TXT for information
 *
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/RtdAlternativeCarrier
	 *
	 * NAME
	 *   RtdAlternativeCarrier
	 * 
	 * DESCRIPTION
	 *   Represents an Alternative Carrier Record in a Handover Selector NDEF message
	 *
	 * SYNOPSIS
	 *   RtdAlternativeCarrier alternativeCarrier = new RtdAlternativeCarrier()
	 *   RtdAlternativeCarrier alternativeCarrier = new RtdAlternativeCarrier(byte[] payload)
	 *   RtdAlternativeCarrier alternativeCarrier = new RtdAlternativeCarrier(string carrierDateReference, byte carrierPowerState)
	 *
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USED BY
	 *   RtdHandoverSelector
	 * 
	 * 
	 **/
	public class RtdAlternativeCarrier : Rtd
	{
		
		private byte _carrierPowerState = 0x00;
		private string _carrierDataReference = "";
		private List<string> _auxilirayDataReference = new List<string>();
		
		public RtdAlternativeCarrier() : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "ac")
		{
			
		}
		
		public RtdAlternativeCarrier(byte[] payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "ac")
		{
			int offset = 0;
			int nbchar = 0;
			int auxiliary_data_reference_count = 0;
			
			if ((payload != null) && (payload.Length > 3))
			{
				/* Power State	*/
				_carrierPowerState = payload[offset++];
				
				/* Carrier Data Reference	*/
				nbchar = payload[offset++];
				if (nbchar != 0)
				{
					byte[] carrierDataReference_bytes = new byte[nbchar];
					for (int i = 0; i<nbchar ; i++)
						if (offset < payload.Length)
							carrierDataReference_bytes[i] = payload[offset++];
					
					_carrierDataReference = System.Text.Encoding.ASCII.GetString(carrierDataReference_bytes);
				}
				
				/* Auxiliary Data Reference Count	*/
				if (offset < payload.Length)
					auxiliary_data_reference_count = payload[offset++];
				
				/* Auxiliary Data Reference				*/
				for (int i = 0; i<auxiliary_data_reference_count; i++)
				{
					if (offset < payload.Length)
						nbchar = payload[offset++];
					
					if (nbchar != 0)
					{
						byte[] auxiliaryDataReference_bytes = new byte[nbchar];
						for (int j = 0; j<nbchar; j++)
							if (offset < payload.Length)
								auxiliaryDataReference_bytes[j] = payload[offset++];
						
						string auxiliaryDataReference = System.Text.Encoding.ASCII.GetString(auxiliaryDataReference_bytes);
						_auxilirayDataReference.Add(auxiliaryDataReference);
						
					}
					
				}
				
			}

		}
		
		public RtdAlternativeCarrier(string carrierDateReference, byte carrierPowerState) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "ac")
		{
			_carrierPowerState = carrierPowerState;
			_carrierDataReference = carrierDateReference;
		}
		
		
		/**v* SpringCard.NfcForum.Ndef/RtdAlternativeCarrier.CarrierPowerState
		 *
		 * SYNOPSIS
		 *   public byte CarrierPowerState
		 * 
		 * DESCRIPTION
		 *   Gets the Carrier Power State of the Alternative Carrier
		 *
		 *
		 **/
		public byte CarrierPowerState
		{
			get
			{
				return _carrierPowerState;
			}
		}
		
		
		/**v* SpringCard.NfcForum.Ndef/RtdAlternativeCarrier.CarrierDataReference
		 *
		 * SYNOPSIS
		 *   public byte CarrierPowerState
		 * 
		 * DESCRIPTION
		 *   Gets the Carrier Data Reference of the Alternative Carrier
		 *
		 *
		 **/
		public string CarrierDataReference
		{
			get
			{
				return _carrierDataReference;
			}
		}

		/**m* SpringCard.NfcForum.Ndef/RtdAlternativeCarrier.Encode
		 *
		 * SYNOPSIS
		 *   public override bool Encode(ref byte[] buffer)
		 * 
		 * DESCRIPTION
		 *   Serializes first the Carrier Power State, the Carrier Data Reference
		 *   and the auxiliary data references, that will become the payload of the
		 * 	 RtdAlternativeCarrier. Then, it calls base.Encode to serialize the whole
		 *   content (header).
		 *	 Returns true if it succeeds.
		 * 
		 *
		 **/
		public override bool Serialize(ref byte[] buffer)
		{
			int NbBytesInAuxiliaryDataReferences = 0;
			for (int i = 0; i< _auxilirayDataReference.Count; i++)
				NbBytesInAuxiliaryDataReferences += 1 + _auxilirayDataReference[i].Length;
			
			/* length = CPS + data_reference_len + data_reference + auxiliray_data_reference_count + RFU	*/
			int payload_len = 1 + 1 + _carrierDataReference.Length + 1 + NbBytesInAuxiliaryDataReferences + 1;
			
			byte[] payload = new byte[payload_len];
			int offset = 0;
			
			payload[offset++] = _carrierPowerState;
			payload[offset++] = (byte )_carrierDataReference.Length;
			byte[] _carrierDataReference_bytes = System.Text.Encoding.ASCII.GetBytes(_carrierDataReference);
			for (int i = 0; i<_carrierDataReference_bytes.Length; i++)
				payload[offset++] = _carrierDataReference_bytes[i];
			
			payload[offset++] = (byte) _auxilirayDataReference.Count;
			for (int i = 0; i<_auxilirayDataReference.Count ; i++)
			{
				payload[offset++] = (byte) _auxilirayDataReference[i].Length;
				byte[] _auxiliaryDataReference_bytes = System.Text.Encoding.ASCII.GetBytes(_auxilirayDataReference[i]);
				for (int j = 0; j<_auxiliaryDataReference_bytes.Length; j++)
					payload[offset++] = _auxiliaryDataReference_bytes[j];
			}
			payload[offset++] = 0x00; /*RFU	*/
			
			PAYLOAD = payload;
			return base.Serialize(ref buffer);
		}
		
	}
	
}
