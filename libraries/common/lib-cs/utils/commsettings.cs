/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
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
using System.IO.Ports;

namespace SpringCard.LibCs
{
	/**
	 * \brief Manipulation of comm. port settings
	 */
	public class CommSettings
	{
		int _bitrate = 38400;
		int _databits = 8;
		Parity _parity = Parity.None;
		StopBits _stopbits = StopBits.One;
		Handshake _handshake = Handshake.None;

		public int BitRate
		{
			get { return _bitrate; }
			set { _bitrate = value; }
		}

		public int DataBits
		{
			get { return _databits; }
			set { _databits = value; }
		}

		public StopBits StopBits
		{
			get { return _stopbits; }
			set { _stopbits = value; }
		}

		public Parity Parity
		{
			get { return _parity; }
			set { _parity = value; }
		}

		public Handshake Handshake
		{
			get { return _handshake; }
			set { _handshake = value; }
		}

		public CommSettings(string settings)
		{
			_handshake = Handshake.None;
			_parity = Parity.None;
			_stopbits = StopBits.One;
			_databits = 8;
			_bitrate = 38400;

			string[] t = settings.Split(':');

			if (t.Length >= 1)
			{
				int.TryParse(t[0], out _bitrate);
			}
			if (t.Length >= 2)
			{
				int.TryParse(t[1], out _databits);
			}
			if (t.Length >= 3)
			{
				switch (t[2].ToUpper())
				{
					case "O":
						_parity = Parity.Odd;
						break;

					case "E":
						_parity = Parity.Even;
						break;

					case "N":
					default:
						break;
				}
			}
			if (t.Length >= 4)
			{
				switch (t[3])
				{
					case "1.5":
						_stopbits = StopBits.OnePointFive;
						break;

					case "2":
						_stopbits = StopBits.Two;
						break;

					case "1":
					default:
						_stopbits = StopBits.One;
						break;
				}
			}
			if (t.Length >= 5)
			{
				switch (t[4].ToUpper())
				{
					case "HW":
						_handshake = Handshake.RequestToSend;
						break;

					case "XON":
						_handshake = Handshake.XOnXOff;
						break;

					case "":
					default:
						break;
				}
			}
		}

		public CommSettings(int bitrate, int databits, Parity parity, StopBits stopbits, Handshake handshake)
		{
			_bitrate = bitrate;
			_databits = databits;
			_parity = parity;
			_stopbits = stopbits;
			_handshake = handshake;
		}

		public string Explain()
		{
			string r = "";

			r += String.Format("{0}", _bitrate);
			r += " bit/s, ";
			r += String.Format("{0}", _databits);
			r += " data bits, ";
			switch (_stopbits)
			{
				case StopBits.Two:
					r += "2 stop bits";
					break;

				case StopBits.OnePointFive:
					r += "1.5 stop bits";
					break;

				case StopBits.One:
				default:
					r += "1 stop bit";
					break;
			}
			r += ", ";
			switch (_parity)
			{
				case Parity.Odd:
					r += "parity Odd";
					break;

				case Parity.Even:
					r += "parity Even";
					break;

				case Parity.None:
				default:
					r += "no parity";
					break;
			}
			r += ", ";
			switch (_handshake)
			{
				case Handshake.RequestToSend:
					r += "flow control: hardware";
					break;

				case Handshake.XOnXOff:
					r += "flow control: Xon/Xoff";
					break;

				case Handshake.None:
				default:
					r += "no flow control";
					break;
			}

			return r;
		}

		public string AsString()
		{
			string r = "";

			r += String.Format("{0}", _bitrate);
			r += ":";
			r += String.Format("{0}", _databits);
			r += ":";
			switch (_parity)
			{
				case Parity.Odd:
					r += "O";
					break;

				case Parity.Even:
					r += "E";
					break;

				case Parity.None:
				default:
					r += "N";
					break;
			}
			r += ":";
			switch (_stopbits)
			{
				case StopBits.Two:
					r += "2";
					break;

				case StopBits.OnePointFive:
					r += "1.5";
					break;

				case StopBits.One:
				default:
					r += "1";
					break;
			}
			r += ":";
			switch (_handshake)
			{
				case Handshake.RequestToSend:
					r += "HW";
					break;

				case Handshake.XOnXOff:
					r += "XON";
					break;

				case Handshake.None:
				default:
					r += "";
					break;
			}

			return r;
		}
	}
}
