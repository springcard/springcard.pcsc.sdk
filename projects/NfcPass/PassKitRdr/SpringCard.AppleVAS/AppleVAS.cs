
/*
 * Read LICENSE.txt for license details and restrictions.
 */


using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
/**
*
* \defgroup AppleVAS   SpringCard AppleVAS Library for .NET
*
* \brief Terminal-side implementation of Apple VAS protocol, to read Pass Kit / Wallet data through NFC
*
* This Library implements the terminal-side specification of Apple's Wallet Package Format / Loyalty enhancement, version 1.0 draft 6 dated 15/10/2015
* 
* Basic usage:
* - create a AppleVasConfig object, holding your merchant's Merchant ID and ECC Private Key
* - create a AppleVasTerminal, and feed it with the AppleVasConfig (you may also associate feed than one AppleVasConfig to a single AppleVasController)
* - connect to a PC/SC reader using the SpringCard.PCSC Class Library. The connection to the iPhone will be a CardChannel instance
* - run the AppleVasTerminal.DoTransaction method over the CardChannel instance
* - DoTransaction returns an AppleVasData object holding the pass message
* 
* Target: .NET framework v4.6.2.
* 
* \copyright
*   Copyright (c) 2018-2019 SpringCard - www.springcard.com
*   All right reserved
*
* \author
*   Johann.D et al. / SpringCard
*
*/
namespace SpringCard.AppleVas
{
    /**
	 * \brief Library information
	 **/
    public static class Library
    {
        /**
         * \brief Read Library information
         **/
        public static readonly AppModuleInfo ModuleInfo = new AppModuleInfo(Assembly.GetExecutingAssembly());
    }
}
