/**h* SpringCard.NfcForum.Ndef/RtdHandoverSelector
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Handover Selector class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2013
 *   See LICENSE.TXT for information
 *
 **/

using System;
using System.Collections.Generic;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/RtdHandoverSelector
	 *
	 * NAME
	 *   RtdHandoverSelector
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores an Handover Selector
	 *
	 * SYNOPSIS
	 *   RtdHandoverSelector handoverSelector = new RtdHandoverSelector()
	 *   RtdHandoverSelector handoverSelector = new RtdHandoverSelector(byte[] payload, ref byte[] buffer, ref int next_ndef_starting_point)
	 *   RtdHandoverSelector handoverSelector = new RtdHandoverSelector(RtdAlternativeCarrier[] alternative_carrier, AbsoluteUri[] absolute_uri)
	 *   RtdHandoverSelector handoverSelector = new RtdHandoverSelector(RtdAlternativeCarrier alternative_carrier, AbsoluteUri absolute_uri)
	 *
	 * DERIVED FROM
	 *   Rtd
	 *
	 **/
	public class RtdHandoverSelector : Rtd
	{
		private byte version = 0x00;
		private RtdAlternativeCarrier[] _alternative_carriers;
		private AbsoluteUri[] _related_absolute_uris;
		
		public RtdHandoverSelector() : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "Hs")
		{
			
		}
		
		public RtdHandoverSelector(byte[] payload, ref byte[] buffer, ref int next_ndef_starting_point) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "Hs")
		{
			
			/* Take care of Payload	*/
			int offset = 1;
			NdefObject ndef = null;
			bool terminated = true;
			
			if ((payload != null) && (payload.Length >0 ))
				version = payload[0];
			
			if (version < 0x10)
			{
				Logger.Trace("Incompatible version: " + String.Format("{0:x02}", version));
				offset = payload.Length - 1; /* so that it won't be parsed	*/
			}
			
			List<RtdAlternativeCarrier> altenative_carriers_list = new List<RtdAlternativeCarrier>();
			
			while (NdefObject.Parse(payload, ref offset, ref ndef, ref terminated))
			{
				if (ndef is RtdAlternativeCarrier)
				{
					Logger.Trace("Got a new Alternative Carrier");
					altenative_carriers_list.Add((RtdAlternativeCarrier) ndef);
				}
				
				if (terminated)
					break;
			}
			_alternative_carriers = altenative_carriers_list.ToArray();
			
			
			/* Take care of following NDEFs	*/
			terminated = true;
			List<AbsoluteUri> related_absolute_uris_list = new List<AbsoluteUri>();
			while (NdefObject.Parse(buffer, ref next_ndef_starting_point, ref ndef, ref terminated))
			{
				if (ndef is AbsoluteUri)
				{
					Logger.Trace("Got a new Absolute Uri");
					related_absolute_uris_list.Add((AbsoluteUri) ndef);
				}
				
				if (terminated)
					break;
			}
			_related_absolute_uris = related_absolute_uris_list.ToArray();
			
		}

		
		public RtdHandoverSelector(RtdAlternativeCarrier[] alternative_carrier, AbsoluteUri[] absolute_uri) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "Hs")
		{
			version = 0x12;
			
			_alternative_carriers = new RtdAlternativeCarrier[alternative_carrier.Length];
			_alternative_carriers = alternative_carrier;
			
			_related_absolute_uris = new AbsoluteUri[absolute_uri.Length];
			_related_absolute_uris = absolute_uri;
		}
		
		public RtdHandoverSelector(RtdAlternativeCarrier alternative_carrier, AbsoluteUri absolute_uri) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "Hs")
		{
			version = 0x12;
			
			_alternative_carriers = new RtdAlternativeCarrier[1];
			_alternative_carriers[0] = alternative_carrier;
			
			_related_absolute_uris = new AbsoluteUri[1];
			_related_absolute_uris[0] = absolute_uri;
		}
		
		/**v* SpringCard.NfcForum.Ndef/RtdHandoverSelector.AlternativeCarriers
		 *
		 * SYNOPSIS
		 *   public RtdAlternativeCarrier[] AlternativeCarriers
		 * 
		 * DESCRIPTION
		 *  Gets the Alternative Carriers of this Handover Selector record
		 *
		 *
		 **/
		public RtdAlternativeCarrier[] AlternativeCarriers
		{
			get
			{
				return _alternative_carriers;
			}
		}
		
		/**v* SpringCard.NfcForum.Ndef/RtdHandoverSelector.RelatedAbsoluteUris
		 *
		 * SYNOPSIS
		 *   public AbsoluteUri[] RelatedAbsoluteUris
		 * 
		 * DESCRIPTION
		 *  Gets the related absolute URIs of this Handover Selector record.
		 * 	These absolute URIs are those which are referenced by all the Carrier
		 * 	Data References and Auxiliary Data References of all the Alternative Carrier Records
		 * 	of his Handover Selector.
		 *
		 *
		 **/
		public AbsoluteUri[] RelatedAbsoluteUris
		{
			get
			{
				return _related_absolute_uris;
			}
		}
		
		/**m* SpringCard.NfcForum.Ndef/RtdHandoverSelector.Encode
		 *
		 * SYNOPSIS
		 *   public virtual bool Encode(ref byte[] buffer)
		 * 
		 * DESCRIPTION
		 *   Serializes the Handover Selector Record and also concatenates
		 *   the serialized Absolute Uris, referenced by the Alternative
		 * 	 Carrier Records
		 * 	 Returns true if it succeeds, false otherwise
		 * 
		 **/
		public override bool Serialize(ref byte[] buffer)
		{
			/* First : encode the alternative carriers	*/
			byte[] buffer_HsNDEF = null;
			for (int i=0; i<_alternative_carriers.Length ; i++)
			{
				if (i==0)
					_alternative_carriers[i].MB = true;
				
				if (i==_alternative_carriers.Length - 1)
					_alternative_carriers[i].ME = true;
				
				_alternative_carriers[i].Serialize(ref buffer_HsNDEF);
			}
			
			/* Second : add the version in front of it : this becomes the payload of the Hs NDEF	*/
			byte[] payload = new byte[buffer_HsNDEF.Length + 1];
			payload[0] = version;
			Array.ConstrainedCopy(buffer_HsNDEF, 0, payload, 1, buffer_HsNDEF.Length);
			PAYLOAD = payload;
			
			/* Third : encode the Hs NDEF	*/
			base.SetMessageEnd(false);
			if (!base.Serialize(ref buffer_HsNDEF))
				return false;
			
			/* Fourth : Encode the _related_absolute_uris	*/
			byte[] buffer_related_ndef = null;
			List<byte> buffer_related_ndefs_list_bytes = new List<byte>();
			for (int i=0; i<_related_absolute_uris.Length ; i++)
			{
				
				byte[] buffer_tmp = null;
				if (i== (_related_absolute_uris.Length-1))
					_related_absolute_uris[i].ME = true;
				
				if (!_related_absolute_uris[i].Serialize(ref buffer_tmp))
					return false;
				
				if (buffer_tmp != null)
					for (int j = 0; j<buffer_tmp.Length; j++)
						buffer_related_ndefs_list_bytes.Add(buffer_tmp[j]);
				
			}
			
			buffer_related_ndef = buffer_related_ndefs_list_bytes.ToArray();
			
			
			/* Fourth : concat the two buffers (Hs NDEF + related absolute URI NDEFs)	*/
			buffer = new byte[buffer_HsNDEF.Length + buffer_related_ndef.Length];
			Array.ConstrainedCopy(buffer_HsNDEF, 0, buffer, 0, buffer_HsNDEF.Length);
			Array.ConstrainedCopy(buffer_related_ndef, 0, buffer, buffer_HsNDEF.Length, buffer_related_ndef.Length);
			
			return true;
		}
		
	}
	
}
