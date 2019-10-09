/*
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
 * $Id: State.cs,v 1.1 2003/05/30 06:10:54 metaforge Exp $
 * 
 */
using System;
using System.Net.Sockets;

namespace SpringCard.LibCs.Net.Telnet
{
	/// <summary>
	/// State object for receiving data from remote device.
	/// </summary>
	public class State 
	{
		/// <summary>
		/// Size of receive buffer.
		/// </summary>
		public const int BufferSize = 256;
		/// <summary>
		/// Client socket.
		/// </summary>
		public Socket WorkSocket    = null;
		/// <summary>
		/// Receive buffer.
		/// </summary>
		public byte[] Buffer        = new byte[BufferSize];
	}
}
