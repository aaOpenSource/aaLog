/*
 * Created by SharpDevelop.
 * User: administrator
 * Date: 4/19/2014
 * Time: 8:30 AM
 * 
 * To change this template use Tools | aaLogReaderOptionsStruct | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;



using aaLogReader;
//using svcSMCLogs.DataAccess;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
//using System.Messaging;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
//using System.ServiceProcess;
using System.Timers;
using log4net;

using Newtonsoft.Json;
using System.Net.Sockets;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaLogForwarder
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{

        // First things first, setup logging 
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}


 
        private void addlog(string Message)
        {
            try
            { 
                //txtLog.AppendText(Environment.NewLine + System.DateTime.Now.ToString() + "     " + Message);
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

        private void button2_Click(object sender, EventArgs e)
        {
            addlog("Stop");
        }

        private void dostuff()
        {

            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader();
            txtLog.Text = "";
            try
            {

                //aaLogReader.LogRecord lastRecordRead = new LogRecord();

                Stopwatch sw = new Stopwatch();

                //int RecordsToRead = 100000;

                sw.Reset();
                sw.Start();
                //List<LogRecord> records = logReader.GetRecordsInternal();
                sw.Stop();

                //addlog("Timer(ms) " + sw.ElapsedMilliseconds.ToString());
                //addlog("Actual Records " + records.Count.ToString());
                //addlog("Rate (records/s): " + ((int)(1000.0 * (float)records.Count / (float)sw.ElapsedMilliseconds)).ToString());
                //addlog("");
                //addlog(JsonConvert.SerializeObject(records, Formatting.Indented));
                ////lastRecordRead = logReader.ReadStatusCacheFile();
                //addlog(JsonConvert.SerializeObject(lastRecordRead, Formatting.Indented));
                //addlog("");
                //addlog(JsonConvert.SerializeObject(logReader.ReadLogHeader(), Formatting.Indented));
                ////Debug.Print(JsonConvert.SerializeObject(lastRecordRead,Formatting.Indented));
               
                TcpClient vSocket = new System.Net.Sockets.TcpClient("localhost", 14500);
                System.Net.Sockets.NetworkStream ServerStream = vSocket.GetStream();
                System.IO.StreamWriter swriter = new StreamWriter(ServerStream);

                List<LogRecord> records = logReader.GetUnreadRecords();

                addlog(System.DateTime.Now.ToString() + " " + records.Count.ToString());
                foreach(LogRecord lr in records)
                {
                    swriter.WriteLine(lr.ToKVP());
                }

                //swriter.Close();
                //ServerStream.Close();
                vSocket.Close();

            }
            catch (Exception ex)
            {
                addlog("***********");
                addlog(ex.ToString());
                addlog("***********");
                log.Error(ex);
            }
            finally
            {
                logReader.CloseCurrentLogFile();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            

            timer1.Interval = 1000;
            timer1.Start();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dostuff();
        }

	}
}
