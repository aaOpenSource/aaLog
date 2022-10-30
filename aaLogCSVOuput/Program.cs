using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using aaLogReader;
using System.IO;
using Newtonsoft.Json;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaLogCSVOuput
{
    class Program
    {
        #region globals 
            private static aaLogReader.aaLogReader logReader;
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        static void Main(string[] args)
        {

            log4net.Config.BasicConfigurator.Configure();
            //aaLogReader.aaLogReader logReader;

            try
            {
                DateTime startTimestamp = System.DateTime.Parse(args[0]);
                DateTime endTimestamp = System.DateTime.Parse(args[1]);
                string outputfile = args[2];

                log.InfoFormat("startTimestamp: {0}", startTimestamp.ToString());
                log.InfoFormat("endTimestamp: {0}", endTimestamp.ToString());
                log.InfoFormat("outputfile: {0}", outputfile);

                log.Info("Starting Log Collection");

                aaLogReader.OptionsStruct options = new OptionsStruct();

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
                }

                options.IgnoreCacheFileOnFirstRead = true;
                logReader = new aaLogReader.aaLogReader(options);

                List<LogRecord> records = logReader.GetRecordsByStartAndEndTimeStamp(startTimestamp, endTimestamp);
                log.InfoFormat("Writing {0} records to {1}.", records.Count, outputfile);

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputfile, false))
                {
                    file.WriteLine(LogRecord.HeaderTSV());

                    foreach (LogRecord record in records)
                    {
                        file.WriteLine(record.ToCSV());
                    }
                }
                log.Info("File write complete");
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                logReader.Dispose();
            }
        }
    }
}
