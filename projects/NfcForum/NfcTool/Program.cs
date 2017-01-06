using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using System.Threading;

namespace SpringCardApplication
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// 
    [STAThread]
    static void Main()
    {
			Mutex AccessReaderMutex = new Mutex(true, "AccessReaderMutex");
			AccessReaderMutex.WaitOne(0);
			Application.Run(new MainForm());
			AccessReaderMutex.ReleaseMutex();
			AccessReaderMutex.Close();
    }

  }
  
}