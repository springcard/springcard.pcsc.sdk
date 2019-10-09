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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SpringCard.LibCs
{
    /**
	 * \brief Encapsulation of various configuration-related objects (reader part)
	 */
    public interface IConfigReader
    {
        string ReadString(string Name, string Default = "");
        int ReadInteger(string Name, int Default = 0);
        uint ReadUnsigned(string Name, uint Default = 0);
        bool ReadBoolean(string Name, bool Default = false);
    }

    /**
	 * \brief Encapsulation of various configuration-related objects (writer part)
	 */
    public interface IConfigWriter
    {
        bool Remove(string Name);
        bool WriteName(string Name);
        bool WriteString(string Name, string Value);
        bool WriteInteger(string Name, int Value);
        bool WriteUnsigned(string Name, uint Value);
        bool WriteBoolean(string Name, bool value);
    }
}