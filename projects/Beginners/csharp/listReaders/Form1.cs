using System;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace listReaders
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            refreshReaders();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refreshReaders();
        }

        private void refreshReaders()
        {
            listBox1.Items.Clear();
            try
            {
                string[] readers = SCARD.Readers;
                foreach (string reader in readers)
                    listBox1.Items.Add(reader);
            }
            catch (Exception)
            {
                MessageBox.Show("There was a problem while searching for the list of readers, may be there's no reader?");
            }
        }
    }
}
