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
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs
{
    public class AppModuleInfo
    {
        internal string Filename { get; private set; }
        internal FileVersionInfo VersionInfo { get; private set; }

        public AppModuleInfo(Assembly assembly)
        {
            try
            {
                this.Filename = assembly.Location;
                VersionInfo = FileVersionInfo.GetVersionInfo(this.Filename);
            }
            catch { }
        }

        public AppModuleInfo(string filename)
        {
            try
            {
                this.Filename = filename;
                VersionInfo = FileVersionInfo.GetVersionInfo(this.Filename);
            }
            catch { }
        }

        public string IdentificationString
        {
            get
            {
                return String.Format("{0} {1} {2}", VersionInfo.CompanyName, VersionInfo.ProductName, VersionInfo.ProductVersion);
            }
        }

        public string Version
        {
            get
            {
                return GetVersion(VersionInfo.ProductVersion);
            }
        }

        public string LongVersion
        {
            get
            {
                return GetLongVersion(VersionInfo.ProductVersion);
            }
        }

        public string FullVersion
        {
            get
            {
                return VersionInfo.ProductVersion;
            }
        }

        public static string GetVersion(string FullVersion)
        {
            string[] e = FullVersion.Split('.');
            if (e.Length < 4) return FullVersion;
            return string.Format("{0}.{1}", e[0], e[1]);
        }

        public static string GetLongVersion(string FullVersion)
        {
            string[] e = FullVersion.Split('.');
            if (e.Length < 4) return FullVersion;
            return string.Format("{0}.{1} [{2}.{3}]", e[0], e[1], e[2], e[3]);
        }

        public string Copyright
        {
            get
            {
                return VersionInfo.LegalCopyright;
            }
        }
    }

    /**
	 * \brief Utility to retrieve most information regarding the running program.
	 */
    public static class AppInfo
	{
		private static AppModuleInfo mainModuleInfo;
		private static List<AssemblyInfo> assemblyInfoTree = new List<AssemblyInfo>();

		private static void Prepare()
		{
			if (mainModuleInfo == null)
                mainModuleInfo = new AppModuleInfo(Assembly.GetEntryAssembly());
		}

		/**
		 * \brief Get the program's execution directory
		 */
		public static string ExecutionDirectory
		{
			get
			{
				Prepare();
				return Path.GetDirectoryName(mainModuleInfo.Filename);
			}
		}

		/**
		 * \brief Get the program's FileVersionInfo
		 */		
		public static FileVersionInfo VersionInfo
		{
			get
			{
				Prepare();
				return mainModuleInfo.VersionInfo;
			}
		}

		/**
		 * \brief Get the program's complete name: Company name + product name + product version
		 */		
		public static string IdentificationString
		{
			get
			{
				Prepare();
				return mainModuleInfo.IdentificationString;
			}
		}

        /**
		 * \brief Get the program's displayable name
		 */
        public static string Name
        {
            get
            {
                Prepare();
                return mainModuleInfo.VersionInfo.ProductName;
            }
        }

        /**
		 * \brief Get the program's version (MM.mm only)
		 */
        public static string Version
		{
			get
			{
				Prepare();
                return mainModuleInfo.Version;
			}
		}

        /**
		 * \brief Get the program's version in the form MM.mm [rr.bb]
		 */
        public static string LongVersion
		{
			get
			{
				Prepare();
                return mainModuleInfo.LongVersion;
			}
		}

        /**
		 * \brief Get the program's version in the form MM.mm.rr.bb
		 */
        public static string FullVersion
        {
            get
            {
                Prepare();
                return mainModuleInfo.FullVersion;
            }
        }

        /**
		 * \brief Get the program's copyright
		 */
        public static string Copyright
        {
            get
            {
                Prepare();
                return mainModuleInfo.VersionInfo.LegalCopyright;
            }
        }

        /**
		 * \brief Get the name of the program to be displayed in the main form's title bar
		 */
        public static string WindowsTitle
		{
			get
			{
				Prepare();
				return String.Format("{0} {1} v.{2}", mainModuleInfo.VersionInfo.CompanyName, mainModuleInfo.VersionInfo.ProductName, Version);
			}
		}

		/**
		 * \brief Dump the list of assemblies, with their version, through the Logger output
		 */
		public static void LogAssemblyList(bool onlyLocals = true, Logger.Level level = Logger.Level.Info)
		{
			AssemblyInfo[] infos = GetAssembliesInfo(onlyLocals);
			foreach (AssemblyInfo info in infos)
				Logger.Log(level, info.ToString());
		}

		/**
		 * \brief Information regarding an assembly (program or library)
		 */		
		public class AssemblyInfo : IEquatable<AssemblyInfo>, IComparable<AssemblyInfo>
		{
			AssemblyName name;
			Assembly assembly;
			FileVersionInfo info;

			internal AssemblyInfo(AssemblyName name, Assembly assembly, FileVersionInfo info)
			{
				this.name = name;
				this.assembly = assembly;
				this.info = info;
			}

            public static AssemblyInfo Create(string FileName)
            {
                try
                {
                    AssemblyName name = AssemblyName.GetAssemblyName(FileName);
                    Assembly assembly = Assembly.LoadFile(FileName);
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FileName);

                    return new AssemblyInfo(name, assembly, info);
                }
                catch
                {
                    return null;
                }
            }

			/**
			 * \brief Name of the program or library
			 */
			public string Name
			{
				get
				{
					return name.Name;
				}
			}

			/**
			 * \brief Version of the program or library
			 */			
			public string Version
			{
				get
				{
					string result = info.FileVersion;

					if (info.IsPreRelease)
						result += " [Beta]";
					if (info.IsPatched)
						result += " [Patch]";
					if (info.IsDebug)
						result += " [Debug]";

					return result;
				}
			}

			/**
			 * \brief On-disk file name of the program or library
			 */			
			public string FileName
			{
				get
				{
					return assembly.Location;
				}
			}

			/**
			 * \brief The base name of the file name of the program or library
			 */			
			public string BaseName
			{
				get
				{
					return Path.GetFileName(FileName);
				}
			}

			public override string ToString()
			{
				string result = Name;
				result += " v." + Version;
				result += " in " + FileName;
				return result;
			}

			public override bool Equals(object obj)
			{
				if (obj == null) return false;
				AssemblyInfo objAsPart = obj as AssemblyInfo;
				if (objAsPart == null) return false;
				else return Equals(objAsPart);
			}

			public override int GetHashCode()
			{
				return assembly.GetHashCode();
			}

			public bool Equals(AssemblyInfo other)
			{
				if (other == null) return false;
				return (this.Name.Equals(other.Name));
			}

			public int CompareTo(AssemblyInfo comparePart)
			{
				// A null value means that this object is greater.
				if (comparePart == null)
					return 1;
				else
					return this.BaseName.CompareTo(comparePart.BaseName);
			}
		}

		private static bool AssemblyInfoExists(string name)
		{
			foreach (AssemblyInfo info in assemblyInfoTree)
				if (info.Name == name)
					return true;
			return false;
		}

		private static void CreateAssemblyInfoList(Assembly parent = null)
		{
			if (parent == null)
			{
				assemblyInfoTree.Clear();
				parent = Assembly.GetEntryAssembly();
			}

			AssemblyName[] names = parent.GetReferencedAssemblies();

			foreach (AssemblyName name in names)
			{
				if (AssemblyInfoExists(name.Name))
					continue;

				Assembly assembly = null;
				try
				{
					assembly = Assembly.Load(name.FullName);
				}
				catch { }

				if (assembly == null)
					continue;

				FileVersionInfo version = null;
				try
				{
					version = FileVersionInfo.GetVersionInfo(assembly.Location);
				}
				catch { }

				if (version == null)
					continue;

				assemblyInfoTree.Add(new AssemblyInfo(name, assembly, version));

				CreateAssemblyInfoList(assembly);
			}
		}

		/**
		 * \brief Selection criteria for GetAssembliesInfo()
		 */		
		public enum AssemblyInfoSelect
		{
			All, /*!< All assemblies referenced by the program, whatever their location */
			OnlyLocals, /*!< All the assemblies that are in the program's execution directory */
			AllButLocals /*!< All the assemblies that are outside of the program's execution directory */
		}

		/**
		 * \brief Get a list of AssemblyInfo for all assemblies referenced by the program
		 */		
		public static AssemblyInfo[] GetAssembliesInfo(AssemblyInfoSelect select = AssemblyInfoSelect.OnlyLocals)
		{
			if (assemblyInfoTree.Count == 0)
				CreateAssemblyInfoList();

			List<AssemblyInfo> result = new List<AssemblyInfo>();

			string localDirectory = ExecutionDirectory.ToLower();

			foreach (AssemblyInfo info in assemblyInfoTree)
			{
				if (select == AssemblyInfoSelect.OnlyLocals)
				{
					if (!Path.GetDirectoryName(info.FileName).ToLower().Equals(localDirectory))
						continue;
				}
				else
				if (select == AssemblyInfoSelect.AllButLocals)
				{
					if (Path.GetDirectoryName(info.FileName).ToLower().Equals(localDirectory))
						continue;
				}

				result.Add(info);
			}

			result.Sort();

			return result.ToArray();
		}

		/**
		 * \brief Get a list of AssemblyInfo for all assemblies referenced by the program
		 */		
		public static AssemblyInfo[] GetAssembliesInfo(bool onlyLocals = true)
		{
			AssemblyInfoSelect select = onlyLocals ? AssemblyInfoSelect.OnlyLocals : AssemblyInfoSelect.All;
			return GetAssembliesInfo(select);
		}



        /**
		 * \brief Expose a program version in the form MM.mm
		 */
        public static string GetVersion(string FullVersion)
        {
            return AppModuleInfo.GetVersion(FullVersion);
        }

        /**
          * \brief Expose a program version in the form MM.mm [rr.bb]
          */
        public static string GetLongVersion(string FullVersion)
        {
            return AppModuleInfo.GetLongVersion(FullVersion);
        }

        internal static int[] FullVersionToNumeric(string FullVersion)
        {
            string[] explode = FullVersion.Split('.');
            int[] result = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (explode.Length > i)
                    int.TryParse(explode[i], out result[i]);
            }
            return result;
        }

        /**
          * \brief Result of CompareVersions
          */
        public enum VersionDelta : int
        {
            MajorLower = -4,
            MinorLower = -3,
            BuildLower = -2,
            RevisionLower = -1,
            Equals = 0,
            RevisionGreater = 1,
            BuildGreater = 2,
            MinorGreater = 3,
            MajorGreater = 4
        }

        /**
          * \brief Compare two versions
          */
        public static VersionDelta CompareVersions(string FullVersionA, string FullVersionB)
        {
            int[] versionA = FullVersionToNumeric(FullVersionA);
            int[] versionB = FullVersionToNumeric(FullVersionB);

            if (versionB[0] > versionA[0]) return VersionDelta.MajorGreater;
            if (versionB[0] < versionA[0]) return VersionDelta.MajorLower;

            if (versionB[1] > versionA[1]) return VersionDelta.MinorGreater;
            if (versionB[1] < versionA[1]) return VersionDelta.MinorLower;

            if (versionB[2] > versionA[2]) return VersionDelta.RevisionGreater;
            if (versionB[2] < versionA[2]) return VersionDelta.RevisionLower;

            if (versionB[3] > versionA[3]) return VersionDelta.BuildGreater;
            if (versionB[3] < versionA[3]) return VersionDelta.BuildLower;

            return VersionDelta.Equals;
        }

        /**
          * \brief Compare two versions - Including wildcards
          */
        public static bool VersionsEqual(string FullVersionA, string FullVersionB)
        {
            FullVersionA = FullVersionA + ".*.*.*.*";
            FullVersionB = FullVersionB + ".*.*.*.*";
            string[] e_a = FullVersionA.Split('.');
            string[] e_b = FullVersionB.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if (e_a[i] == "*") break;
                int.TryParse(e_a[i], out int n_a);
                if (e_b[i] == "*") break;
                int.TryParse(e_b[i], out int n_b);
                if (n_a != n_b) return false;
            }

            return true;
        }

        /**
          * \brief Compare two versions - Including wildcards
          */
        public static bool VersionIsOlder(string FullVersionA, string FullVersionB)
        {
            FullVersionA = FullVersionA + ".*.*.*.*";
            FullVersionB = FullVersionB + ".*.*.*.*";
            string[] e_a = FullVersionA.Split('.');
            string[] e_b = FullVersionB.Split('.');
            for (int i = 0; i < 4; i++)
            {
                if (e_a[i] == "*") break;
                int.TryParse(e_a[i], out int n_a);
                if (e_b[i] == "*") break;
                int.TryParse(e_b[i], out int n_b);
                if (n_a > n_b) return false;
            }

            return true;
        }
    }
}