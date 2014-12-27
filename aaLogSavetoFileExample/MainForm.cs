using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using aaLogReader;

namespace aaLogSavetoFileExample
{
    public partial class MainForm : Form
    {

        aaLogReader.aaLogReader logReader;

        public MainForm()
        {
            InitializeComponent();
        }

        private void addlog(string Message)
        {
            try
            {                
                txtLog.AppendText(Environment.NewLine + Message);
            }
            catch (Exception ex)
            {
                // Do Nothing
                if (ex.Message == "Cross-thread operation not valid: Control 'txtLog' accessed from a thread other than the thread it was created on.")
                {
                    // Eat the exception;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            logReader = new aaLogReader.aaLogReader();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(txtFileName.Text, true))
            {
                file.WriteLine(LogRecord.HeaderTSV());
            }

            tmrTimer1.Interval = 1000;
            tmrTimer1.Start();
        }

        private void Run()
        {
            List<LogRecord> records = logReader.GetUnreadRecords();

            addlog(System.DateTime.Now.ToString() + " " + records.Count.ToString());

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(txtFileName.Text, true))
            {
                foreach (LogRecord lr in records)
                {
                        file.WriteLine(lr.ToTSV());
                }
            }                
        }
        
        private void tmrTimer1_Tick(object sender, EventArgs e)
        {
            this.Run();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            tmrTimer1.Stop();
        }
    }
}
