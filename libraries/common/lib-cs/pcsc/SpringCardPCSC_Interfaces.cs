/**
 *
 * \ingroup PCSC 
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D / SpringCard 
 *
 */
/*
 *	This software is part of the SPRINGCARD SDK FOR PC/SC
 *
 *   Redistribution and use in source (source code) and binary
 *   (object code) forms, with or without modification, are
 *   permitted provided that the following conditions are met :
 *
 *   1. Redistributed source code or object code shall be used
 *   only in conjunction with products (hardware devices) either
 *   manufactured, distributed or developed by SPRINGCARD,
 *
 *   2. Redistributed source code, either modified or
 *   un-modified, must retain the above copyright notice,
 *   this list of conditions and the disclaimer below,
 *
 *   3. Redistribution of any modified code must be clearly
 *   identified "Code derived from original SPRINGCARD 
 *   copyrighted source code", with a description of the
 *   modification and the name of its author,
 *
 *   4. Redistributed object code must reproduce the above
 *   copyright notice, this list of conditions and the
 *   disclaimer below in the documentation and/or other
 *   materials provided with the distribution,
 *
 *   5. The name of SPRINGCARD may not be used to endorse
 *   or promote products derived from this software or in any
 *   other form without specific prior written permission from
 *   SPRINGCARD.
 *
 *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
 *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
 *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
 *
 *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *
 *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
 *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
 *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
 *
 *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 *   OF SUCH DAMAGE. 
 *
 **/
using SpringCard.LibCs;
using System;
using System.Collections.Generic;

namespace SpringCard.PCSC
{
	/**
	 *
	 * \brief Interface to exchange APDUs with a smartcard (byte[] version) 
	 *
	 **/
    public interface ICardTransmitter
    {
        byte[] Transmit(byte[] command);

        void Disconnect();
        void Reconnect();
    }

    /**
	 *
	 * \brief Interface to exchange APDUs with a smartcard (RAPDU / CAPDU version) 
	 *
	 **/
    public interface ICardApduTransmitter
    {
        RAPDU Transmit(CAPDU capdu);

        void Disconnect();
        void Reconnect();
    }


    public class VirtualCard : ICardTransmitter, ICardApduTransmitter
    {
        private Dictionary<string, string> apdus = new Dictionary<string, string>();

        public void Clear()
        {
            apdus.Clear();
        }

        public void AddApduPair(string command, string response)
        {
            apdus.Add(command, response);
        }

        public void AddApduPair(byte[] command, byte[] response)
        {
            AddApduPair(BinConvert.ToHex(command), BinConvert.ToHex(response));
        }

        public void AddApduPair(CAPDU command, RAPDU response)
        {
            AddApduPair(command.Bytes, response.Bytes);
        }

        public byte[] Transmit(byte[] command)
        {
            string sCommand = BinConvert.ToHex(command);
            if (apdus.ContainsKey(sCommand))
                return BinConvert.HexToBytes(apdus[sCommand]);
            return new byte[2] { 0x6F, 0x00 };
        }

        public RAPDU Transmit(CAPDU command)
        {
            return new RAPDU(Transmit(command.Bytes));
        }

        public void Disconnect()
        {

        }

        public void Reconnect()
        {

        }
    }
}
