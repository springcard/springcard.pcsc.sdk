using System;

namespace SpringCard.LibCs
{
	public class SystemInfo
	{
		public enum BuildEnvironment
		{
			Unknown,
			MS_Net,
			Mono
		}
		
		public enum RuntimeEnvironment
		{
			Unknown,
			MS_Net,
			Mono
		}

		public enum RuntimeSystem
		{
			Unknown,
			Windows,
			Unix,
			MacOSX
		}
		
		public static BuildEnvironment GetBuildEnvironment()
		{
#if __MonoCS__
			return BuildEnvironment.Mono;
#else
      		return BuildEnvironment.MS_Net;
#endif
		}
		
		private static RuntimeEnvironment _RuntimeEnvironment = RuntimeEnvironment.Unknown;		
		public static RuntimeEnvironment GetRuntimeEnvironment()
		{
			if (_RuntimeEnvironment == RuntimeEnvironment.Unknown)
			{
				if (Type.GetType("Mono.Runtime") != null)
				{
					_RuntimeEnvironment = RuntimeEnvironment.Mono;
				}
				else
				{
					_RuntimeEnvironment = RuntimeEnvironment.MS_Net;
				}
			}
			
			return _RuntimeEnvironment;
		}

		private static RuntimeSystem _RuntimeSystem = RuntimeSystem.Unknown;		
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