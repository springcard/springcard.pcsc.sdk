using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;
using System.Resources;
using System.Threading;

namespace SpringCard.LibCs
{
    public static class T
    {
        public static string _(string text)
        {
            return Translation.T(text);
        }
    }

	public static class Translation
	{
		private static Object locker = new Object();
		private static CultureInfo defaultCulture;		
		private static Dictionary<string, ResourceManager> resourceManagers = new Dictionary<string, ResourceManager>();
		private static string defaultContext = "Strings";
		private static bool fallbackAllContexts = false;
		
		static Translation()
		{
			LoadContext(defaultContext);
		}
       
		private static string GetSetting(string setting, string defaultValue)
		{
			var section = (System.Collections.Specialized.NameValueCollection)System.Configuration.ConfigurationManager.GetSection("appSettings");
			if (section == null)
				return defaultValue;
			else
				return section[setting] ?? defaultValue;
		}


        /// <summary>
        /// Resources directory used to retrieve files from.
        /// </summary>
        public static string ResourcesDirectory = GetSetting("ResourcesDir", "po");

        /// <summary>
        /// Format of the file based on culture and resource name.
        /// </summary>
        public static string FileFormat = GetSetting("ResourcesFileFormat", "{{culture}}/{{resource}}.po");


        /// <summary>
        /// Returns the cached ResourceManager instance used by this class.
        /// </summary>
        public static void LoadContext(string context)
        {
			lock (locker)
            {
				if (resourceManagers.ContainsKey(context))
					return;			
				try
				{
					var manager = new global::Gettext.Cs.GettextResourceManager(context, ResourcesDirectory, FileFormat);
					Logger.Debug("Loading translations '{0}' from {1}", context, manager.Path);
					resourceManagers[context] = manager;
				}
                catch 
				{
					Logger.Debug("No translation for context '{0}'", context);
					resourceManagers[context] = null;
				}
			}
			if (context.Contains("."))
			{
				context = Path.GetFileNameWithoutExtension(context);
				if (!string.IsNullOrEmpty(context))
					LoadContext(context);
			}
		}

		/// <summary>
		/// Overrides the current thread's CurrentUICulture property for all
		/// resource lookups using this strongly typed resource class.
		/// </summary>
		public static System.Globalization.CultureInfo Culture {
			get
			{
				return defaultCulture;
			}
			set
			{
				defaultCulture = value;
				Logger.Debug("Culture set to {0}", defaultCulture.ToString());
			}
		}
		
		public static string T(string context, string text, CultureInfo culture)
		{
			return Translate(context, text, culture);
		}

		public static string T(string context, string text, string lang)
		{
			return Translate(context, text, null);
		}
		
		public static string T(string text, CultureInfo culture)
		{
			return Translate(Assembly.GetCallingAssembly().GetName().Name, text, culture);
		}

		public static string T(string text, string lang)
		{
			return Translate(Assembly.GetCallingAssembly().GetName().Name, text, null);
		}

		public static string T(string text)
		{
			return Translate(Assembly.GetCallingAssembly().GetName().Name, text, null);
		}
		
		public static string Translate(string context, string text, CultureInfo culture)
		{
			if (String.IsNullOrEmpty(text))
				return text;
			if (culture == null)
				culture = defaultCulture;
			
			if (context != null)
			{
				if (!resourceManagers.ContainsKey(context))
					LoadContext(context);
				if (resourceManagers[context] != null)
				{
                    try
                    {
                        string translated = resourceManagers[context].GetString(text, culture);
                        if (!String.IsNullOrEmpty(translated))
                            return translated;
                    }
                    catch { }
				}
			}
			
			if (fallbackAllContexts)
			{
				foreach (KeyValuePair<string, ResourceManager> entry in resourceManagers)
				{
                    try
                    {
                        string translated = entry.Value.GetString(text, culture);
                        if (!String.IsNullOrEmpty(translated))
                            return translated;
                    }
                    catch { }
				}
			}
            else
			{
                try
                {
                    string translated = resourceManagers[defaultContext].GetString(text, culture);
                    if (!String.IsNullOrEmpty(translated))
                        return translated;
                }
                catch { }
			}
			
			if (culture != null)
				Logger.Debug("Missing translation[{0}] in {1}:{2}", culture.ToString(), context, text);
			return text;
		}

		/// <summary>
		/// Looks up a localized string and formats it with the parameters provided; used to mark string for translation as well.
		/// </summary>
		public static string T(string t, params object[] parameters)
		{
			return T((CultureInfo) null, t, parameters);
		}

		/// <summary>
		/// Looks up a localized string and formats it with the parameters provided; used to mark string for translation as well.
		/// </summary>
		public static string T(CultureInfo info, string t, params object[] parameters)
		{
			if (String.IsNullOrEmpty(t))
				return t;
			return String.Format(T(info, t), parameters);
		}

		/// <summary>
		/// Marks a string for future translation, does not translate it now.
		/// </summary>
		public static string M(string t)
		{
			Logger.Debug("M({0})", t);
			return t;
		}
        
		/// <summary>
		/// Returns the resource set available for the specified culture.
		/// </summary>
		//public static System.Resources.ResourceSet GetResourceSet(CultureInfo culture)
		//{
		//	return ResourceManager.GetResourceSet(culture, true, true);
		//}
		
		private static string Lang()
		{
			return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
		}
		
		
		public static string TK(string key, string defaultMessage = null, string translationFile = "Translations")
		{
			string lang = Lang();		
			if (defaultMessage == null)
				defaultMessage = key;
			string message = defaultMessage;			

			string baseName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + translationFile;	// Assemblyname.Translations			
			try
            {
				ResourceManager rm = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
				string projectTranslation = rm.GetString(key, Thread.CurrentThread.CurrentCulture);
				if (projectTranslation == null)
                {
					Logger.Debug("TK:{0}({1}) not found in {2}", lang, key, baseName);
				}
                else
                {
					message = projectTranslation;
				}
			}
            catch (Exception e)
            {
				Logger.Debug("TK:{0}({1}) not found in {2} ({3})", lang, key, baseName, e.Message);
			}
			
			return message;
		}
	}
}
