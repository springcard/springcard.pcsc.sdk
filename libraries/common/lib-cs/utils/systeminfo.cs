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

namespace SpringCard.LibCs
{
	/**
	 * \brief Utility to retrieve most information regarding the operating system
	 */
	public class SystemInfo
	{
		/**
		 * \brief Microsoft's .NET, or Mono?
		 */
		public enum DotNetEnvironment
		{
			Unknown,
			MS_Net,
			Mono
		}

		/**
		 * \brief Which operating system family?
		 */
		public enum RuntimeSystem
		{
			Unknown,
			Windows,
			Unix,
			MacOSX
		}

		/**
		 * \brief Which environment has the program been built on?
		 */
		public static DotNetEnvironment GetBuildEnvironment()
		{
#if __MonoCS__
			return BuildEnvironment.Mono;
#else
      		return DotNetEnvironment.MS_Net;
#endif
		}

		private static DotNetEnvironment _RuntimeEnvironment = DotNetEnvironment.Unknown;

		/**
		 * \brief Which environment is the program currently running on?
		 */
		public static DotNetEnvironment GetRuntimeEnvironment()
		{
			if (_RuntimeEnvironment == DotNetEnvironment.Unknown)
			{
				if (Type.GetType("Mono.Runtime") != null)
				{
					_RuntimeEnvironment = DotNetEnvironment.Mono;
				}
				else
				{
					_RuntimeEnvironment = DotNetEnvironment.MS_Net;
				}
			}
			
			return _RuntimeEnvironment;
		}

		private static RuntimeSystem _RuntimeSystem = RuntimeSystem.Unknown;

		/**
		 * \brief Which operating system is the program currently running on?
		 */
		public static RuntimeSystem GetRuntimeSystem()
		{
			if (_RuntimeSystem == RuntimeSystem.Unknown)
			{
				int i = (int) Environment.OSVersion.Platform;
				switch (i)
				{
					case 4 :
					case 128 :
						_RuntimeSystem = RuntimeSystem.Unix;
						break;
					case 6 :
						_RuntimeSystem = RuntimeSystem.MacOSX;
						break;
					default :
						_RuntimeSystem = RuntimeSystem.Windows;
						break;
				}
			}
			
			return _RuntimeSystem;
		}		
	}
}