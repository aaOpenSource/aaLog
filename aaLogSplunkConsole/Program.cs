using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aaLogReader;
using Newtonsoft.Json;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaLogSplunkConsole
{
    class Program
    {
        #region globals 
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        static void Main(string[] args)
        {
            try
            {
                ulong messageCount = 0;
                aaLogReader.OptionsStruct options = new OptionsStruct();
                string aaLGXDirectory = "";

                log.Info("Starting Log Collection");

                // Read the message count as the single argument to the exe
                try
                {
                    messageCount = System.Convert.ToUInt32(args[0]);
                }
                catch (Exception)
                {
                    //Ignore the error and use default value of 1000
                    log.InfoFormat("No maximum message count specified.  Include a single integer argument when calling the application to set the maximum message count retrieved");
                    messageCount = 1000;
                }

                // Final check to make sure we don't have a null messageCount
                if (messageCount <= 0) { messageCount = 1000; }

                
                try
                {
                    // Read the aaLGX directory command line switch
                    if (args.Length >= 2)
                    {
                        aaLGXDirectory = args[1] ?? "";
                    }

                    if (aaLGXDirectory != "")
                    {
                        log.InfoFormat("Reading aaLGX files from {0}", aaLGXDirectory);

                        if (Directory.Exists(aaLGXDirectory))
                        {

                            string[] filesList = Directory.GetFiles(aaLGXDirectory, "*.aalgx");

                            foreach (string fileName in filesList)
                            {
                                log.InfoFormat("Processing file {0}", fileName);
                                var aaLGXRecords = aaLogReader.aaLgxReader.ReadLogRecords(fileName);

                                log.InfoFormat("Found {0} records in {1}", aaLGXRecords.Count(), fileName);

                                foreach (LogRecord record in aaLGXRecords)
                                {
                                    Console.WriteLine(record.ToKVP());
                                }

                                log.InfoFormat("Deleting {0} after reading records.", fileName);
                                File.Delete(fileName);
                            }
                        }
                        else
                        {
                            log.WarnFormat("aaLGX Directory {0} does not exist.", aaLGXDirectory);
                        }
                    }
                }
                catch(Exception ex)
                {
                    log.Error(ex);
                }

                //Read the options from a local file
                try
                {
                    if (System.IO.File.Exists("options.json"))
                    {
                        log.DebugFormat("Reading Options from local options.json");
                        options = JsonConvert.DeserializeObject<OptionsStruct>(System.IO.File.ReadAllText("options.json"));
                    }
                }
                catch
                {
                    log.InfoFormat("No options.json file exists.  Using default options");
                    options = new OptionsStruct();
                }

                log.InfoFormat("Reading records with maximum message count of {0}", messageCount);

                aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(options);
                List<LogRecord> logRecords = logReader.GetUnreadRecords(maximumMessages: messageCount);

                log.InfoFormat("{0} unread log records found", logRecords.Count);

                // If we have any records then output kvp format to the console
                if (logRecords.Count > 0)
                {
                    foreach (LogRecord record in logRecords)
                    {
                        Console.WriteLine(record.ToKVP());
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
