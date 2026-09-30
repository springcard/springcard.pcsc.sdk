using System;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;


namespace SpringCardApplication
{
	public class ProcessAction
	{
		public override string ToString() { return Name == null ? "unnamed" : Name; }
		public string Name;
		public bool Enabled = false;
		public string MatchString;
		public System.Diagnostics.ProcessPriorityClass PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
	}

	public class PriorityManagerSettings
	{
		public bool Enabled;
		public bool ToastEnabled;
		public ProcessAction [] ProcessActions;
	}
	
	public class SettingsManager
	{
		const string SettingsFileName = "PriorityManagerSettings.xml";

		public static void WriteSettings( bool enabled , bool toast , ArrayList processActions )
		{
			PriorityManagerSettings p = new PriorityManagerSettings();
			p.Enabled = enabled;
			p.ToastEnabled = toast;

			lock( processActions.SyncRoot )
			{
				p.ProcessActions = (ProcessAction [])processActions.ToArray( typeof( ProcessAction ) );
			}

			XmlSerializer serialiser = new XmlSerializer( typeof(PriorityManagerSettings) );
			IsolatedStorageFileStream fileStream  = new IsolatedStorageFileStream( SettingsFileName , FileMode.Create , FileAccess.Write , FileShare.None );
			System.IO.StreamWriter writer = new System.IO.StreamWriter( fileStream );
			serialiser.Serialize( writer , p );
			writer.Close();
		}

		public static void ReadSettings( out bool enabled , out bool toast , ArrayList processActions )
		{
			PriorityManagerSettings p;

			try
			{
				XmlSerializer deSerialiser = new XmlSerializer( typeof(PriorityManagerSettings) );
				IsolatedStorageFileStream fileStream  = new IsolatedStorageFileStream( SettingsFileName , FileMode.Open , FileAccess.Read , FileShare.None );
				System.IO.StreamReader reader = new System.IO.StreamReader( fileStream );
				p = (PriorityManagerSettings)deSerialiser.Deserialize( reader );
				enabled	= p.Enabled ;
				toast		= p.ToastEnabled;
				
				lock ( processActions.SyncRoot )
				{
					processActions.Clear();
					processActions.AddRange( p.ProcessActions );
				}
				
				reader.Close();
			}
			catch
			{
				// hard coded defaults...
				toast			= true;
				enabled			= true;
				processActions	= new ArrayList();
			}
		}
	}
}
