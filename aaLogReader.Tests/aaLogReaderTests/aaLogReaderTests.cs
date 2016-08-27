using System;
using System.Collections.Generic;
using System.IO;
using aaLogReader.Helpers;
using NUnit.Framework;

namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests : aaLogBaseTest
    {
        private const string ROOT_FILE_PATH = @"aaLogReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";
        private const string LOG_FILE_INSTANCE_MIDDLE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";

        static aaLogReaderTests()
        {
            RootFilePath = ROOT_FILE_PATH;
        }

        /// <summary>
        /// Create a new aaLogReader with the log path for the unit tests.
        /// The method is used to avoid any possible multi-threaded issues in unit tests.
        /// </summary>
        /// <returns></returns>
        private aaLogReader GetLogReader()
        {
            // Make sure the aaLogReader is looking at our test logs and not at the 
            // default log file location.
            return new aaLogReader(new OptionsStruct { LogDirectory = LOG_FILE_PATH });
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Cleanup any old cache files
            foreach (string filename in Directory.GetFiles(LOG_FILE_PATH, "*cache*"))
            {
                File.Delete(filename);
            }
        }

        [Test]
        public void OpenCurrentLogFile()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenCurrentLogFile(LOG_FILE_PATH);

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");
            Assert.That(logReader.CurrentLogFilePath, Contains.Substring(RootFilePath), "CurrentLogFilePath");
        }

        [Test]
        public void OpenCurrentLogFileWithOptions()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenCurrentLogFile();

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");
            Assert.That(logReader.CurrentLogFilePath, Contains.Substring(RootFilePath), "CurrentLogFilePath");
        }

        [Test]
        public void OpenLogFile()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");
            Assert.That(logReader.CurrentLogFilePath, Contains.Substring(RootFilePath), "CurrentLogFilePath");
        }

        [Test]
        public void ReadLogFileReadHeader()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            var actual = logReader.CurrentLogHeader;
            var expected = new LogHeader
            {
                StartMsgNumber = 60380,
                MsgCount = 5,
                StartFileTime = 130943708747330261,
                EndFileTime = 130943708773045647,
                OffsetFirstRecord = 160,
                OffsetLastRecord = 718,
                ComputerName = "2014R2-VS-WSP",
                Session = "Session",
                PrevFileName = "2014R2-VS-WSP1449897272.aaLOG",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void CloseCurrentLogFile()
        {
            var logReader = GetLogReader();
            ReturnCodeStruct rcs = logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");

            rcs = logReader.CloseCurrentLogFile();

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");
        }

        [Test]
        public void CurrentLogFilePath()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True, "Status is wrong");
            Assert.That(rcs.Message.Length, Is.EqualTo(0), "Message.Length is wrong");

            Assert.That(logReader.CurrentLogFilePath, Is.EqualTo(LOG_FILE_INSTANCE), "CurrentLogFilePath is wrong");
        }

        [Test]
        public void GetFirstRecord()
        {
            var logReader = GetLogReader();
            logReader.OpenLogFile(LOG_FILE_INSTANCE);

            var actual = logReader.GetFirstRecord();
            var expected = new LogRecord
            {
                MessageNumber = 60380,
                RecordLength = 94,
                OffsetToPrevRecord = 0,
                OffsetToNextRecord = 254,
                ProcessID = 9672,
                ProcessName = "",
                SessionID = "0.0.0.0",
                ThreadID = 12224,
                EventFileTime = 130943708747330261,
                LogFlag = "Info",
                Component = "aaLogger",
                Message = "Logger Started.",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void GetNextRecordFromFirst()
        {
            var logReader = GetLogReader();
            var rcs = logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True, "Status is wrong");

            var actual0 = logReader.GetFirstRecord();

            var actual1 = logReader.GetNextRecord();
            AreEqual(Expected_60381, actual1);

            var actual2 = logReader.GetNextRecord();
            AreEqual(Expected_60382, actual2);

            var actual3 = logReader.GetNextRecord();
            AreEqual(Expected_60383, actual3);
        }

        [Test]
        public void GetNextRecordFromLast()
        {
            var logReader = GetLogReader();

            //Open a log file in the middle of the overall list of log files
            logReader.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);
            Console.WriteLine(logReader.CurrentLogFilePath);

            var actual0 = logReader.GetLastRecord();

            var actual1 = logReader.GetNextRecord();
            var expected1 = new LogRecord
            {
                MessageNumber = 60385,
                RecordLength = 94,
                OffsetToPrevRecord = 0,
                OffsetToNextRecord = 254,
                ProcessID = 9348,
                ProcessName = "",
                SessionID = "0.0.0.0",
                ThreadID = 10740,
                EventFileTime = 130943708773045647,
                LogFlag = "Info",
                Component = "aaLogger",
                Message = "Logger Started.",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected1, actual1);

            var actual2 = logReader.GetNextRecord();
            var expected2 = new LogRecord
            {
                MessageNumber = 60386,
                RecordLength = 200,
                OffsetToPrevRecord = 160,
                OffsetToNextRecord = 454,
                ProcessID = 6844,
                ProcessName = "aaEngine",
                SessionID = "55.2.254.255",
                ThreadID = 11272,
                EventFileTime = 130943708777415338,
                LogFlag = "Info",
                Component = "ScriptRuntime",
                Message = "ServiceRestarter_001.scr: forcing restart of log viewer",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected2, actual2);

            var actual3 = logReader.GetNextRecord();
            var expected3 = new LogRecord
            {
                MessageNumber = 60387,
                RecordLength = 106,
                OffsetToPrevRecord = 254,
                OffsetToNextRecord = 560,
                ProcessID = 9348,
                ProcessName = "",
                SessionID = "0.0.0.0",
                ThreadID = 10740,
                EventFileTime = 130943708777485254,
                LogFlag = "Info",
                Component = "aaLogger",
                Message = "Logger Shutting down.",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected3, actual3);
        }

        [Test]
        public void GetLastRecord()
        {
            var logReader = GetLogReader();
            logReader.OpenLogFile(LOG_FILE_INSTANCE);

            var actual = logReader.GetLastRecord();
            var expected = new LogRecord
            {
                MessageNumber = 60384,
                RecordLength = 94,
                OffsetToPrevRecord = 612,
                OffsetToNextRecord = 812,
                ProcessID = 9348,
                ProcessName = "",
                SessionID = "0.0.0.0",
                ThreadID = 10740,
                EventFileTime = 130943708773045647,
                LogFlag = "Info",
                Component = "aaLogger",
                Message = "Logger Started.",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void GetLogFilePathsForMessageFileTime()
        {
            var logReader = GetLogReader();
            var logHeaders = logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message
            foreach (var lh in logHeaders)
            {
                Assert.That(logReader.GetLogFilePathsForMessageFileTime(lh.EndFileTime).Exists(x => x == lh.LogFilePath),
                    "End FileTime log path not correctly identified");
                Assert.That(logReader.GetLogFilePathsForMessageFileTime(lh.StartFileTime).Exists(x => x == lh.LogFilePath),
                    "Start FileTime log path not correctly identified");
                Assert.That(
                    logReader.GetLogFilePathsForMessageFileTime((lh.StartFileTime + lh.EndFileTime) / 2)
                        .Exists(x => x == lh.LogFilePath), "Middle FileTime log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageTimestamp()
        {
            var logReader = GetLogReader();
            var logHeaders = logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (var lh in logHeaders)
            {
                Assert.That(logReader.GetLogFilePathsForMessageTimestamp(lh.StartDateTimeUtc).Exists(x => x == lh.LogFilePath),
                    "End Message Timestamp log path not correctly identified");
                Assert.That(logReader.GetLogFilePathsForMessageTimestamp(lh.EndDateTimeUtc).Exists(x => x == lh.LogFilePath),
                    "Start Mesage Timestamp log path not correctly identified");
                Assert.That(
                    logReader.GetLogFilePathsForMessageTimestamp(
                        lh.StartDateTimeUtc.AddSeconds(lh.EndDateTime.Subtract(lh.StartDateTime).Seconds / 2))
                        .Exists(x => x == lh.LogFilePath), "Middle Message Timestamp log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageNumber()
        {
            ulong randomUlong;
            var rnd = new Random(DateTime.Now.Millisecond);

            var logReader = GetLogReader();
            var logHeaders = logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (var lh in logHeaders)
            {
                Assert.That(logReader.GetLogFilePathsForMessageNumber(lh.StartMsgNumber).Exists(x => x == lh.LogFilePath),
                    "End Message Number log path not correctly identified");
                Assert.That(logReader.GetLogFilePathsForMessageNumber(lh.EndMsgNumber).Exists(x => x == lh.LogFilePath),
                    "Start Mesage Number log path not correctly identified");
                Assert.That(
                    logReader.GetLogFilePathsForMessageNumber((lh.StartMsgNumber + lh.EndMsgNumber) / 2)
                        .Exists(x => x == lh.LogFilePath), "Middle Message Number log path not correctly identified");

                randomUlong = (ulong)rnd.Next(Convert.ToInt32(lh.MsgCount)) + lh.StartMsgNumber;
                Assert.That(logReader.GetLogFilePathsForMessageNumber(randomUlong).Exists(x => x == lh.LogFilePath),
                    "Random Message Number " + randomUlong + " log path not correctly identified");
            }
        }


        [Test]
        public void GetPrevRecord()
        {
            var logReader = GetLogReader();
            logReader.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            var actual4 = logReader.GetLastRecord();

            var actual3 = logReader.GetPrevRecord();
            AreEqual(Expected_60383, actual3);

            var actual2 = logReader.GetPrevRecord();
            AreEqual(Expected_60382, actual2);

            var actual1 = logReader.GetPrevRecord();
            AreEqual(Expected_60381, actual1);
        }

        [Test]
        public void GetRecordByMessageNumber()
        {
            var logReader = GetLogReader();
            logReader.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            var actual = logReader.GetRecordByMessageNumber(60383);
            AreEqual(Expected_60383, actual);
        }

        [Test]
        public void GetUnreadRecords()
        {
            var logReader = GetLogReader();
            
            var unreadRecords = logReader.GetUnreadRecords(4, "", true);

            var actual4 = logReader.GetLastRecord();
            AreEqual(actual4, unreadRecords[0],"First record did not match");

            var actual3 = logReader.GetPrevRecord();
            AreEqual(actual3, unreadRecords[1]);

            var actual2 = logReader.GetPrevRecord();
            AreEqual(actual2, unreadRecords[2]);

            var actual1 = logReader.GetPrevRecord();
            AreEqual(actual1, unreadRecords[3],"Last record did not match");

            var unreadRecordsUsingCache = logReader.GetUnreadRecords(4, "", false);

            Assert.AreEqual(0, unreadRecordsUsingCache.Count, "Function should not return records when using cache file");
        }

        [Test]
        public void GetUnreadRecordsIgnoreCacheFile()
        {
            var logReader = GetLogReader();

            var unreadRecords = logReader.GetUnreadRecords(4, "", true);
            Assert.AreEqual(4, unreadRecords.Count, "Function did not return correct number of records.");

            var unreadRecords2 = logReader.GetUnreadRecords(4, "", true);
            Assert.AreEqual(4, unreadRecords2.Count, "Function should return same records when ignoring cache file");

            AreEqual(unreadRecords, unreadRecords2,"Function should return same results for second call when ignoring cache file");
        }

        [Test]
        public void GetUnreadRecordsWithCustomClientID()
        {
            var logReader = GetLogReader();

            var unreadRecords = logReader.GetUnreadRecords(4, "",false,"ClientID1");
            unreadRecords = logReader.GetUnreadRecords(4, "", false, "ClientID1");
            Assert.AreEqual(0, unreadRecords.Count, "Second call with same client ID did not return 0 records");
        }

        [Test]
        public void GetUnreadRecordsWithDifferentCustomClientID()
        {
            var logReader = GetLogReader();

            var unreadRecords = logReader.GetUnreadRecords(4, "", false, "ClientID1");
            unreadRecords = logReader.GetUnreadRecords(4, "", false, "ClientID2");
            Assert.AreEqual(4, unreadRecords.Count, "Second call with different client ID did not return records");
        }

        //[Test]
        //public void GetRecordByFileTime()
        //{
        //    Assert.That(false, "TODO");
        //}

        //[Test]
        //public void GetRecordByFileTimeTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordByTimestampTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void aaLogReaderTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void DisposeTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void OpenLogFileTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void OpenCurrentLogFileTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void CloseCurrentLogFileTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void ReadLogHeaderTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void ReadLogHeaderTest1()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetFirstRecordTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetLastRecordTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetNextRecordTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetPrevRecordTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetLogFilePathsForMessageNumberTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetLogFilePathsForMessageTimestampTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetLogFilePathsForMessageFileTimeTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordByMessageNumberTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartMessageNumberAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByEndMessageNumberAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByMessageNumberAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartandEndMessageNumberTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByEndFileTimeAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByEndTimestampAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartFileTimeAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartTimestampAndCountTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartAndEndFileTimeTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetRecordsByStartAndEndTimeStampTest()
        //{
        //    Assert.Fail("TODO");
        //}

        //[Test]
        //public void GetUnreadRecordsTest()
        //{
        //    Assert.Fail("TODO");
        //}

        #region Shared Expected Result Objects
        private static readonly LogRecord Expected_60381 = new LogRecord
        {
            MessageNumber = 60381,
            RecordLength = 158,
            OffsetToPrevRecord = 160,
            OffsetToNextRecord = 412,
            ProcessID = 6844,
            ProcessName = "aaEngine",
            SessionID = "40.119.32.23",
            ThreadID = 6848,
            EventFileTime = 130943708751488101,
            LogFlag = "Error",
            Component = "ScriptRuntime",
            Message = "LogGen_001.scr: Error Message 703",
            HostFQDN = ExpectedFqdn,
        };

        private static readonly LogRecord Expected_60382 = new LogRecord
        {
            MessageNumber = 60382,
            RecordLength = 200,
            OffsetToPrevRecord = 254,
            OffsetToNextRecord = 612,
            ProcessID = 6844,
            ProcessName = "aaEngine",
            SessionID = "40.119.32.23",
            ThreadID = 11372,
            EventFileTime = 130943708752027927,
            LogFlag = "Info",
            Component = "ScriptRuntime",
            Message = "ServiceRestarter_001.scr: forcing restart of log viewer",
            HostFQDN = ExpectedFqdn,
        };

        private static readonly LogRecord Expected_60383 = new LogRecord
        {
            RecordLength = 106,
            OffsetToPrevRecord = 412,
            OffsetToNextRecord = 718,
            MessageNumber = 60383,
            ProcessID = 9672,
            ProcessName = "",
            SessionID = "0.0.0.0",
            ThreadID = 12224,
            EventFileTime = 130943708752067116,
            LogFlag = "Info",
            Component = "aaLogger",
            Message = "Logger Shutting down.",
            HostFQDN = ExpectedFqdn,
        };
        #endregion
    }
}