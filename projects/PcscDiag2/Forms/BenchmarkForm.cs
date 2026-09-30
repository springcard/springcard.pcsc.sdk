using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcscDiag2
{
    public partial class Benchmark : Form
    {
        SCardChannel channel;
        CAPDU capdu;
        Thread thread;
        volatile bool running;

        public Benchmark(SCardChannel channel, CAPDU capdu)
        {
            this.channel = channel;
            this.capdu = capdu;
            InitializeComponent();
        }

        private void Proc()
        {
            int bytesSent = 0;
            int bytesReceived = 0;

            int count = (int) eCount.Value;

            int index;
            DateTime start = DateTime.Now;
            for (index=0; index<count; index++)
            {
                if (!running)
                    break;

                if (index % 100 == 0)
                {
                    Progress(index, count, (int) DateTime.Now.Subtract(start).TotalMilliseconds, bytesSent, bytesReceived);
                }

                RAPDU rapdu = channel.Transmit(capdu);

                if (rapdu == null)
                {
                    running = false;
                    break;
                }

                bytesSent += capdu.Length;
                bytesReceived += rapdu.Length;
            }

            Progress(index, count, (int) DateTime.Now.Subtract(start).TotalMilliseconds, bytesSent, bytesReceived);
            Done();
        }

        delegate void ProgressInvoker(int index, int count, int timeElapsed, int bytesSent, int bytesReceived);
        private void Progress(int index, int count, int timeElapsed, int bytesSent, int bytesReceived)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ProgressInvoker(Progress), index, count, timeElapsed, bytesSent, bytesReceived);
                return;
            }

            eProgress.Text = string.Format("{0}", index);
            eTimeElapsed.Text = string.Format("{0}", timeElapsed);
            if (timeElapsed > 0)
            {
                eBenchmark.Text = string.Format("{0}", (1000 * index) / timeElapsed);
                ePcToCard.Text = string.Format("{0}", (1000 * bytesSent) / timeElapsed);
                eCardToPc.Text = string.Format("{0}", (1000 * bytesReceived) / timeElapsed);
            }
        }

        delegate void DoneInvoker();
        private void Done()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DoneInvoker(Done));
                return;
            }

            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnClose.Enabled = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnClose.Enabled = false;
            btnStop.Enabled = true;

            eProgress.Text = "";
            eBenchmark.Text = "";
            ePcToCard.Text = "";
            eCardToPc.Text = "";

            running = true;
            thread = new Thread(Proc);
            thread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            running = false;
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnClose.Enabled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
