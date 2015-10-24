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


        static void f1()
        {
            //try
            //{
                throw new Exception("Execption in F1");
            //}
            //catch(Exception)
            //{
            //    throw;
            //}
        }

        static void f2()
        {
            try
            {
                throw new Exception("Execption in F2");
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void Main(string[] args)
        {
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

            try
            {

                string answer;

            aaLogReader.OptionsStruct testOptions = JsonConvert.DeserializeObject<OptionsStruct>(System.IO.File.ReadAllText("options.json"));
            testOptions.IgnoreCacheFileOnFirstRead = true;
          
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "Message", Filter = "Warning 40|Message 41" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "MessageNumberMin", Filter = "6826080" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "MessageNumberMax", Filter = "6826085" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "DateTimeMin", Filter = "2015-06-19 01:45:00" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "DateTimeMax", Filter = "2015-06-19 01:45:05" });

            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "ProcessID", Filter = "7260" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "ThreadID", Filter = "7264" });
            
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "Message", Filter = "Started" });
            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct() { Field = "HostFQDN", Filter = "865" });

            answer = "y";

            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader(testOptions);

            //List<LogHeader> logHeaderIndexes = logReader.LogHeaderIndex;

            //System.IO.File.WriteAllLines("headers.txt", new string[] { aaLogReader.LogHeader.HeaderTSV() });

            ////Console.WriteLine(aaLogReader.LogHeader.HeaderTSV());

            //foreach (aaLogReader.LogHeader localHeader in logReader.LogHeaderIndex)
            //{
            //    System.IO.File.AppendAllLines("headers.txt", new string[] { localHeader.ToTSV() });
            //}


            //testOptions.LogRecordPostFilters.Add(new LogRecordFilterStruct(){Field="LogFlag",Filter="Warning"});

            //System.IO.File.WriteAllText("options2.json", JsonConvert.SerializeObject(testOptions));

            //aaLogReader.Testing.HeaderTesting.TestHeaderIndexingSearches(logReader);

            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber(0) == "Tom");
            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber((ulong)10000) == "Jerry");
            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber((ulong)20000));
            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber(30000));
            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber(40000));
            //Console.WriteLine(logReader.GetLogFilePathForMessageNumber(50000));

           

                //8064517	6/27/2015	1:42:45 PM	953	7224	7228	Warning	ScriptRuntime	logs.scrLog: Warning 1741	

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                List<LogRecord> records = new List<LogRecord>();
                LogRecord record;

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
                
                ////sw.Stop();
                
                ////log.InfoFormat("Found {0} messages", records.Count);

                ////log.InfoFormat("Time - {0} millseconds", sw.ElapsedMilliseconds);

                ////writelogs(records);

                ////log.Info(JsonConvert.SerializeObject(records));
                
                ////sw.Stop();
                ////log.InfoFormat("Time - {0}",sw.ElapsedMilliseconds);
                ////log.InfoFormat("Count - {0}", records.Count);

                //int count = 10;
                ////ulong startmsg = 2534930;
                ////ulong endmsg = 2559030;

                //ulong startmsg = 7064310;
                //ulong endmsg = 7064310;

                //DateTime startTime = DateTime.Parse("2015-06-21 08:00:00");
                ////DateTime endTime = new DateTime.Parse("2015-06-27 08:00:00");
                //DateTime endTime = DateTime.Parse("2015-06-28 08:00:00");

                //sw.Restart();                
                //records = logReader.GetRecordsByStartAndEndTimeStamp(startTime, endTime);
                //sw.Stop();
                //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                //log.InfoFormat("Count - {0}", records.Count);

                //log.Info("Forward");
                //sw.Restart();
                //records = logReader.GetRecordsByStartMessageNumberAndCount(startmsg, count);
                //sw.Stop();
                //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                //log.InfoFormat("Count - {0}", records.Count);

                //log.Info("Forward");
                //sw.Restart();
                //logReader.GetRecordsByStartMessageNumberAndCount(startmsg, count);
                //sw.Stop();
                //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                
                //log.Info("Back");
                //sw.Start();
                //records = logReader.GetRecordsByEndMessageNumberAndCount(endmsg, count);
                //sw.Stop();
                //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                //log.InfoFormat("Count - {0}", records.Count);

                //log.Info("Back");
                //sw.Start();
                //logReader.GetRecordsByEndMessageNumberAndCount(endmsg, count);
                //sw.Stop();
                //log.InfoFormat("Time - {0}", sw.ElapsedMilliseconds);
                
                //Console.ReadLine();
                //return;

                //List<LogHeader> lhindex = logReader.LogHeaderIndex;

                //log.Info(logReader.GetRecordByMessageNumber(197753).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(225592).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(0).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(514).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(515).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(750).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(751).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1633).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1634).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2368).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2369).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2889).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2890).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3407).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3408).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(9171).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(9172).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(39747).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(39748).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(65346).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(65347).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(92407).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(92408).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(118783).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(118784).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(145068).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(145069).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(171416).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(171417).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(197754).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(197753).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(225592).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(225593).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(253186).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(253187).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(280781).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(280782).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(308376).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(308377).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(336022).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(336023).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(363713).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(363714).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(391314).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(391315).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(418833).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(418834).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(445223).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(445224).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(471547).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(471548).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(497805).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(497806).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(524056).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(524057).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(550307).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(550308).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(578776).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(578777).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(606652).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(606653).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(634315).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(634316).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(661978).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(661979).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(690427).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(690428).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(718162).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(718163).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(745826).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(745827).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(756921).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(756922).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(793764).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(793765).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(828609).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(828610).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(861803).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(861804).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(891973).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(891974).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(922144).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(922145).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(952324).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(952325).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(982407).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(982408).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1012560).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1012561).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1042788).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1042789).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1072959).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1072960).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1103130).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1103131).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1133342).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1133343).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1163424).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1163425).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1193541).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1193542).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1223767).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1223768).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1242715).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1242716).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1243240).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1243241).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1246484).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1246485).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1248036).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1248037).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1248491).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1248492).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1249309).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1249310).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1253272).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7015707).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7018129).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1253273).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1253810).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1253811).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1254264).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1254265).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1259609).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1259610).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1260058).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1260059).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1260999).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1260999).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1261002).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1261000).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1261451).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1261452).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1262185).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1262186).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1262859).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1262860).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1263385).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1263386).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1263930).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1263931).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1264464).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1264465).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1265456).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1265457).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1265926).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1265927).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1266833).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1266834).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1266890).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1266891).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1267402).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1267403).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1267938).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1267939).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1281254).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1281255).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1303888).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1303889).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1306280).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1306281).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1327942).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1327943).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1329700).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1329701).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1383189).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1383190).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1437239).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1437240).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1491288).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1491289).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1545337).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1545338).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1599386).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1599387).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1653436).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1653437).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1707485).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1707486).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1761534).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1761535).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1815583).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1815584).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1869633).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1869634).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1923302).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1923303).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1966727).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1966728).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1967570).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1967571).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1971936).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1971937).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1972768).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1972769).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1973880).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1973881).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1974575).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1974576).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1975773).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1975774).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(1976519).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(1976520).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2019420).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2019421).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2062379).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2062380).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2105337).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2105338).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2148295).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2148296).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2191254).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2191255).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2234213).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2234214).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2277171).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2277172).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2320130).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2320131).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2363089).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2363090).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2406047).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2406048).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2449006).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2449007).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2491965).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2491966).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2534926).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2534925).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2577882).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2577883).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2620841).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2620842).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2663799).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2663800).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2706757).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2706758).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2749716).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2749717).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2792674).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2792675).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2835632).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2835633).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2878591).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2878592).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2921550).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2921551).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2964508).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2964509).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(2964941).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(2964942).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3004251).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3004252).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3060553).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3060554).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3117991).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3117992).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3122401).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3122402).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3128851).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3128852).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3140979).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3140980).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3152368).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3152369).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3184761).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3184762).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3185444).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3185445).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3187243).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3187244).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3187924).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3187925).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3188605).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3188606).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3194547).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3194548).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3256648).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3256649).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3299334).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3299335).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3367261).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3367262).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3452746).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3452747).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3538533).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3538534).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3624322).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3624323).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3710111).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3710112).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3795898).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3795899).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3881687).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3881688).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(3967476).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(3967477).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4005933).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4005934).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4068357).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4068358).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4131768).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4131769).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4135901).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4135902).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4156680).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4156681).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4166868).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4166869).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4230921).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4230922).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4295645).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4295646).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4360367).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4360366).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4425089).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4425090).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4489812).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4489813).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4554535).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4554536).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4619246).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4619247).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4683957).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4683958).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4748668).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4748669).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4813379).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4813380).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4878093).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4878094).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(4942805).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(4942806).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5007490).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5007491).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5072163).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5072164).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5136851).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5136852).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5201565).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5201566).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5266289).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5266290).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5331013).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5331014).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5395737).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5395738).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5460420).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5460421).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5525144).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5525145).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5589868).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5589869).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5654592).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5654593).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5719316).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5719317).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5784040).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5784041).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5848747).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5848748).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5913157).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5913158).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(5973074).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(5973075).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6032991).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6032992).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6069504).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6069505).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6114915).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6114916).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6118182).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6118183).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6142091).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6142092).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6157606).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6157607).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6179972).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6179973).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6184871).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6184872).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6239146).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6239147).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6284001).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6284002).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6284741).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6284742).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6285470).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6285471).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6286196).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6286197).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6290542).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6290543).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6291263).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6291264).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6292532).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6292533).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6293410).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6293411).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6309204).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6309205).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6318650).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6318651).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6325372).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6325373).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6383613).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6383614).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6442188).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6442189).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6500762).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6500763).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6524837).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6524838).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6583077).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6583078).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6641652).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6641653).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6700227).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6700228).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6758802).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6758803).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6763649).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6763650).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6796649).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6796650).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6869853).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6869854).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(6942997).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(6942998).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7015706).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7018130).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7032705).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7032706).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7104019).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7104020).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7175441).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7175442).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7246863).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7246864).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7318285).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7318286).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7389707).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7389708).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7457744).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7457745).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7505598).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7505599).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7556032).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7556033).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7627454).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7627455).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7697911).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7697912).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7768754).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7768755).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7839597).ToJSON());
                //log.Info(logReader.GetRecordByMessageNumber(7839598).ToJSON()); log.Info(logReader.GetRecordByMessageNumber(7898405).ToJSON());


                //Console.ReadLine(); return;

            while (answer.ToLower() == "y")
            {
                Console.WriteLine("Read Unread Records (Y=Yes, Any Other Key=Exit)");
                answer = Console.ReadLine();

                if (answer.ToLower() == "y")
                {
                    List<LogRecord> recordslocal = logReader.GetUnreadRecords(1000,"",false);

                    Console.WriteLine("Record count : " + records.Count.ToString());

                    foreach (LogRecord lr in recordslocal.OrderBy(x => x.MessageNumber))
                    {
                        //string writeMsg = (lr.MessageNumber.ToString() + '\t' + lr.EventFileTime.ToString()  + '\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
                        string writeMsg = (lr.MessageNumber.ToString() +'\t' + lr.EventDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") + '\t' + lr.LogFlag + '\t' + lr.Message);
                        log.Info(writeMsg);
                        //Console.WriteLine(writeMsg);
                    }
                }
            }
            
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
