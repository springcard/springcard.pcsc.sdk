/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:02
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Xml;
using System.Text;

namespace SpringCard.PCSC
{
	/// <summary>
	/// Description of PiccEmulDesfireEV0.
	/// </summary>
	public class CardEmul7816 : CardEmulBase
	{
		public CardEmul7816()
		{
			OnCardDeselect();
		}
		
		protected override void OnCardSelect()
		{

		}
		
		protected override void OnCardDeselect()
		{

		}
		
		protected override RAPDU OnApdu(CAPDU capdu)
		{
			System.Threading.Thread.Sleep(100);
			return new RAPDU(new byte[1], 0x90, 0x00);
		}
	}
}
