/*
 * Author : Hervé Thouzard
 * Date: 24/10/2014
 */
using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace SpringCard.LibCs
{
	/// <summary>
	/// This class can be used in projects where internationalization is used and projects where it's not used
	///
	/// How to use it:
	///
	/// 1) Your class must inherit from this one:
	///		public class MyClass : Translatable
	///
	/// In your code, replace your hard coded texts with a call to GetTranslation(), for example:
	///		...
	///		throw new Exception(GetTranslation("NumberMustBeGreater", "Message d'erreur par défaut"));
	///		...
	///	The first parameter is the translation's key (in your .resx file), the second one a default text to use
	///	There is a third parameter thzt you can use to specify the name of your .resx file
	///
	///	If your class alreay inherit from a class, just copy the (unique) GetTranslation() method from this class to your own
	///	And don't forget to add the usings
	/// </summary>
	public static class Translatable
	{
		public static string GetTranslation(string MessageKey, string TranslationDefaultValue = "", string TranslationFile = "Translations")
		{
			string baseName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + TranslationFile;	// Assemblyname.Translations
			string translation = TranslationDefaultValue;	// Mise en place d'une valeur par défaut

			ResourceManager rm = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
			try
			{
				string projectTranslation = rm.GetString(MessageKey, Thread.CurrentThread.CurrentCulture);
				if (projectTranslation != null)
					translation = projectTranslation;
			}
			catch (Exception)
			{
			}
			return translation;
		}
	}
}
