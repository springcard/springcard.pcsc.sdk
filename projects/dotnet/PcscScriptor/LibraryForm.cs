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
		private string selectedItem = null;
		private Dictionary<string, string> itemsInList = null;

		public LibraryForm()
		{
			InitializeComponent();
		}

		private void SetSelectedItem()
		{
			if (listBoxApdus.SelectedIndex == -1)
				return;
			string selectedItemText = listBoxApdus.Items[listBoxApdus.SelectedIndex].ToString();
			this.selectedItem = itemsInList[selectedItemText];
		}

		public string GetSelectedItem()
		{
			return this.selectedItem;
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			if(listBoxApdus.SelectedIndex == -1)
			{
				MessageBox.Show("Error, please select an item");
				return;
			}
			SetSelectedItem();
			DialogResult = DialogResult.OK;
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void LibraryForm_Load(object sender, EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			this.itemsInList = new Dictionary<string, string>();
			RestClient rest = new RestClient();
			var response = rest.GET_Json("http://models.springcard.com/api/models/");
			foreach (JSONObject item in response.ArrayValue)
			{
				string mode = item["mode"].IntValue == 0 ? "Transmit" : "Control";
				string listItem = item["id"].IntValue + " - " + item["title"].StringValue + " - " + "Mode: " + mode;
				itemsInList.Add(listItem, item["apdu"].StringValue);
				listBoxApdus.Items.Add(listItem);
			}
			this.Cursor = Cursors.Default;
			listBoxApdus.Focus();
		}

		private void listBoxApdus_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetSelectedItem();
		}
	}
}
