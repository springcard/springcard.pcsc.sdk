/*
 * Ported from Matthias L. Jugel's and Marcus Meiﬂner's JTA.
 *
 * (c) Matthias L. Jugel, Marcus Meiﬂner 1996-2002. All Rights Reserved.
 * (c) Seva Petrov 2002. All Rights Reserved.
 *
 * --LICENSE NOTICE--
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 * --LICENSE NOTICE--
 *
 * $Date: 2003/05/30 06:10:54 $
 * $Id: TelnetProtocolHandler.cs,v 1.3 2003/05/30 06:10:54 metaforge Exp $
 * 
 */
using System;

namespace SpringCard.LibCs.Net.Telnet
{
	/// <summary>
	/// This is a telnet protocol handler. The handler needs implementations
	/// for several methods to handle the telnet options and to be able to
	/// read and write the buffer.
	/// </summary>
	public abstract class TelnetProtocolHandler
	{
		#region Globals and properties

		/// <summary>
		/// temporary buffer for data-telnetstuff-data transformation
		/// </summary>
		private byte[] tempbuf = new byte[0];

		/// <summary>
		/// the data sent on pressing [RETURN] \n
		/// </summary>
		private byte[] crlf = new byte[2];
		/// <summary>
		/// the data sent on pressing [LineFeed] \r
		/// </summary>
		private byte[] cr = new byte[2];

		/// <summary>
		/// Gets or sets the data sent on pressing [RETURN] \n
		/// </summary>
		public string CRLF
		{
			get
			{
				return System.Text.Encoding.ASCII.GetString(crlf);
			}
			set
			{
				crlf = System.Text.Encoding.ASCII.GetBytes(value);
			}
		}

		/// <summary>
		/// Gets or sets the data sent on pressing [LineFeed] \r
		/// </summary>
		public string CR
		{ 
			get 
			{
				return System.Text.Encoding.ASCII.GetString(cr);
			}
			set 
			{
				cr = System.Text.Encoding.ASCII.GetBytes(value);
			}
		}
        
		/// <summary>
		/// The current terminal type for TTYPE telnet option.
		/// </summary>
		protected string terminalType = "dumb"; 

		/// <summary>
		/// The window size of the terminal for the NAWS telnet option.
		/// </summary>
		protected int windowSizeHeight = 0;
		protected int windowSizeWidth = 0;

		/// <summary>
		/// Set the local echo option of telnet.
		/// </summary>
		/// <param name="echo">true for local echo, false for no local echo</param>
		protected abstract void SetLocalEcho(bool echo);

		/// <summary>
		/// Generate an EOR (end of record) request. For use by prompt displaying.
		/// </summary>
		protected abstract void NotifyEndOfRecord();

		/// <summary>
		/// Send data to the remote host.
		/// </summary>
		/// <param name="b">array of bytes to send</param>
		protected abstract void Write(byte[] b);

		/// <summary>
		/// Send one byte to the remote host.
		/// </summary>
		/// <param name="b">the byte to be sent</param>
		private void Write(byte b) 
		{
			Write(new byte[] {b});
		}

		/// <summary>
		/// Reset the protocol handler. This may be necessary after the
		/// connection was closed or some other problem occured.
		/// </summary>
		protected void Reset() 
		{
			neg_state = 0;
			receivedDX = new byte[256]; 
			sentDX = new byte[256];
			receivedWX = new byte[256]; 
			sentWX = new byte[256];
		}

		#endregion

		/// <summary>
		/// Create a new telnet protocol handler.
		/// </summary>
		public TelnetProtocolHandler() 
		{
			Reset();

			crlf[0] = 13; 
			crlf[1] = 10;
			cr[0] = 13; 
			cr[1] = 0;
		}

		/// <summary>
		/// state variable for telnet negotiation reader
		/// </summary>
		private byte neg_state = 0;
		
		/// <summary>
		/// What IAC SB we are handling right now
		/// </summary>
		private byte current_sb;

		#region Telnet protocol codes

		// constants for the negotiation state
		private const byte STATE_DATA            = 0;
		private const byte STATE_IAC             = 1;
		private const byte STATE_IACSB           = 2;
		private const byte STATE_IACWILL         = 3;
		private const byte STATE_IACDO           = 4;
		private const byte STATE_IACWONT         = 5;
		private const byte STATE_IACDONT         = 6;
		private const byte STATE_IACSBIAC        = 7;
		private const byte STATE_IACSBDATA       = 8;
		private const byte STATE_IACSBDATAIAC    = 9;

		/// <summary>
		/// IAC - init sequence for telnet negotiation.
		/// </summary>
		private const byte IAC             = 255;
		/// <summary>
		/// [IAC] End Of Record
		/// </summary>
		private const byte EOR             = 239;
		/// <summary>
		/// [IAC] WILL
		/// </summary>
		private const byte WILL            = 251;
		/// <summary>
		/// [IAC] WONT
		/// </summary>
		private const byte WONT            = 252;
		/// <summary>
		/// [IAC] DO
		/// </summary>
		private const byte DO              = 253;
		/// <summary>
		/// [IAC] DONT
		/// </summary>
		private const byte DONT            = 254;
		/// <summary>
		/// [IAC] Sub Begin
		/// </summary>
		private const byte SB              = 250;
		/// <summary>
		/// [IAC] Sub End
		/// </summary>
		private const byte SE              = 240;
		/// <summary>
		/// Telnet option: binary mode
		/// </summary>
		private const byte TELOPT_BINARY   = 0;  /* binary mode */
		/// <summary>
		/// Telnet option: echo text
		/// </summary>
		private const byte TELOPT_ECHO     = 1;  /* echo on/off */
		/// <summary>
		/// Telnet option: sga
		/// </summary>
		private const byte TELOPT_SGA      = 3;  /* supress go ahead */
		/// <summary>
		/// Telnet option: End Of Record
		/// </summary>
		private const byte TELOPT_EOR      = 25; /* end of record */
		/// <summary>
		/// Telnet option: Negotiate About Window Size
		/// </summary>
		private const byte TELOPT_NAWS     = 31; /* NA-WindowSize*/
		/// <summary>
		/// Telnet option: Terminal Type
		/// </summary>
		private const byte TELOPT_TTYPE    = 24;  /* terminal type */

		private static byte[] IACWILL  = { IAC, WILL };
		private static byte[] IACWONT  = { IAC, WONT };
		private static byte[] IACDO    = { IAC, DO   };
		private static byte[] IACDONT  = { IAC, DONT };
		private static byte[] IACSB    = { IAC, SB   };
		private static byte[] IACSE    = { IAC, SE   };

		/// <summary>
		/// Telnet option qualifier 'IS'
		/// </summary>
		private static byte TELQUAL_IS = (byte)0;
		/// <summary>
		/// Telnet option qualifier 'SEND'
		/// </summary>
		private static byte TELQUAL_SEND = (byte)1;

		#endregion

		/// <summary>
		/// What IAC DO(NT) request do we have received already ?
		/// </summary>
		private byte[] receivedDX;
		/// <summary>
		/// What IAC WILL/WONT request do we have received already ?
		/// </summary>
		private byte[] receivedWX;
		/// <summary>
		/// What IAC DO/DONT request do we have sent already ?
		/// </summary>
		private byte[] sentDX;
		/// <summary>
		/// What IAC WILL/WONT request do we have sent already ?
		/// </summary>
		private byte[] sentWX;

		#region The actual negotiation handling for the telnet protocol

		/// <summary>
		/// Send a Telnet Escape character
		/// </summary>
		/// <param name="code">IAC code</param>
		protected void SendTelnetControl(byte code)
		{
			byte[] b = new byte[2];

			b[0] = IAC;
			b[1] = code;
			Write(b);
		}

		/// <summary>
		/// Handle an incoming IAC SB type bytes IAC SE
		/// </summary>
		/// <param name="type">type of SB</param>
		/// <param name="sbdata">byte array as bytes</param>
		/// <param name="sbcount">nr of bytes. may be 0 too</param>
		private void HandleSB(byte type, byte[] sbdata, int sbcount)
		{
			switch (type)
			{
				case TELOPT_TTYPE:
					if (sbcount > 0 && sbdata[0] == TELQUAL_SEND)
					{
						Write(IACSB);
						Write(TELOPT_TTYPE);
						Write(TELQUAL_IS);
						/* FIXME: need more logic here if we use 
						* more than one terminal type
						*/
						Write(System.Text.Encoding.ASCII.GetBytes(terminalType));
						Write(IACSE);
					}
					break;
			}
		}

	
		/// <summary>
		/// Transpose special telnet codes like 0xff or newlines to values
		/// that are compliant to the protocol. This method will also send
		/// the buffer immediately after transposing the data.
		/// </summary>
		/// <param name="buf">the data buffer to be sent</param>
		protected void Transpose(byte[] buf)
		{
			int i;

			byte[] nbuf, xbuf;
			int nbufptr = 0;
			nbuf = new byte[buf.Length * 2];

			for (i = 0; i < buf.Length; i++)
			{
				switch (buf[i])
				{
						// Escape IAC twice in stream ... to be telnet protocol compliant
						// this is there in binary and non-binary mode.
					case IAC:
						nbuf[nbufptr++] = IAC;
						nbuf[nbufptr++] = IAC;
						break;
						// We need to heed RFC 854. LF (\n) is 10, CR (\r) is 13
						// we assume that the Terminal sends \n for lf+cr and \r for just cr
						// linefeed+carriage return is CR LF  
					case 10:    // \n
						if (receivedDX[TELOPT_BINARY + 128] != DO)
						{
							while (nbuf.Length - nbufptr < crlf.Length)
							{
								xbuf = new byte[nbuf.Length * 2];
								Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
								nbuf = xbuf;
							}
							for (int j = 0; j < crlf.Length; j++)
								nbuf[nbufptr++] = crlf[j];
							break;
						} 
						else 
						{
							// copy verbatim in binary mode.
							nbuf[nbufptr++] = buf[i];
						}
						break;
						// carriage return is CR NUL */ 
					case 13:    // \r
						if (receivedDX[TELOPT_BINARY + 128] != DO) 
						{
							while (nbuf.Length - nbufptr < cr.Length)
							{
								xbuf = new byte[nbuf.Length * 2];
								Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
								nbuf = xbuf;
							}
							for (int j = 0; j < cr.Length; j++)
								nbuf[nbufptr++] = cr[j];
						} 
						else
						{
							// copy verbatim in binary mode.
							nbuf[nbufptr++] = buf[i];
						}
						break;
						// all other characters are just copied
					default:
						nbuf[nbufptr++] = buf[i];
						break;
				}
			}
			xbuf = new byte[nbufptr];
			Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
			Write(xbuf);
		}


		/// <summary>
		/// Handle telnet protocol negotiation. The buffer will be parsed
		/// and necessary actions are taken according to the telnet protocol.
		/// <see cref="RFC-Telnet"/>
		/// </summary>
		/// <param name="nbuf">the byte buffer used for negotiation</param>
		/// <returns>a new buffer after negotiation</returns>
		protected int Negotiate(byte[] nbuf)
		{
			int count = tempbuf.Length;
			if (count == 0)     // buffer is empty.
				return -1;

			byte[] sendbuf = new byte[3];
			byte[] sbbuf   = new byte[tempbuf.Length];
			byte[] buf     = tempbuf;
			
			byte b;
			byte reply;
			
			int sbcount = 0;
			int boffset = 0;
			int noffset = 0;

			bool done    = false;
			bool foundSE = false;
			
			//bool dobreak = false;

			while (!done && (boffset < count && noffset < nbuf.Length))
			{
				b = buf[boffset++];

				// of course, byte is a signed entity (-128 -> 127)
				// but apparently the SGI Netscape 3.0 doesn't seem
				// to care and provides happily values up to 255
				if (b >= 128)
					b = (byte)((int)b - 256);

				switch (neg_state)
				{
					case STATE_DATA:
						if (b == IAC)
						{
							neg_state = STATE_IAC;
							//dobreak = true; // leave the loop so we can sync.
						} 
						else 
						{
							nbuf[noffset++] = b;
						}
						break;
					
					case STATE_IAC:
						switch (b)
						{
							case IAC:
								neg_state = STATE_DATA;
								nbuf[noffset++] = IAC; // got IAC, IAC: set option to IAC
								break;
							
							case WILL:
								neg_state = STATE_IACWILL;
								break;
							
							case WONT:
								neg_state = STATE_IACWONT;
								break;
							
							case DONT:
								neg_state = STATE_IACDONT;
								break;
							
							case DO:
								neg_state = STATE_IACDO;
								break;
							
							case EOR:
								NotifyEndOfRecord();
								//dobreak = true; // leave the loop so we can sync.
								neg_state = STATE_DATA;
								break;
							
							case SB:
								neg_state = STATE_IACSB;
								sbcount = 0;
								break;
							
							default:
								neg_state = STATE_DATA;
								break;
						}
						break;
					
					case STATE_IACWILL:
						switch(b)
						{
							case TELOPT_ECHO:
								reply = DO;
								SetLocalEcho(false);
								break;

							case TELOPT_SGA:
								reply = DO;
								break;
							
							case TELOPT_EOR:
								reply = DO;
								break;

							case TELOPT_BINARY:
								reply = DO;
								break;

							default:
								reply = DONT;
								break;
						}
						
						if (reply != sentDX[b + 128] || WILL != receivedWX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							Write(sendbuf);
							
							sentDX[b + 128] = reply;
							receivedWX[b + 128] = WILL;
						}

						neg_state = STATE_DATA;
						break;
					
					case STATE_IACWONT:
						
						switch(b) 
						{
							case TELOPT_ECHO:
								SetLocalEcho(true);
								reply = DONT;
								break;
							
							case TELOPT_SGA:
								reply = DONT;
								break;

							case TELOPT_EOR:
								reply = DONT;
								break;

							case TELOPT_BINARY:
								reply = DONT;
								break;

							default:
								reply = DONT;
								break;
						}

						if(reply != sentDX[b + 128] || WONT != receivedWX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							Write(sendbuf);
							
							sentDX[b + 128] = reply;
							receivedWX[b + 128] = WILL;
						}

						neg_state = STATE_DATA;
						break;
					
					case STATE_IACDO:
						switch (b)
						{
							case TELOPT_ECHO:
								reply = WILL;
								SetLocalEcho(true);
								break;
							
							case TELOPT_SGA:
								reply = WILL;
								break;
							
							case TELOPT_TTYPE:
								reply = WILL;
								break;
							
							case TELOPT_BINARY:
								reply = WILL;
								break;
							
							case TELOPT_NAWS:
								int sizeWidth = windowSizeWidth;
								int sizeHeight = windowSizeHeight;
								receivedDX[b] = DO;

								reply = WILL;
								sentWX[b] = WILL;
								sendbuf[0]=IAC;
								sendbuf[1]=WILL;
								sendbuf[2]=TELOPT_NAWS;
								
								Write(sendbuf);
								Write(IAC);
								Write(SB);
								Write(TELOPT_NAWS);
								Write((byte) (sizeWidth >> 8));
								Write((byte) (sizeWidth & 0xff));
								Write((byte) (sizeHeight >> 8));
								Write((byte) (sizeHeight & 0xff));
								Write(IAC);Write(SE);
								break;
							
							default:
								reply = WONT;
								break;
						}

						if (reply != sentWX[128 + b] || DO != receivedDX[128 + b])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							Write(sendbuf);
							
							sentWX[b + 128] = reply;
							receivedDX[b + 128] = DO;
						}
						
						neg_state = STATE_DATA;
						break;
					
					case STATE_IACDONT:
						switch (b)
						{
							case TELOPT_ECHO:
								reply = WONT;
								SetLocalEcho(false);
								break;

							case TELOPT_SGA:
								reply = WONT;
								break;

							case TELOPT_NAWS:
								reply = WONT;
								break;

							case TELOPT_BINARY:
								reply = WONT;
								break;

							default:
								reply = WONT;
								break;
						}
						
						if (reply != sentWX[b + 128] || DONT != receivedDX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							Write(sendbuf);
							
							sentWX[b + 128] = reply;
							receivedDX[b + 128] = DONT;
						}
						
						neg_state = STATE_DATA;
						break;
					
					case STATE_IACSBIAC:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < tempbuf.Length; i++)
							if (tempbuf[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							break;
						}

						foundSE = false;

						if (b == IAC)
						{
							sbcount = 0;
							current_sb = b;
							neg_state = STATE_IACSBDATA;
						}
						else
						{
							neg_state = STATE_DATA;
						}
						break;
					
					case STATE_IACSB:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < tempbuf.Length; i++)
							if (tempbuf[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							break;
						}

						foundSE = false;

						switch (b)
						{
							case IAC:
								neg_state = STATE_IACSBIAC;
								break;
							
							default:
								current_sb = b;
								sbcount = 0;
								neg_state = STATE_IACSBDATA;
								break;
						}
						break;
					
					case STATE_IACSBDATA:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < tempbuf.Length; i++)
							if (tempbuf[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							break;
						}

						foundSE = false;

						switch (b)
						{
							case IAC:
								neg_state = STATE_IACSBDATAIAC;
								break;
							default:
								sbbuf[sbcount++] = b;
								break;
						}
						break;
					
					case STATE_IACSBDATAIAC:
						switch (b)
						{
							case IAC:
								neg_state = STATE_IACSBDATA;
								sbbuf[sbcount++] = IAC;
								break;
							case SE:
								HandleSB(current_sb,sbbuf,sbcount);
								current_sb = 0;
								neg_state = STATE_DATA;
								break;
							case SB:
								HandleSB(current_sb,sbbuf,sbcount);
								neg_state = STATE_IACSB;
								break;
							default:
								neg_state = STATE_DATA;
								break;
						}
						break;
					
					default:
						neg_state = STATE_DATA;
						break;
				}
			}
			
			// shrink tempbuf to new processed size.
			byte[] xb = new byte[count - boffset];
			Array.Copy(tempbuf, boffset, xb, 0, count - boffset);
			tempbuf = xb;

			return noffset;
		}

		#endregion

		/// <summary>
		/// Adds bytes to the input buffer we'll parse for codes.
		/// </summary>
		/// <param name="b">Bytes array from which to add.</param>
		/// <param name="len">Number of bytes to add.</param>
		protected void InputFeed(byte[] b, int len)
		{
			byte[] bytesTmp = new byte[tempbuf.Length + len];

			Array.Copy(tempbuf, 0, bytesTmp, 0, tempbuf.Length);
			Array.Copy(b, 0, bytesTmp, tempbuf.Length, len);
			
			tempbuf = bytesTmp;
		}
	}
}
