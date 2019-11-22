using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Runtime.InteropServices;
using aaLogReader;
using System.IO;
using YamlDotNet.Serialization;

namespace aaLogElasticFileBeat
{
    public partial class aaLogElasticFileBeat : ServiceBase
    {
        int interval;
        string binaryLocation;
        string configLocation;

        //Taken from the tutorial on writing a Windows service
        //https://docs.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        public aaLogElasticFileBeat()
        {
            InitializeComponent();
            //Set up logging
            localEventLog = new EventLog();
            if (!EventLog.SourceExists("aaLogElasticFileBeat"))
            {
                EventLog.CreateEventSource("aaLogElasticFileBeat", "Application");
            }
            localEventLog.Source = "aaLogElasticFileBeat";
            localEventLog.Log = "Application";
        }

        //Function to create default settings file in the install folder
        //YAML is used because that is consistent with Elastic / Beats
        private void writeNewSettingsFile(string configFilePath)
        {
            Dictionary<string, string> settingsDict = new Dictionary<string, string>();
            settingsDict.Add("Interval", "60");
            settingsDict.Add("BinaryLocation", @"C:\Program Files\filebeat\filebeat.exe");
            settingsDict.Add("SettingsLocation", @"C:\Program Files\filebeat\filebeat.yml");

            Serializer s = new Serializer();
            using (StreamWriter outfile = File.CreateText(configFilePath))
            {
                s.Serialize(outfile, settingsDict);
            }
        }

        protected override void OnStart(string[] args)
        {
            //Log entry for starting
            //localEventLog.WriteEntry("Running OnStart", EventLogEntryType.Information);

            //Check if the config file exists. If not, create it.
            //Read the settings file and place settings in class variables
            string localPathURI = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            string localPath = System.IO.Path.GetDirectoryName(localPathURI.Substring(8, localPathURI.Length - 8));
            string configFilePath = localPath + @"\aaLogElasticFileBeat.yml";
            localEventLog.WriteEntry(configFilePath, EventLogEntryType.Information);
            if (!File.Exists(configFilePath))
            {
                this.writeNewSettingsFile(configFilePath);
            }

            string configFileString = File.ReadAllText(configFilePath);
            Deserializer configDeserializer = new Deserializer();
            Dictionary<string, string> settingsDict = configDeserializer.Deserialize<Dictionary<string, string>>(configFileString);

            this.interval = int.Parse(settingsDict["Interval"]) * 1000;
            this.binaryLocation = settingsDict["BinaryLocation"];
            this.configLocation = settingsDict["SettingsLocation"];

            localEventLog.WriteEntry("Settings: Interval = " + this.interval.ToString() + "; Binary location = " + 
                this.binaryLocation + "; Config file location = " + this.configLocation, EventLogEntryType.Information);

            //Set up the polling timer
            //TODO: make this interval configurable
            Timer pollingTimer = new Timer();
            pollingTimer.Interval = this.interval;
            pollingTimer.Elapsed += new ElapsedEventHandler(this.PollingAction);
            pollingTimer.Start();

            //Update the status of the service
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        protected override void OnStop()
        {
            //Log entry for stopping
            //localEventLog.WriteEntry("Running OnStop", EventLogEntryType.Information);
            
            //Update the state of the service
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        //Function run by the timer
        //Wrapper only so that the shipLogs function can be used in other places without the args
        private void PollingAction(object sender, ElapsedEventArgs args)
        {
            //localEventLog.WriteEntry("Running PollingAction", EventLogEntryType.Information);
            this.shipLogs();
        }

        //Function to read logs and send them to filebeat
        private void shipLogs()
        {
            //Set up the aaLogReader and get unread records
            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader();
            List<LogRecord> logRecords = logReader.GetUnreadRecords();

            //If there were any records to ship:
            if (logRecords.Count > 0)
            {
                //Log message
                //localEventLog.WriteEntry("Shipping entries, count:" + logRecords.Count().ToString(), EventLogEntryType.Information);
                
                //Use a Process to run the filebeat binary
                using (Process fileBeatProcess = new Process())
                {
                    //Set up the process
                    //TODO: make locations configurable
                    fileBeatProcess.StartInfo.FileName = this.binaryLocation;
                    fileBeatProcess.StartInfo.Arguments = "-c \"" + this.configLocation + "\" -e -once";
                    fileBeatProcess.StartInfo.UseShellExecute = false;

                    //Important: redirect standard input as this is how we will send the data to Filebeat
                    fileBeatProcess.StartInfo.RedirectStandardInput = true;
                    
                    //Start the process
                    fileBeatProcess.Start();

                    //Get a StreamWriter object to send data to stdin of the process
                    StreamWriter fileBeatInputStream = fileBeatProcess.StandardInput;

                    //Ship each record
                    foreach (LogRecord record in logRecords)
                    {
                        //Ship to stdin as json, which can be decoded by filebeat
                        fileBeatInputStream.WriteLine(record.ToJSON());
                    }

                    //Close the stream; this should cause Filebeat to exit since we used -once
                    fileBeatInputStream.Close();
                    fileBeatProcess.WaitForExit();

                    //Log the end of the file
                    //localEventLog.WriteEntry("Finished shipping entries", EventLogEntryType.Information);
                }
            }
        }

        //Taken from the tutorial on writing a Windows service
        //https://docs.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
