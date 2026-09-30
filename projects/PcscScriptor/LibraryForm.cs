using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.LibCs;

namespace scscriptorxv
{
	public partial class LibraryForm : Form
	{
		private List<JSONObject> items = new List<JSONObject>();
		private string SelectedApdu = null;
		private Dictionary<int, string> ApduList = null;

		public LibraryForm()
		{
			InitializeComponent();
		}

		private void SetSelectedItem(int id)
		{
			SelectedApdu = ApduList[id];
			eApdu.Text = SelectedApdu;
		}

		public string GetSelectedItem()
		{
			return this.SelectedApdu;
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void LibraryForm_Load(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			ApduList = new Dictionary<int, string>();
			LoadGroups();
			LoadModels();
			this.Cursor = Cursors.Default;
			lvApdus.Focus();
		}

		private void LoadGroups()
        {
            {
				/* Override default group */
				ListViewGroup group = new ListViewGroup("#", "NFC/RFID commands");
				lvApdus.Groups.Add(group);
			}

			try
			{
				RestClient rest = new RestClient();
				var response = rest.GET_Json("https://models.springcard.com/api/groups/");
				foreach (JSONObject item in response.ArrayValue)
				{
					int id = item["id"].IntValue;
					string title = item["title"].StringValue;

					Logger.Trace("Adding group '{0}'", title);
					ListViewGroup group = new ListViewGroup(id.ToString(), title);
					lvApdus.Groups.Add(group);
				}
			}
			catch (Exception e)
			{
				Logger.Warning(e.Message);
			}
		}

		private void LoadModels()
        {
			try
			{
				RestClient rest = new RestClient();
				var response = rest.GET_Json("https://models.springcard.com/api/models/");

				var tmpItemList = new Dictionary<int, Tuple<string, string>>();

				foreach (JSONObject entry in response.ArrayValue)
				{
					int id = entry["id"].IntValue;
					int group_id = entry["group_id"].IntValue;
					string title = entry["title"].StringValue;
					string mode = entry["mode"].IntValue == 0 ? "Transmit" : "Control";
					string apdu = entry["apdu"].StringValue;

					if (entry["mode"].IntValue != 0)
						continue; /* This is a control not a transmit */

					Logger.Trace("{0}: {1}", id, apdu);

						string listItem = entry["id"].IntValue + " - " + entry["title"].StringValue + " - " + "Mode: " + mode;
					tmpItemList.Add(entry["id"].IntValue, Tuple.Create(listItem, entry["apdu"].StringValue));

					ListViewItem item = new ListViewItem(title);
					item.Tag = id;
					ListViewGroup group = lvApdus.Groups[group_id.ToString()];
					if (group == null)
						group = lvApdus.Groups["#"];
					if (group != null)
						item.Group = group;
					lvApdus.Items.Add(item);

					ApduList.Add(id, apdu);
				}
			}
			catch (Exception e)
			{
				Logger.Warning(e.Message);
			}
		}

        private void lvApdus_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (lvApdus.SelectedItems.Count == 1)
				SetSelectedItem((int)lvApdus.SelectedItems[0].Tag);
		}
    }
}
