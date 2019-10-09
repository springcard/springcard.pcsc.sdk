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
 * $Id: DataAvailableEventArgs.cs,v 1.1 2003/05/30 06:10:54 metaforge Exp $
 * 
 */
using System;

namespace SpringCard.LibCs.Net.Telnet
{
	/// <summary>
	/// Encapsulates arguments of the event fired when data becomes
	/// available on the telnet socket.
	/// </summary>
	public class DataAvailableEventArgs : EventArgs
	{
		private string data;
		
		/// <summary>
		/// Creates a new instance of the DataAvailableEventArgs class.
		/// </summary>
		/// <param name="output">Output from the session.</param>
		public DataAvailableEventArgs(string output)
		{
			data = output;
		}

		/// <summary>
		/// Gets the data from the telnet session.
		/// </summary>
		public string Data 
		{
			get 
			{
				return data;
			}
		}
	}
}
