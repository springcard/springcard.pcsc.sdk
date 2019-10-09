/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:02
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardEmulation
{
	/// <summary>
	/// Description of PiccEmulDesfireEV0.
	/// </summary>
	public class CardEmulLoopback : CardEmulBase
	{
		public CardEmulLoopback(string ReaderName) : base(ReaderName)
		{

		}
		
		protected override void OnSelect()
		{
            Logger.Info("Loopback:Select");
        }
		
		protected override void OnDeselect()
		{
            Logger.Info("Loopback:Deselect");
        }

        protected override void OnError()
        {
            Logger.Info("Loopback:Error");
        }

        protected override RAPDU OnApdu(CAPDU capdu)
		{
            Logger.Info("Loopback:APDU");
            System.Threading.Thread.Sleep(100);
			return new RAPDU(new byte[1], 0x90, 0x00);
		}
	}
}
