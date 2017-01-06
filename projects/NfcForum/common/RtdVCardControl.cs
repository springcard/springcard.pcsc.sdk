/**h* SpringCardApplication/RtdVCardControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD VCard Control class
 * 
 * COPYRIGHT
 *   Copyright (c) Pro Active SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.NFC;
using System.Reflection;
using System.Resources;

namespace SpringCardApplication
{

	/**c* SpringCardApplication/RtdVCardControl
	 *
	 * NAME
	 *   RtdVCardControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD VCard NDEF
	 *
	 * SYNOPSIS
	 *   RtdVCardControl control = new RtdVCardControl()
	 * 
	 * DERIVED FROM
	 *   RtdControl
	 * 
	 * 
	 **/
	public partial class RtdVCardControl : RtdControl
	{
		
		private const int imageSize = 75;
		private Image image = null;
		private string pic = "";
		
		public RtdVCardControl()
		{
			InitializeComponent();
			Family_name.Text = LoadString("VCard.LastName", "Doe");
			First_name.Text = LoadString("VCard.FirstName", "John");
			Email.Text = LoadString("VCard.eMail", "john.doe@springcard.com");
			Company.Text = LoadString("VCard.Company", "SpringCard");

			initializePicture();
		}

		private void initializePicture()
		{
			/*
			image = null;
			bRemovePicture.Enabled = false;
			ResourceManager resources = new ResourceManager("NfcVCardControlEmptyImage", Assembly.GetExecutingAssembly());
			//ResourceManager resources = new ResourceManager("ZeniusVCard.Resource1", Assembly.GetExecutingAssembly());
			Image unknown = (Image) resources.GetObject("Unknown9090");
			//(Image)resources.GetObject("Unknown9090");
			
			PictureBox pb = new PictureBox();
      
			//Image loadedImage = Image.FromFile(openFileDialog1.FileName);
      pb.Height = unknown.Height;
      pb.Width = unknown.Width;
      
      pb.Image = unknown;
      flpPicture.Controls.Clear();
      flpPicture.Controls.Add(pb);
      pic = "";
      lPSize.Text = ""+ pic.Length + " bytes";
			 */
			
		}
		
		public override void SetEditable(bool yes)
		{
			foreach (Control c in pContact.Controls)
			{
				if (c is TextBox)
					(c as TextBox).ReadOnly = !yes;
			}
			
			foreach (Control c in pBusiness.Controls)
			{
				if (c is TextBox)
					(c as TextBox).ReadOnly = !yes;
			}
			
			foreach (Control c in pPrivate.Controls)
			{
				if (c is TextBox)
					(c as TextBox).ReadOnly = !yes;
			}
			
			bChangePicture.Enabled = yes;
			bRemovePicture.Enabled = yes;
			
		}
		
		public override void ClearContent()
		{
			foreach (Control c in pContact.Controls)
			{
				if (c is TextBox)
					c.Text = "";
			}
			
			foreach (Control c in pBusiness.Controls)
			{
				if (c is TextBox)
					c.Text = "";
			}
			
			foreach (Control c in pPrivate.Controls)
			{
				if (c is TextBox)
					c.Text = "";
			}
			initializePicture();
		}
		
		/**m* SpringCardApplication/RtdVCardControl.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdVCard NFCcard)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an RtdVCard object.
		 *   It prints on the form the content of the RtdVCard object passed as a parameter.
		 *
		 **/
		public void SetContent(RtdVCard NFCcard)
		{
			ClearContent();
			First_name.Text = NFCcard.First_name;
			Family_name.Text = NFCcard.Family_name;
			Nickname.Text = NFCcard.Nickname;
			
			Business_phone.Text = NFCcard.Business_phone;
			Home_phone.Text = NFCcard.Home_phone;
			Pager.Text = NFCcard.Pager;
			Fax.Text = NFCcard.Fax;
			Cell_phone.Text = NFCcard.Cell_phone;
			Email.Text = NFCcard.Email;
			Email_alternative.Text = NFCcard.Email_alternative;
			
			Address1.Text = NFCcard.Address1;
			Address2.Text = NFCcard.Address2;
			Town.Text = NFCcard.Town;
			Region_State.Text = NFCcard.Region_State;
			Post_Code.Text = NFCcard.Post_Code;
			Country.Text = NFCcard.Country;
			
			Pro_Address1.Text = NFCcard.Pro_Address1;
			Pro_Address2.Text = NFCcard.Pro_Address2;
			Pro_Town.Text = NFCcard.Pro_Town;
			Pro_Region_State.Text = NFCcard.Pro_Region_State;
			Pro_Post_Code.Text = NFCcard.Pro_Post_Code;
			Pro_Country.Text = NFCcard.Pro_Country;
			
			Title.Text = NFCcard.Title;
			Role.Text = NFCcard.Role;
			Company.Text = NFCcard.Company;

			if (!NFCcard.get_photo().Equals(""))
			{
				PictureBox pb = new PictureBox();
				Image loadedImage = Base64ToImage(NFCcard.get_photo());
				
				if (loadedImage != null)
				{
					/* Re-dimension before the image to go on the screen : 90*90	*/
					float WidthPercent = (float)loadedImage.Width/90;
					float HeigthPercent = (float)loadedImage.Height/90;
					float newWidth;
					float newHeigth;
					if (WidthPercent < HeigthPercent)
					{
						newWidth 	=	loadedImage.Width/HeigthPercent;
						newHeigth = loadedImage.Height/HeigthPercent;
					}
					else
					{
						newWidth 	= loadedImage.Width/WidthPercent;
						newHeigth = loadedImage.Height/WidthPercent;
					}
					
					Image NewLoadedImage = new Bitmap(loadedImage, new Size((int)newWidth, (int)newHeigth));
					pb.Size = new System.Drawing.Size(90, 90);
					pb.SizeMode = PictureBoxSizeMode.CenterImage;
					pb.Image = NewLoadedImage;
					flpPicture.Controls.Clear();
					flpPicture.Controls.Add(pb);
					
					image = NewLoadedImage;
					bRemovePicture.Enabled = true;
					
					lPSize.Text = NFCcard.get_photo().Length + " bytes";
					
				}

			}
			
			string birth = NFCcard.Birthday;
			int year = 0;
			int month = 0;
			int day = 0;
			
			if (birth.Length == 8)
			{
				try
				{
					/* Format: yyyymmdd	*/
					year 		= Int32.Parse(birth.Substring(0, 4));
					month 	= Int32.Parse(birth.Substring(4, 2));
					day 		= Int32.Parse(birth.Substring(6, 2));
					
					DateTime dateBirthday = new DateTime(year, month, day);
					Birthday.Text = dateBirthday.ToShortDateString();
				}
				catch
				{
					Birthday.Text = NFCcard.Birthday;
				}
			} else
				if ( (birth.Length == 6) && (birth.Substring(0,2).Equals("--") ))
			{
				try
				{
					/* Format: --mmdd		*/
					month 				=  Int32.Parse(birth.Substring(2, 2));
					day	 					=  Int32.Parse(birth.Substring(4, 2));
					DateTime dateBirthday = new DateTime(year, month, day);
					Birthday.Text = dateBirthday.ToShortDateString();
					
				} catch
				{
					Birthday.Text = NFCcard.Birthday;
				}

				
			} else
			{
				Birthday.Text = NFCcard.Birthday;
			}

		}
		
		public override bool ValidateUserContent()
		{
			if ((First_name.Text.Equals("")) || (Family_name.Text.Equals("")))
			{
				MessageBox.Show("Please enter at least a First Name and a Family Name to encode a VCard.");
				return false;
			}
			
			if (!Birthday.Text.Equals(""))
			{
				DateTime dateBirthday;
				if (!DateTime.TryParse(Birthday.Text, out dateBirthday))
				{
					MessageBox.Show("The birthday is not a valid date.");
					return false;
				}
			}

			if ( (Email.Text.Equals("")) && (!Email_alternative.Text.Equals("")) )
			{
				MessageBox.Show("Please enter an e-mail address before entering an alternate e-mail address.");
				return false;
			}

			foreach (Control c in pContact.Controls)
			{
				if (c is TextBox)
				{
					if ((c.Text.IndexOf(":") != -1) || (c.Text.IndexOf(";") != -1))
					{
						MessageBox.Show("Please remove the forbidden character(s) (':' or ';').");
						c.Focus();
						return false;
					}
				}
			}

			foreach (Control c in  pBusiness.Controls)
			{
				if (c is TextBox)
				{
					if ((c.Text.IndexOf(":") != -1) || (c.Text.IndexOf(";") != -1))
					{
						MessageBox.Show("Please remove the forbidden character(s) (':' or ';').");
						c.Focus();
						return false;
					}

				}
			}

			foreach (Control c in pPrivate.Controls)
			{
				if (c is TextBox)
				{
					if ((c.Text.IndexOf(":") != -1) || (c.Text.IndexOf(";") != -1))
					{
						MessageBox.Show("Please remove the forbidden character(s) (':' or ';').");
						c.Focus();
						return false;
					}
				}
			}
			
			return true;
		}
		

		private string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				// Convert Image to byte[]
				image.Save(ms, format);
				byte[] imageBytes = ms.ToArray();
				
				// Convert byte[] to Base64 String
				string base64String = Convert.ToBase64String(imageBytes);
				return base64String;
			}
		}
		
		
		public Image Base64ToImage(string base64String)
		{
			
			try
			{
				/* Convert Base64 String to byte[] */
				byte[] imageBytes = Convert.FromBase64String(base64String);
				MemoryStream ms = new MemoryStream(imageBytes, 0,
				                                   imageBytes.Length);
				
				/* Convert byte[] to Image */
				ms.Write(imageBytes, 0, imageBytes.Length);
				Image pic = Image.FromStream(ms, true);
				return pic;
			}
			catch
			{
				return null;
			}
		}
		
		/**m* SpringCardApplication/RtdVCard.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdVCard GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdVCard object, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdVCard GetContentEx()
		{
			RtdVCard nfcVCard = new RtdVCard();
			
			if (image != null)
			{
				pic = ImageToBase64(image, System.Drawing.Imaging.ImageFormat.Jpeg);
				nfcVCard.set_photo(pic);
			}
			
			foreach (Control c in pContact.Controls)
			{
				if (c is TextBox)
					nfcVCard.set_input_texts(c.Name, c.Text);
			}
			
			foreach(Control c in pBusiness.Controls)
			{
				if (c is TextBox)
					nfcVCard.set_input_texts(c.Name, c.Text);
			}
			
			foreach(Control c in pPrivate.Controls)
			{
				if (c is TextBox)
					nfcVCard.set_input_texts(c.Name, c.Text);
			}

			return nfcVCard;
		}
		
		
		private void BChangePictureClick(object sender, EventArgs e)
		{
			openFileDialog1.Filter =
				"Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
				"All files (*.*)|*.*";
			openFileDialog1.Title = "Select the picture";
			DialogResult dr = openFileDialog1.ShowDialog();
			
			if (dr == DialogResult.OK)
			{
				try
				{

					PictureBox pb = new PictureBox();
					Image loadedImage = Image.FromFile(openFileDialog1.FileName);
					
					/* First: re-dimension the image to go on the screen : 90*90	*/
					float WidthPercent = (float)loadedImage.Width/90;
					float HeigthPercent = (float)loadedImage.Height/90;
					float newWidth;
					float newHeigth;
					if (WidthPercent < HeigthPercent)
					{
						newWidth =loadedImage.Width/HeigthPercent;
						newHeigth = loadedImage.Height/HeigthPercent;
					}	else
					{
						newWidth = loadedImage.Width/WidthPercent;
						newHeigth =loadedImage.Height/WidthPercent;
					}

					Image NewLoadedImage = new Bitmap(loadedImage, new Size((int)newWidth, (int)newHeigth));
					
					pb.Size = new System.Drawing.Size(90, 90);
					pb.SizeMode = PictureBoxSizeMode.CenterImage;
					
					pb.Image = NewLoadedImage;
					
					flpPicture.Controls.Clear();
					flpPicture.Controls.Add(pb);
					
					image = NewLoadedImage;
					bRemovePicture.Enabled = true;
					pic = ImageToBase64(image, System.Drawing.Imaging.ImageFormat.Jpeg);
					lPSize.Text = ""+pic.Length + " bytes";
					
				}
				catch
				{

				}
			}

			
		}
		
		private void BRemovePictureClick(object sender, EventArgs e)
		{
			initializePicture();
			image = null;
			flpPicture.Controls.Clear();
			lPSize.Text = "";
		}
		
		public override void SetContent(Ndef ndef)
		{
			SaveString("VCard.LastName", Family_name.Text);
			SaveString("VCard.FirstName", First_name.Text);
			SaveString("VCard.eMail", Email.Text);
			SaveString("VCard.Company", Company.Text);

			if (ndef is RtdVCard)
				SetContent((RtdVCard) ndef);
		}
		
		public override Ndef GetContent()
		{
			return GetContentEx();
		}

		
	}
	
}
