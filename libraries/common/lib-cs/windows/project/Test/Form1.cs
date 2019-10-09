using SpringCard.LibCs.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAboutClassic_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this);
        }

        private void btnAboutModern_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.Modern);
        }

        private void btnAboutRed_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernRed);
        }

        private void btnAboutMarroon_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernMarroon);
        }
    }
}
