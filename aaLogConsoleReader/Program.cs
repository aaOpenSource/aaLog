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

            //aaLogReader.aaLogReaderOptions testOptions = new aaLogReader.aaLogReaderOptions();

            //System.IO.File.WriteAllText("options.json", JsonConvert.SerializeObject(testOptions));

            aaLogReader.aaLogReaderOptions testOptions = JsonConvert.DeserializeObject<aaLogReaderOptions>(System.IO.File.ReadAllText("options.json"));

            answer = "y";

            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(testOptions);
            
            while (answer.ToLower() == "y")
            {
                Console.WriteLine("Read Unread Records (Y=Yes, N=Exit)");
                answer = Console.ReadLine();

                if (answer.ToLower() == "y")
                {

                    //Force reread of options file on every cycle
                    logReader.SetOptions(JsonConvert.DeserializeObject<aaLogReaderOptions>(System.IO.File.ReadAllText("options.json")));

                    List<LogRecord> records = logReader.GetUnreadRecords(10);

                    Console.WriteLine("Record count : " + records.Count.ToString());

                    foreach (LogRecord lr in records)
                    {
                        Console.WriteLine(lr.MessageNumber.ToString() + " " + lr.Message);
                        //Console.WriteLine(lr.ToKVP());
                    }
                }
            }
        }
    }
}
