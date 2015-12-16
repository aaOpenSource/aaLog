using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests
    {
        private static readonly string TEST_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string ROOT_FILE_PATH = Path.Combine(TEST_PATH, @"aaLogReaderTests");
        private static readonly string LOG_FILE_PATH = Path.Combine(ROOT_FILE_PATH, @"logFiles");
        private static readonly string LOG_FILE_INSTANCE = Path.Combine(LOG_FILE_PATH, @"2014R2-VS-WSP1449897274.aaLog");
        private static readonly string LOG_FILE_INSTANCE_MIDDLE = Path.Combine(LOG_FILE_PATH, @"2014R2-VS-WSP1449897274.aaLog");

        private const string TEMP_PATH = @"C:\LocalRepo\aaLog\aaLogReader.Tests\aaLogReaderTests";

        [OneTimeSetUp]
        public void Setup()
        {

            // Cleanup any old cache files
            foreach (string filename in Directory.GetFiles(LOG_FILE_PATH, "*cache*"))
            {
                File.Delete(filename);
            }
        }

        /// <summary>
        /// Removes system specific entries from JSON objects
        /// </summary>
        /// <param name="obj"></param>
        private void Cleanse(JObject obj)
        {
            obj.Remove("EventDateTime");
            obj.Remove("StartDateTime");
            obj.Remove("EndDateTime");
            obj.Remove("HostFQDN");
        }

        private void Compare<T>(string fileName, T actualObj, string message = null)
        {
            var actualJson = JObject.FromObject(actualObj);
            Compare(fileName, actualJson, message);
        }

        /// <summary>
        /// Compares an actual JSON object result to a reference JSON file
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="fileName"></param>
        /// <param name="actualJson"></param>
        private void Compare(string fileName, JObject actualJson, string message = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(ROOT_FILE_PATH, fileName));
            var expectedJson = JObject.Parse(expectedText);

            Cleanse(actualJson);
            Cleanse(expectedJson);

            Assert.AreEqual(expectedJson, actualJson, message);
        }

        [Test]
        public void OpenCurrentLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenCurrentLogFile(LOG_FILE_PATH);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void OpenCurrentLogFileWithOptions()
        {
            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader(options);

            ReturnCodeStruct rcs = alr.OpenCurrentLogFile();

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void OpenLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void ReadLogFileReadHeader()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            Compare(@"refFiles\ReadLogFileReadHeader.json", alr.CurrentLogHeader);
        }

        [Test]
        public void CloseCurrentLogFile()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            rcs = alr.CloseCurrentLogFile();

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);
        }

        [Test]
        public void CurrentLogFilePath()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            Assert.AreEqual(alr.CurrentLogFilePath, LOG_FILE_INSTANCE, "Current log file path did not match");

        }

        [Test]
        public void GetFirstRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord actualRecord = alr.GetFirstRecord();
            Compare(@"refFiles\GetFirstRecord.json", actualRecord);
        }

        [Test]
        public void GetNextRecordFromFirst()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = alr.GetFirstRecord();
            lr = alr.GetNextRecord();
            Compare(@"refFiles\GetNextRecord01.json", lr, "Next Record contents did not match for Iteration 1");

            lr = alr.GetNextRecord();
            Compare(@"refFiles\GetNextRecord02.json", lr, "Next Record contents did not match for Iteration 2");

            lr = alr.GetNextRecord();
            Compare(@"refFiles\GetNextRecord03.json", lr, "Next Record contents did not match for Iteration 3");
        }

        [Test]
        public void GetNextRecordFromLast()
        {
            aaLogReader alr = new aaLogReader();

            //Open a log file in the middle of the overall list of log files
            alr.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            LogRecord lr = alr.GetLastRecord();
            lr = alr.GetNextRecord();
            // TODO: Assert move to next log file
            Compare(@"refFiles\GetNextRecordFromLast01.json", lr, "Next Record contents did not match 1");

            lr = alr.GetNextRecord();
            Compare(@"refFiles\GetNextRecordFromLast02.json", lr, "Next Record contents did not match 2");

            lr = alr.GetNextRecord();
            Compare(@"refFiles\GetNextRecordFromLast03.json", lr, "Next Record contents did not match 3");
        }

        [Test]
        public void GetLastRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = alr.GetLastRecord();
            Compare(@"refFiles\GetLastRecord.json", lr, "Record contents did not match");
        }

        [Test]
        public void GetLogFilePathsForMessageFileTime()
        {
            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader(options);

            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.EndFileTime).Exists(x => x == lh.LogFilePath), "End FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.StartFileTime).Exists(x => x == lh.LogFilePath), "Start FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime((lh.StartFileTime + lh.EndFileTime) / 2).Exists(x => x == lh.LogFilePath), "Middle FileTime log path not correctly identified");
            }

        }

        [Test]
        public void GetLogFilePathsForMessageTimestamp()
        {
            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader(options);

            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.StartDateTime).Exists(x => x == lh.LogFilePath), "End Message Timestamp log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.EndDateTime).Exists(x => x == lh.LogFilePath), "Start Mesage Timestamp log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.StartDateTime.AddSeconds(lh.EndDateTime.Subtract(lh.StartDateTime).Seconds / 2)).Exists(x => x == lh.LogFilePath), "Middle Message Timestamp log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageNumber()
        {
            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;
            ulong RandomUlong;
            System.Random rnd = new Random(DateTime.Now.Millisecond);

            aaLogReader alr = new aaLogReader(options);

            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageNumber(lh.StartMsgNumber).Exists(x => x == lh.LogFilePath), "End Message Number log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageNumber(lh.EndMsgNumber).Exists(x => x == lh.LogFilePath), "Start Mesage Number log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageNumber((lh.StartMsgNumber + lh.EndMsgNumber) / 2).Exists(x => x == lh.LogFilePath), "Middle Message Number log path not correctly identified");

                RandomUlong = (ulong)rnd.Next(Convert.ToInt32(lh.MsgCount)) + lh.StartMsgNumber;
                Assert.That(alr.GetLogFilePathsForMessageNumber(RandomUlong).Exists(x => x == lh.LogFilePath), "Random Message Number " + RandomUlong + " log path not correctly identified");

            }
        }

        [Test]
        public void GetPrevRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            LogRecord lr = alr.GetLastRecord();
            lr = alr.GetPrevRecord();
            Compare(@"refFiles\GetPrevRecord01.json", lr, "Prev Record contents did not match 1");

            lr = alr.GetPrevRecord();
            Compare(@"refFiles\GetPrevRecord02.json", lr, "Prev Record contents did not match 2");

            lr = alr.GetPrevRecord();
            Compare(@"refFiles\GetPrevRecord03.json", lr, "Prev Record contents did not match 3");
        }

        [Test]
        public void GetRecordByMessageNumber()
        {
            Assert.That(false, "TODO");
        }

        [Test]
        public void GetRecordByFileTime()
        {
            Assert.That(false, "TODO");
        }

        [Test()]
        public void GetRecordByFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordByTimestampTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void aaLogReaderTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void DisposeTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void OpenLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void OpenCurrentLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void CloseCurrentLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void ReadLogHeaderTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void ReadLogHeaderTest1()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetFirstRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetLastRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetNextRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetPrevRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetLogFilePathsForMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetLogFilePathsForMessageTimestampTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetLogFilePathsForMessageFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordByMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByEndMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartandEndMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByEndFileTimeAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByEndTimestampAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartFileTimeAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartTimestampAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartAndEndFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetRecordsByStartAndEndTimeStampTest()
        {
            Assert.Fail("TODO");
        }

        [Test()]
        public void GetUnreadRecordsTest()
        {
            Assert.Fail("TODO");
        }


    }
}