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

            try
            {

            string answer;
                long totalTime;
                long totalRecords;

            aaLogReader.OptionsStruct testOptions = JsonConvert.DeserializeObject<OptionsStruct>(System.IO.File.ReadAllText("options.json"));
            testOptions.IgnoreCacheFileOnFirstRead = true;

                #region Filter Section

                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "Message", Filter = "Warning 40|Message 41" });

                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "MessageNumberMin", Filter = "6826080" });
                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "MessageNumberMax", Filter = "6826085" });

                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "DateTimeMin", Filter = "2015-06-19 01:45:00" });
                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "DateTimeMax", Filter = "2015-06-19 01:45:05" });

                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "ProcessID", Filter = "7260" });
                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "ThreadID", Filter = "7264" });

                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "Message", Filter = "Started" });
                //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "HostFQDN", Filter = "865" });
                #endregion

                answer = "y";

                totalTime = 0;
                totalRecords = 0;

                for (int x = 1; x <= 50; x++)
                {
                    aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(testOptions);

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    List<LogRecord> records = new List<LogRecord>();

                    sw.Reset();
                    sw.Start();
                    records = logReader.GetUnreadRecords(100000);

                    ////log.Info("Back");
                    ////sw.Start();
                    ////records = logReader.GetRecordsByStartMessageNumberAndCount(startmsg, count, SearchDirection.Back);

                    //sw.Start();
                    ////records = logReader.GetRecordsByStartandEndMessageNumber(startmsg, endmsg, count);                
                    ////records = logReader.GetRecordsByEndMessageNumberAndCount(18064517, 10);
                    ////records = logReader.GetRecordsByStartMessageNumberAndCount(8064512, 30);

                    ////record = logReader.GetRecordByTimestamp(DateTime.Parse("2015-06-27 13:42:33"));
                    ////records.Add(record);
                    ////record = logReader.GetRecordByTimestamp(DateTime.Parse("2015-06-27 13:42:33"),EarliestOrLatest.Latest);
                    ////records.Add(record);

                    ////writelogs(logReader.GetRecordsByEndTimestampAndCount(DateTime.Parse("2015-06-27 13:42:33"),10));
                    ////writelogs(logReader.GetRecordsByStartTimestampAndCount(DateTime.Parse("2015-06-27 13:42:33"), 10));

                    sw.Stop();

                    //log.InfoFormat("Found {0} messages", records.Count);
                    Console.WriteLine(records.Count + " records");

                    //log.InfoFormat("Time - {0} millseconds", sw.ElapsedMilliseconds);
                    Console.WriteLine(sw.ElapsedMilliseconds + " milliseconds");

                    //log.InfoFormat("Rate - {0} records/second", records.Count / sw.Elapsed.TotalSeconds);

                    Console.WriteLine("Rate - " + records.Count / ((float)sw.ElapsedMilliseconds/1000) + " records/second");

                    totalRecords += records.Count;
                    totalTime += sw.ElapsedMilliseconds;

                    //writelogs(records);

                    //log.Info(JsonConvert.SerializeObject(records));

                    //sw.Stop();
                    //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                    //log.InfoFormat("Count - {0}", records.Count);
                    logReader.Dispose();
                    records = null;
                    sw = null;


                }

                Console.WriteLine("Average Rate - " + totalRecords / ((float)totalTime/1000) + " records/second");
                Console.WriteLine("Complete");
                Console.ReadLine();
                
            //while (answer.ToLower() == "y")
            //{
            //    Console.WriteLine("Read Unread Records (Y=Yes, Any Other Key=Exit)");
            //    answer = Console.ReadLine();

            //    if (answer.ToLower() == "y")
            //    {
            //        List<LogRecord> recordslocal = logReader.GetUnreadRecords(1000,"",false);

            //        Console.WriteLine("Record count : " + records.Count.ToString());

            //        foreach (LogRecord lr in recordslocal.OrderBy(x => x.MessageNumber))
            //        {
            //            //string writeMsg = (lr.MessageNumber.ToString() + '\t' + lr.EventFileTime.ToString()  + '\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
            //            string writeMsg = (lr.MessageNumber.ToString() +'\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
            //            log.Info(writeMsg);
            //            //Console.WriteLine(writeMsg);
            //        }
            //    }
            //}
            
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            Console.Read();

            return;
        }
        

        static void writelogs(List<LogRecord> records)
        {
            foreach (LogRecord lr in records.OrderBy(x => x.MessageNumber))
            {
                //string writeMsg = (lr.MessageNumber.ToString() + '\t' + lr.EventFileTime.ToString()  + '\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
                string writeMsg = (lr.MessageNumber.ToString() + '\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
                //log.Info(writeMsg);
                Console.WriteLine(writeMsg);
            }

        }
    }
}
