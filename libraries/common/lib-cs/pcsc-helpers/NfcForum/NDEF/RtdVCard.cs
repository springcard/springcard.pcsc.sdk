/**h* SpringCard.NfcForum.Ndef/RtdVCard
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD VCard class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/RtdVCard
	 *
	 * NAME
	 *   RtdVCard
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a VCard
	 *
	 * SYNOPSIS
	 *   RtdVCard vCard = new RtdVCard()
	 *   RtdVCard vCard = new RtdVCard(byte[] Payload)
	 * 
	 * DERIVED FROM
	 *   RtdMedia
	 * 
	 **/
	public class RtdVCard : RtdMedia
	{
		public RtdVCard() : base("text/x-vCard")
		{
			
		}
		
		public RtdVCard(byte[] payload) : base("text/x-vCard")
		{
			Logger.Trace("Parsing VCard Payload");
			
			/* First: loop to determine how many lines are present in the VCard	*/
			int i = 0;
			int Nb_lines = 0;
			while(i < payload.Length)
			{
				if (i+1 < payload.Length)
				{
					if (payload[i] == 0x0D && payload[i+1] == 0x0A)
					{
						Nb_lines++;
						i+=2;
					} else
					{
						i++;
					}
				} else
				{
					i++;
				}
			}
			
			/* Second: create an array of strings : 									*/
			/* Each entry corresponds to a line present in the VCard	*/
			string[] lines_array = new string[Nb_lines];
			int index = 0;
			int begining_position = 0;
			byte[] line;
			i = 0;
			while(i < payload.Length)
			{
				if ( (i+1) < payload.Length)
				{
					if ( (payload[i] == 0x0D) && (payload[i+1] == 0x0A))
					{
						line = new byte[i - begining_position];
						Array.ConstrainedCopy(payload, begining_position, line, 0, i - begining_position);
						lines_array[index] = CardBuffer.StringFromBytes(line);
						index++;
						i+=2;
						begining_position = i;
						
					} else
					{
						i++;
					}
				} else
				{
					i++;
				}
			}
			
			/* Third: for each entry in the array of strings							*/
			/* Determine what it represents and populate the VCard Object	*/
			string[] elements;
			char[] colon = {':'};
			bool email_already_found = false;
			
			for (int j = 0; j < lines_array.Length ; j++)
			{
				elements = lines_array[j].Split(colon);
				
				if (elements[0].IndexOf("FN") != -1)
				{
					string[] names;
					char[] space = {' '};
					names = elements[1].Split(space);
					if (names.Length > 1)
					{
						_first_name = names[0];
						_family_name = names[names.Length - 1];
					}
					
				}

				if (elements[0].Equals("N"))
				{
					string[] names;
					char[] semicolon = {';'};
					names = elements[1].Split(semicolon);
					_family_name = names[0];
					
					if (names.Length > 0)
						_first_name = names[1];
					
					if (names.Length > 1)
						_middle_name = names[2];
					
					if (names.Length > 2)
						_prefix_name = names[3];
					
					if (names.Length > 3)
						_suffix_name = names[4];
					
				}
				
				if (elements[0].IndexOf("NICKNAME") != -1)
					_nickname = elements[elements.Length -1];
				
				if (elements[0].IndexOf("BDAY") != -1)
					_birthday = elements[elements.Length -1];

				if (elements[0].IndexOf("ADR") != -1)
				{
					if (elements[0].IndexOf("work") != -1)
					{
						/* Work address																							*/
						string[] addr_lines;
						char[] semicolon = {';'};
						addr_lines = elements[elements.Length - 1].Split(semicolon);
						
						/* There must be exactly 7 elements, even if some are null	*/
						/* The first of them, "PO BOX", is ignored								 	*/
						if (addr_lines.Length == 7)
						{
							if (addr_lines[1].Length > 0)
								_pro_Address1 = addr_lines[1];
							
							if (addr_lines[2].Length > 0)
								_pro_Address2 = addr_lines[2];
							
							if (addr_lines[3].Length > 0)
								_pro_Town = addr_lines[3];
							
							if (addr_lines[4].Length > 0)
								_pro_Region_State = addr_lines[4];
							
							if (addr_lines[5].Length > 0)
								_pro_Post_Code = addr_lines[5];
							
							if (addr_lines[6].Length > 0)
								_pro_Country = addr_lines[6];
							
						}

					} else
					{
						/* Work is not specified	-> "private" tab */
						string[] addr_lines;
						char[] semicolon = {';'};
						addr_lines = elements[elements.Length - 1].Split(semicolon);
						
						/* There must be exactly 7 elements, even if some are null */
						/* The first of them : PO BOX, is ignored									 */
						if (addr_lines.Length == 7)
						{
							if (addr_lines[1].Length > 0)
								_address1 = addr_lines[1];
							
							if (addr_lines[2].Length > 0)
								_address2 = addr_lines[2];
							
							if (addr_lines[3].Length > 0)
								_town = addr_lines[3];
							
							if (addr_lines[4].Length > 0)
								_region_State = addr_lines[4];
							
							if (addr_lines[5].Length > 0)
								_post_Code = addr_lines[5];
							
							if (addr_lines[6].Length > 0)
								_country = addr_lines[6];
							
						}
						
					}

				}
				
				if (elements[0].IndexOf("TEL") != -1)
				{
					
					if (elements[0].IndexOf("TYPE=") != -1)
					{
						
						if (elements[0].IndexOf("work") != -1)
							_business_phone = elements[elements.Length - 1];
						
						if (elements[0].IndexOf("cell") != -1)
							_cell_phone = elements[elements.Length - 1];
						
						if (elements[0].IndexOf("home") != -1)
							_home_phone = elements[elements.Length - 1];
						
						if (elements[0].IndexOf("pager") != -1)
							_pager = elements[elements.Length - 1];
						
						if (elements[0].IndexOf("fax") != -1)
							_fax = elements[elements.Length - 1];
						
					} else
					{
						/* No TYPE specified => By default : Business phone	*/
						_business_phone = elements[elements.Length - 1];
					}
					
				}
				
				if (elements[0].IndexOf("EMAIL") != -1)
				{
					if (!email_already_found)
					{
						_email = elements[elements.Length - 1];
						email_already_found = true;
					} else
					{
						_email_alternative = elements[elements.Length - 1];
					}

				}
				
				if (elements[0].IndexOf("TITLE") != -1)
					_title = elements[elements.Length - 1];

				if (elements[0].IndexOf("ROLE") != -1)
					_role = elements[elements.Length - 1];
				
				if (elements[0].IndexOf("ORG") != -1)
					_company = elements[elements.Length - 1];
				
				if (elements[0].IndexOf("PHOTO") != -1)
				{
					/*
					string[] spec;
					char[] comma = {','};
					spec = elements[elements.Length - 1].Split(comma);
					_photo = spec[spec.Length - 1];
					 */
					_photo = elements[elements.Length - 1];
				}
				
			}
			Logger.Trace("VCard Payload successfully parsed");

		}
		
		#region detail_VCard_fields
		private string _nickname ="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Nickname
		 *
		 * SYNOPSIS
		 *   public string Nickname
		 * 
		 * DESCRIPTION
		 *   Gets and sets the nickname
		 *
		 **/
		public string Nickname
		{
			get { return _nickname;}
			set { _nickname = Nickname;}
		}
		
		private string _email_alternative ="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Email_alternative
		 *
		 * SYNOPSIS
		 *   public string Email_alternative
		 * 
		 * DESCRIPTION
		 *   Gets and sets the alternative e-mail
		 *
		 **/
		public string Email_alternative
		{
			get {return _email_alternative;}
			set {_email_alternative = Email_alternative;}
		}
		
		private string _pager="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pager
		 *
		 * SYNOPSIS
		 *   public string Pager
		 * 
		 * DESCRIPTION
		 *   Gets and sets the pager
		 *
		 **/
		public string Pager
		{
			get {return _pager;}
			set {_pager =Pager;}
		}
		
		private string _fax="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Fax
		 *
		 * SYNOPSIS
		 *   public string Fax
		 * 
		 * DESCRIPTION
		 *   Gets and sets the fax number
		 *
		 **/
		public string Fax
		{
			get {return _fax;}
			set {_fax = Fax;}
		}
		
		private string _home_phone="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Home_phone
		 *
		 * SYNOPSIS
		 *   public string Home_phone
		 * 
		 * DESCRIPTION
		 *   Gets and sets the home phone number
		 *
		 **/
		public string Home_phone
		{
			get {return _home_phone;}
			set {_home_phone = Home_phone;}
		}
		
		private string _cell_phone="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Cell_phone
		 *
		 * SYNOPSIS
		 *   public string Cell_phone
		 * 
		 * DESCRIPTION
		 *   Gets and sets the cell phone number
		 *
		 **/
		public string Cell_phone
		{
			get {return _cell_phone;}
			set {_cell_phone = Cell_phone;}
		}
		
		private string _first_name="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.First_name
		 *
		 * SYNOPSIS
		 *   public string First_name
		 * 
		 * DESCRIPTION
		 *   Gets and sets the first name
		 *
		 **/
		public string First_name
		{
			get {return _first_name;}
			set {_first_name = First_name;}
		}
		
		private string _family_name="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Family_name
		 *
		 * SYNOPSIS
		 *   public string Family_name
		 * 
		 * DESCRIPTION
		 *   Gets and sets the family name
		 *
		 **/
		public string Family_name
		{
			get {return _family_name;}
			set {_family_name = Family_name;}
		}
		
		private string _business_phone="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Business_phone
		 *
		 * SYNOPSIS
		 *   public string Business_phone
		 * 
		 * DESCRIPTION
		 *   Gets and sets the business phone number
		 *
		 **/
		public string Business_phone
		{
			get {return _business_phone;}
			set {_business_phone = Business_phone;}
		}
		
		private string _email="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Email
		 *
		 * SYNOPSIS
		 *   public string Email
		 * 
		 * DESCRIPTION
		 *   Gets and sets the e-mail
		 *
		 **/
		public string Email
		{
			get {return _email;}
			set {_email = Email;}
		}
		
		private string _birthday="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Birthday
		 *
		 * SYNOPSIS
		 *   public string Birthday
		 * 
		 * DESCRIPTION
		 *   Gets and sets the birthday
		 *
		 **/
		public string Birthday
		{
			get {return _birthday;}
			set {_birthday = Birthday;}
		}
		
		private string _country="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Country
		 *
		 * SYNOPSIS
		 *   public string Country
		 * 
		 * DESCRIPTION
		 *   Gets and sets the country
		 *
		 **/
		public string Country
		{
			get {return _country;}
			set {_country = Country;}
		}
		
		private string _town="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Town
		 *
		 * SYNOPSIS
		 *   public string Town
		 * 
		 * DESCRIPTION
		 *   Gets and sets the town
		 *
		 **/
		public string Town
		{
			get {return _town;}
			set {_town = Town;}
		}
		
		private string _address2="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Address2
		 *
		 * SYNOPSIS
		 *   public string Address2
		 * 
		 * DESCRIPTION
		 *   Gets and sets the second part of the address
		 *
		 **/
		public string Address2
		{
			get {return _address2;}
			set {_address2 = Address2;}
		}
		
		private string _address1="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Address1
		 *
		 * SYNOPSIS
		 *   public string Address1
		 * 
		 * DESCRIPTION
		 *   Gets and sets the first part of the adrress
		 *
		 **/
		public string Address1
		{
			get {return _address1;}
			set { _address1 = Address1;}
		}
		private string _post_Code="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Post_Code
		 *
		 * SYNOPSIS
		 *   public string Post_Code
		 * 
		 * DESCRIPTION
		 *   Gets and sets the postal code
		 *
		 **/
		public string Post_Code
		{
			get {return _post_Code;}
			set {_post_Code =Post_Code;}
		}
		
		private string _region_State="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Region_State
		 *
		 * SYNOPSIS
		 *   public string Region_State
		 * 
		 * DESCRIPTION
		 *   Gets and sets the region (state or province)
		 *
		 **/
		public string Region_State
		{
			get {return _region_State;}
			set {_region_State = Region_State;}
		}
		
		private string _role="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Role
		 *
		 * SYNOPSIS
		 *   public string Role
		 * 
		 * DESCRIPTION
		 *   Gets and sets the role (function)
		 *
		 **/
		public string Role
		{
			get {return _role;}
			set {_role = Role;}
		}
		
		private string _title="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Title
		 *
		 * SYNOPSIS
		 *   public string Title
		 * 
		 * DESCRIPTION
		 *   Gets and sets the title
		 *
		 **/
		public string Title
		{
			get {return _title;}
			set {_title = Title;}
		}
		
		private string _company="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Company
		 *
		 * SYNOPSIS
		 *   public string Company
		 * 
		 * DESCRIPTION
		 *   Gets and sets the company
		 *
		 **/
		public string Company
		{
			get {return _company;}
			set {_company = Company;}
		}
		
		private string _pro_Address1="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Address1
		 *
		 * SYNOPSIS
		 *   public string Pro_Address1
		 * 
		 * DESCRIPTION
		 *   Gets and sets the first part of the professional address
		 *
		 **/
		public string Pro_Address1
		{
			get {return _pro_Address1;}
			set {_pro_Address1 = Pro_Address1;}
		}
		
		private string _pro_Address2="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Address2
		 *
		 * SYNOPSIS
		 *   public string Pro_Address2
		 * 
		 * DESCRIPTION
		 *   Gets and sets the second part of the professional address
		 *
		 **/
		public string Pro_Address2
		{
			get {return _pro_Address2;}
			set {_pro_Address2 = Pro_Address2;}
		}
		
		private string _pro_Post_Code="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Post_Code
		 *
		 * SYNOPSIS
		 *   public string Pro_Post_Code
		 * 
		 * DESCRIPTION
		 *   Gets and sets the professional post code
		 *
		 **/
		public string Pro_Post_Code
		{
			get {return _pro_Post_Code;}
			set {_pro_Post_Code = Pro_Post_Code;}
		}
		
		private string _pro_Region_State="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Region_State
		 *
		 * SYNOPSIS
		 *   public string Pro_Region_State
		 * 
		 * DESCRIPTION
		 *   Gets and sets the professional region (state or province)
		 *
		 **/
		public string Pro_Region_State
		{
			get {return _pro_Region_State;}
			set {_pro_Region_State = Pro_Region_State;}
		}
		
		private string _pro_Town="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Town
		 *
		 * SYNOPSIS
		 *   public string Pro_Town
		 * 
		 * DESCRIPTION
		 *   Gets and sets the professional town
		 *
		 **/
		public string Pro_Town
		{
			get {return _pro_Town;}
			set { _pro_Town = Pro_Town;}
		}
		
		private string _pro_Country="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Pro_Country
		 *
		 * SYNOPSIS
		 *   public string Pro_Country
		 * 
		 * DESCRIPTION
		 *   Gets and sets the professional country
		 *
		 **/
		public string Pro_Country
		{
			get {return _pro_Country;}
			set {_pro_Country = Pro_Country;}
		}
		
		private string _middle_name="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Middle_name
		 *
		 * SYNOPSIS
		 *   public string Middle_name
		 * 
		 * DESCRIPTION
		 *   Gets and sets the middle name
		 *
		 **/
		public string Middle_name
		{
			get {return _middle_name;}
			set {_middle_name = Middle_name;}
		}
		
		private string _prefix_name="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Prefix_name
		 *
		 * SYNOPSIS
		 *   public string Prefix_name
		 * 
		 * DESCRIPTION
		 *   Gets and sets the name prefix
		 *
		 **/
		public string Prefix_name
		{
			get {return _prefix_name;}
			set {_prefix_name = Prefix_name;}
		}
		
		private string _suffix_name="";
		
		/**v* SpringCard.NfcForum.Ndef/RtdVCard.Suffix_name
		 *
		 * SYNOPSIS
		 *   public string Prefix_name
		 * 
		 * DESCRIPTION
		 *   Gets and sets the name suffix
		 *
		 **/
		public string Suffix_name
		{
			get {return _suffix_name;}
			set {_suffix_name = Suffix_name;}
		}
		
		private string _photo= "";
		
		/**m* SpringCard.NfcForum.Ndef/RtdVCard.set_photo
		 *
		 * SYNOPSIS
		 *   public void set_photo(string s)
		 * 
		 * DESCRIPTION
		 *   Sets the picture (image or photograph information) in a base 64 string
		 *
		 **/
		public void set_photo(string s)
		{
			_photo = s;
		}
		
		/**m* SpringCard.NfcForum.Ndef/RtdVCard.get_photo
		 *
		 * SYNOPSIS
		 *   public string get_photo()
		 * 
		 * DESCRIPTION
		 *   Gets the picture (image or photograph information) as a base 64 string
		 *
		 **/
		public string get_photo()
		{
			return _photo;
		}
		
		#endregion
		
		/**m* SpringCard.NfcForum.Ndef/RtdVCard.set_input_texts
		 *
		 * SYNOPSIS
		 *   public void set_input_texts(string name, string value)
		 * 
		 * DESCRIPTION
		 *   Sets the value "value" to the attribute "name"
		 *   Example : set_input_texts("First_name", "John") sets the First Name of the VCard to John.
		 * 	 Possible values for parameter "name" are :
		 * 			Nickname / Email_alternative / Pager / Fax / Home_phone / Cell_phone / First_name
		 * 			Family_name / Business_phone / Email / Birthday / Country / Town / Address2 / Address1
		 *   		Post_Code / Region_State / Role / Title / Company / Pro_Address1 / Pro_Address2
		 * 			Pro_Post_Code / Pro_Region_State / Pro_Town / Pro_Country
		 * 
		 **/
		public void set_input_texts(string name, string value)
		{
			if (!value.Equals(""))
			{
				if (name.Equals("Nickname"))
					_nickname = value;
				
				if (name.Equals("Email_alternative"))
					_email_alternative = value;
				
				if (name.Equals("Pager"))
					_pager = value;
				
				if (name.Equals("Fax"))
					_fax = value;
				
				if (name.Equals("Home_phone"))
					_home_phone = value;
				
				if (name.Equals("Cell_phone"))
					_cell_phone = value;
				
				if (name.Equals("First_name"))
					_first_name = value;
				
				if (name.Equals("Family_name"))
					_family_name = value;
				
				if (name.Equals("Business_phone"))
					_business_phone = value;
				
				if (name.Equals("Email"))
					_email = value;
				
				if (name.Equals("Birthday"))
					_birthday = value;
				
				if (name.Equals("Country"))
					_country = value;
				
				if (name.Equals("Town"))
					_town = value;
				
				if (name.Equals("Address2"))
					_address2 = value;
				
				if (name.Equals("Address1"))
					_address1 = value;
				
				if (name.Equals("Post_Code"))
					_post_Code = value;
				
				if (name.Equals("Region_State"))
					_region_State = value;
				
				if (name.Equals("Role"))
					_role = value;
				
				if (name.Equals("Title"))
					_title = value;
				
				if (name.Equals("Company"))
					_company = value;
				
				if (name.Equals("Pro_Address1"))
					_pro_Address1 = value;
				
				if (name.Equals("Pro_Address2"))
					_pro_Address2 = value;

				if (name.Equals("Pro_Post_Code"))
					_pro_Post_Code = value;
				
				if (name.Equals("Pro_Region_State"))
					_pro_Region_State = value;

				if (name.Equals("Pro_Town"))
					_pro_Town = value;

				if (name.Equals("Pro_Country"))
					_pro_Country = value;
				
			}
			
		}
		
		private int count_nb_bytes()
		{
			/* All the ASCII strings in the VCard must be separated by 0x0D0A, which represent 2 characters */
			int NbBytes = 0 ;
			NbBytes = "BEGIN:VCARD".Length + 2 + "VERSION:3.0".Length + 2;
			NbBytes += "FN:".Length + First_name.Length +  1 + Family_name.Length + 2;		/* 1 is for "space"	*/
			
			if (!Nickname.Equals(""))
				NbBytes += "NICKNAME:".Length + Nickname.Length + 2;
			
			/* Birthday with 8 characters: yyyymmdd	*/
			DateTime dateBirthday;
			if ((!Birthday.Equals("")) && (	DateTime.TryParse(Birthday, out dateBirthday)) )
				NbBytes += "BDAY:".Length + 8 + 2;
			
			/* Home address in 6 fields, separated by a ';'						*/
			/* The PO Box is discarded, which explains the first ';'	*/
			if ( 	 !(Address1.Equals(""))
			    || !(Address2.Equals(""))
			    || !(Town.Equals(""))
			    || !(Region_State.Equals(""))
			    || !(Post_Code.Equals(""))
			    || !(Country.Equals(""))
			   )
			{
				NbBytes += "ADR:;".Length
					+ Address1.Length + 1
					+ Address2.Length + 1
					+ Town.Length + 1
					+ Region_State.Length + 1
					+ Post_Code.Length + 1
					+ Country.Length + 2;
			}
			
			/* Work address in 6 fields, separated by a ';'						*/
			/* The PO Box is discarded, which explains the first ';'	*/
			if ( 	 !(Pro_Address1.Equals(""))
			    || !(Pro_Address2.Equals(""))
			    || !(Pro_Town.Equals(""))
			    || !(Pro_Region_State.Equals(""))
			    || !(Pro_Post_Code.Equals(""))
			    || !(Pro_Country.Equals(""))
			   )
			{
				NbBytes += "ADR;TYPE=work:;".Length
					+ Pro_Address1.Length + 1
					+ Pro_Address2.Length + 1
					+ Pro_Town.Length + 1
					+ Pro_Region_State.Length + 1
					+ Pro_Post_Code.Length + 1
					+ Pro_Country.Length +2;
			}
			
			if (!Home_phone.Equals(""))
				NbBytes += "TEL;TYPE=home:".Length + Home_phone.Length + 2;
			
			if (!Business_phone.Equals(""))
				NbBytes += "TEL;TYPE=work:".Length + Business_phone.Length + 2;
			
			if (!Cell_phone.Equals(""))
				NbBytes += "TEL;TYPE=cell:".Length + Cell_phone.Length + 2;
			
			if (!Fax.Equals(""))
				NbBytes += "TEL;TYPE=fax:".Length + Fax.Length +2;
			
			if (!Pager.Equals(""))
				NbBytes += "TEL;TYPE=pager:".Length + Pager.Length +2;
			
			if (!Email.Equals(""))
			{
				if (Email_alternative.Equals(""))
				{
					NbBytes += "EMAIL:".Length + Email.Length +2;
				} else
				{
					/* TWO EMAILS : PREF=1 and PREF=2	*/
					NbBytes += "EMAIL;PREF=1:".Length + Email.Length + 2 + "EMAIL;PREF=2:".Length	+ Email_alternative.Length + 2 ;
				}
			}
			
			if (!Title.Equals(""))
				NbBytes += "TITLE:".Length + Title.Length + 2;

			if (!Role.Equals(""))
				NbBytes += "ROLE:".Length + Role.Length + 2;
			
			if (!Company.Equals(""))
				NbBytes += "ORG:".Length + Company.Length + 2;
			
			if (!_photo.Equals(""))
				NbBytes +="PHOTO;ENCODING=BASE64;TYPE=JPEG:".Length + _photo.Length + 2;

			NbBytes += "END:VCARD".Length +2;
			
			return NbBytes;
		}
		
		private int AddLine(byte[] pl, int len, int index, string desc, string cont)
		{
			byte[] desc_array = CardBuffer.BytesFromString(desc);
			
			if ((index + desc_array.Length) > len)
				return -1;
			
			Array.ConstrainedCopy(desc_array, 0, pl, index, desc_array.Length);
			index += desc_array.Length;
			
			if ((index + cont.Length) > len)
				return -1;
			
			Array.ConstrainedCopy(CardBuffer.BytesFromString(cont), 0, pl, index, cont.Length);
			index += cont.Length;
			
			if ( index > len)
				return -1;
			
			pl[index] = 0x0D;
			index ++;
			if (index > len)
				return -1;
			
			pl[index] = 0x0A;
			index ++;
			if (index >len)
				return -1;
			
			return index;
			
		}

		private string convert_into_vcard_date(string s)
		{
			DateTime dateBirthday;
			if (!DateTime.TryParse(Birthday, out dateBirthday))
				return "";
			
			string month	=	dateBirthday.Month.ToString();
			string day 		= dateBirthday.Day.ToString();
			string year 	= dateBirthday.Year.ToString();
			string date 	=	year + month + day;
			return date;
			
		}
		
		public override bool Serialize(ref byte[] buffer)
		{
			/* First : determine the total number of characters	*/
			int NbBytes = count_nb_bytes();
			
			/* Second : create the whole byte array 						*/
			int index = 0;
			int new_index;
			byte[] pl = new byte[NbBytes];
			
			new_index = AddLine(pl, pl.Length, index, "BEGIN:VCARD", "");
			if (new_index < 0)
			{
				Logger.Trace("Error generating 'VCard' object: after 'BEGIN'");
				return false;
			}
			index = new_index;
			
			new_index = AddLine(pl, pl.Length, index, "VERSION:3.0", "");
			if (new_index < 0)
			{
				Logger.Trace("Error generating 'VCard' object: after 'VERSION'");
				return false;
			}
			index = new_index;
			
			string name = _first_name+ " " + _family_name;
			new_index = AddLine(pl, pl.Length, index, "FN:", name);
			if (new_index < 0)
			{
				Logger.Trace("Error generating 'VCard' object: after 'FN'");
				return false;
			}
			index = new_index;
			
			
			if (!Nickname.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "NICKNAME:", Nickname);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'Nickname'");
					return false;
				}
				index = new_index;
				
			}
			
			DateTime dateBirthday;
			if ((!Birthday.Equals("")) && (	DateTime.TryParse(Birthday, out dateBirthday)) )
			{
				string Bday = convert_into_vcard_date(Birthday);
				new_index = AddLine(pl, pl.Length, index, "BDAY:", Bday);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'Birthday'");
					return false;
				}
				index = new_index;
				
			}
			
			if ( 	 !(Address1.Equals(""))
			    || !(Address2.Equals(""))
			    || !(Town.Equals(""))
			    || !(Region_State.Equals(""))
			    || !(Post_Code.Equals(""))
			    || !(Country.Equals(""))
			   )
			{
				string addr_line = Address1 + ";" + Address2 + ";" + Town + ";"
					+ Region_State + ";" + Post_Code + ";" + Country;
				new_index = AddLine(pl, pl.Length, index, "ADR:;", addr_line);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'Address'");
					return false;
				}
				index = new_index;
				
			}
			
			if ( 	 !(Pro_Address1.Equals(""))
			    || !(Pro_Address2.Equals(""))
			    || !(Pro_Town.Equals(""))
			    || !(Pro_Region_State.Equals(""))
			    || !(Pro_Post_Code.Equals(""))
			    || !(Pro_Country.Equals(""))
			   )
			{
				string pro_addr_line = Pro_Address1 + ";" + Pro_Address2 + ";" + Pro_Town + ";"
					+ Pro_Region_State + ";" + Pro_Post_Code + ";" + Pro_Country;
				new_index = AddLine(pl, pl.Length, index, "ADR;TYPE=work:;", pro_addr_line);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'Professional Address'");
					return false;
				}
				index = new_index;
				
			}

			if (!Home_phone.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TEL;TYPE=home:", Home_phone);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TEL;TYPE=home'");
					return false;
				}
				index = new_index;
				
			}

			
			if (!Business_phone.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TEL;TYPE=work:", Business_phone);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TEL;TYPE=work'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!Cell_phone.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TEL;TYPE=cell:", Cell_phone);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TEL;TYPE=cell'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!Pager.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TEL;TYPE=pager:", Pager);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TEL;TYPE=pager'");
					return false;
				}
				index = new_index;
				
			}

			if (!Fax.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TEL;TYPE=fax:", Fax);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TEL;TYPE=fax'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!Email.Equals(""))
			{
				if (Email_alternative.Equals(""))
				{
					new_index = AddLine(pl, pl.Length, index, "EMAIL:", Email);
					if (new_index < 0)
					{
						Logger.Trace("Error generating 'VCard' object: after 'EMAIL'");
						return false;
					}
					index = new_index;
					
				} else
				{
					/*	Two E-mails to add */
					new_index = AddLine(pl, pl.Length, index, "EMAIL;PREF=1:", Email);
					if (new_index < 0)
					{
						Logger.Trace("Error generating 'VCard' object: after 'EMAIL;PREF=1'");
						return false;
					}
					index = new_index;
					
					new_index = AddLine(pl, pl.Length, index, "EMAIL;PREF=2:", Email_alternative);
					if (new_index < 0)
					{
						Logger.Trace("Error generating 'VCard' object: after 'EMAIL;PREF=2'");
						return false;
					}
					index = new_index;
				}
			}

			if (!Title.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "TITLE:", Title);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'TITLE'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!Role.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "ROLE:", Role);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'ROLE'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!Company.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index, "ORG:", Company);
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'ORG'");
					return false;
				}
				index = new_index;
				
			}
			
			if (!_photo.Equals(""))
			{
				new_index = AddLine(pl, pl.Length, index,  "PHOTO;ENCODING=BASE64;TYPE=JPEG:", _photo);
				
				if (new_index < 0)
				{
					Logger.Trace("Error generating 'VCard' object: after 'PHOTO'");
					return false;
				}
				
				index = new_index;
			}

			new_index = AddLine(pl, pl.Length, index, "END:VCARD", "");
			payload = pl;
			
			return base.Serialize(ref buffer);

		}

	}
	
}
