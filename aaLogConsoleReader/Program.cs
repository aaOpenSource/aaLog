using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aaLogReader;
using Newtonsoft.Json;
//using ArchestrA.Logging;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaLogConsoleTester
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

            string answer;

            aaLogReader.aaLogReaderOptions testOptions = JsonConvert.DeserializeObject<aaLogReaderOptions>(System.IO.File.ReadAllText("options.json"));
            testOptions.IgnoreCacheFileOnFirstRead = true;
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "Message", Filter = "Warning 40|Message 41" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "MessageNumberMin", Filter = "6826080" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "MessageNumberMax", Filter = "6826085" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "DateTimeMin", Filter = "2015-06-19 01:45:00" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "DateTimeMax", Filter = "2015-06-19 01:45:05" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "ProcessID", Filter = "7260" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "ThreadID", Filter = "7264" });
            
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "Message", Filter = "Started" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter() { Field = "HostFQDN", Filter = "865" });


            answer = "y";

            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(testOptions);

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilter(){Field="LogFlag",Filter="Warning"});
            
            while (answer.ToLower() == "y")
            {
                Console.WriteLine("Read Unread Records (Y=Yes, N=Exit)");
                answer = Console.ReadLine();

                if (answer.ToLower() == "y")
                {

                    List<LogRecord> records = logReader.GetUnreadRecords(100);

                    Console.WriteLine("Record count : " + records.Count.ToString());

                    foreach (LogRecord lr in records)
                    {
                        string writeMsg = (lr.MessageNumber.ToString() + '\t' + lr.EventFileTimeUTC.ToString()  + '\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
                        log.Info(writeMsg);
                        Console.WriteLine(writeMsg);
                        //Console.WriteLine(lr.ToKVP());
                    }
                }
            }
        }
    }
}
