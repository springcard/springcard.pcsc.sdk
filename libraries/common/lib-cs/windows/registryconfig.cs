/**
 *
 * \ingroup Windows
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
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows
{
    /**
	 * \brief Registry-based configuration
	 */
    public class RegistryCfgFile : IConfigReader, IConfigWriter
    {
        private bool writable;
        private RegistryKey registryKey;
        public string Prefix = "";

        private RegistryValueKind getKind(string Name)
        {
            try
            {
                return registryKey.GetValueKind(Prefix + Name);
            }
            catch (Exception e)
            {
                return RegistryValueKind.None;
            }
        }

        /**
		 * \brief Read a string value
		 */
        public string ReadString(string Name, string Default = "")
        {
            if (registryKey == null)
                return Default;

            try
            {
                return (string)registryKey.GetValue(Prefix + Name, Default);
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                return Default;
            }
        }

        /**
		 * \brief Read an integer value
		 */
        public int ReadInteger(string Name, int Default = 0)
        {
            if (registryKey == null)
                return Default;

            RegistryValueKind k = getKind(Name);
            if (k == RegistryValueKind.String)
            {
                string s = ReadString(Name, Default.ToString());
                int r;
                int.TryParse(s, out r);
                return r;
            }
            else if (k == RegistryValueKind.DWord)
            {
                try
                {
                    Int32 r = (Int32)registryKey.GetValue(Prefix + Name, Default);
                    return (int)r;
                }
                catch (Exception e)
                {
                    Logger.Debug(e.Message);
                    return Default;
                }
            }
            else
            {
                return Default;
            }
        }

        /**
		 * \brief Read an unsigned integer value
		 */
        public uint ReadUnsigned(string Name, uint Default = 0)
        {
            if (registryKey == null)
                return Default;

            RegistryValueKind k = getKind(Name);
            if (k == RegistryValueKind.String)
            {
                string s = ReadString(Name, Default.ToString());
                uint r;
                uint.TryParse(s, out r);
                return r;
            }
            else if (k == RegistryValueKind.DWord)
            {
                try
                {
                    Int32 r = (Int32)registryKey.GetValue(Prefix + Name, Default);
                    return (uint) r;
                }
                catch (Exception e)
                {
                    Logger.Debug(e.Message);
                    return Default;
                }
            }
            else
            {
                return Default;
            }
        }

        /**
		 * \brief Read a boolean value
		 */
        public bool ReadBoolean(string Name, bool Default = false)
        {
            if (registryKey == null)
                return Default;

            RegistryValueKind k = getKind(Name);
            if (k == RegistryValueKind.String)
            {
                bool v;
                bool r = StrUtils.ReadBoolean(ReadString(Name, null), out v);
                if (!v)
                    return Default;
                return r;
            }
            else if (k == RegistryValueKind.DWord)
            {
                uint r = ReadUnsigned(Name);
                return (r != 0);
            }
            else
            {
                return Default;
            }
        }

        /**
		 * \brief Remove an entry
		 */
        public bool Remove(string Name)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            try
            {
                registryKey.DeleteValue(Prefix + Name);
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /**
		 * \brief Write an empty entry
		 */
        public bool WriteName(string Name)
        {
            return WriteString(Name, null);
        }

        /**
		 * \brief Write a string entry
		 */
        public bool WriteString(string Name, string Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            try
            {
                registryKey.SetValue(Prefix + Name, Value, RegistryValueKind.String);
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /**
		 * \brief Write an integer entry
		 */
        public bool WriteInteger(string Name, int Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            try
            {
                Int32 v = (Int32)Value;
                registryKey.SetValue(Prefix + Name, v, RegistryValueKind.DWord);
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /**
		 * \brief Write an unsigned integer entry
		 */
        public bool WriteUnsigned(string Name, uint Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            try
            {
                Int32 v = (Int32)Value;
                registryKey.SetValue(Prefix + Name, v, RegistryValueKind.DWord);
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /**
		 * \brief Write a boolean entry
		 */
        public bool WriteBoolean(string Name, bool value)
        {
            int i = value ? 1 : 0;
            return WriteInteger(Name, i);
        }

        /**
         * \brief Open branch
         */
        public enum RegistryRoot
        {
            CurrentUser,
            LocalMachine
        }

        private static RegistryCfgFile Open(RegistryRoot Root, string FullPath, bool Writable)
        {
            RegistryCfgFile result = new RegistryCfgFile();
            result.writable = Writable;
            if (Root == RegistryRoot.CurrentUser)
            {
                if (Writable)
                {
                    result.registryKey = Registry.CurrentUser.CreateSubKey(FullPath);
                }
                else
                {
                    result.registryKey = Registry.CurrentUser.OpenSubKey(FullPath);
                }
                if (result.registryKey == null)
                {
                    Logger.Debug(@"Failed opening HKCU\{0} ({1})", FullPath, Writable ? "read/write" : "read only");
                }
            }
            else if (Root == RegistryRoot.LocalMachine)
            {
                if (Writable)
                {
                    result.registryKey = Registry.LocalMachine.CreateSubKey(FullPath);
                }
                else
                {
                    result.registryKey = Registry.LocalMachine.OpenSubKey(FullPath);
                }
                if (result.registryKey == null)
                {
                    Logger.Debug(@"Failed opening HKLM\{0} ({1})", FullPath, Writable ? "read/write" : "read only");
                }
            }
            else
            {
                throw new Exception("Invalid root for RegistryCfgFile");
            }

            return result;
        }

        /**
         * \brief Open a registry path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(RegistryRoot Root, string FullPath)
        {
            return Open(Root, FullPath, false);
        }

        /**
         * \brief Open a registry path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(RegistryRoot Root, string FullPath)
        {
            return Open(Root, FullPath, true);
        }

        /**
         * \brief Open a CurrentUser registry path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(string FullPath)
        {
            return Open(RegistryRoot.CurrentUser, FullPath, false);
        }

        /**
         * \brief Open a CurrentUser registry path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(string FullPath)
        {
            return Open(RegistryRoot.CurrentUser, FullPath, true);
        }

        /**
         * \brief Open a registry company / application / section path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(RegistryRoot Root, string CompanyName, string ApplicationName, string SectionName = "")
        {
            string FullPath = @"SOFTWARE\" + CompanyName + @"\" + ApplicationName;
            if (!string.IsNullOrEmpty(SectionName))
                FullPath += @"\" + SectionName;
            return Open(Root, FullPath, false);
        }

        /**
         * \brief Open a registry company / application / section path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(RegistryRoot Root, string CompanyName, string ApplicationName, string SectionName = "")
        {
            string FullPath = @"SOFTWARE\" + CompanyName + @"\" + ApplicationName;
            if (!string.IsNullOrEmpty(SectionName))
                FullPath += @"\" + SectionName;
            return Open(Root, FullPath, true);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(string CompanyName, string ApplicationName, string SectionName = "")
        {
            return OpenReadOnly(RegistryRoot.CurrentUser, CompanyName, ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in write-only mode
         */
        public static RegistryCfgFile OpenReadWrite(string CompanyName, string ApplicationName, string SectionName = "")
        {
            return OpenReadWrite(RegistryRoot.CurrentUser, CompanyName, ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry in read-only mode for the current application
         */
        public static RegistryCfgFile OpenApplicationSectionReadOnly(string SectionName = "")
        {
            return OpenReadOnly(RegistryRoot.CurrentUser, AppUtils.CompanyName, AppUtils.ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in write-only mode
         */
        public static RegistryCfgFile OpenApplicationSectionReadWrite(string SectionName = "")
        {
            return OpenReadWrite(RegistryRoot.CurrentUser, AppUtils.CompanyName, AppUtils.ApplicationName, SectionName);
        }
    }
}