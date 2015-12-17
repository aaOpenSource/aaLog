using System;
using System.Collections.Generic;
using System.IO;
using aaLogReader.Helpers;
using NUnit.Framework;

namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests
    {
        private const string ROOT_FILE_PATH = @"aaLogReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";
        private const string LOG_FILE_INSTANCE_MIDDLE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";

        private string _expectedFqdn;
        private aaLogReader _logReader;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Cache this machine's FQDN.
            _expectedFqdn = Fqdn.GetFqdn();

            // Cleanup any old cache files
            foreach (string filename in Directory.GetFiles(LOG_FILE_PATH, "*cache*"))
            {
                File.Delete(filename);
            }
        }

        [SetUp]
        public void SetUp()
        {
            // Make sure the aaLogReader is looking at our test logs and not at the 
            // default log file location.
            _logReader = new aaLogReader(new OptionsStruct {LogDirectory = LOG_FILE_PATH});
        }

        [Test]
        public void OpenCurrentLogFile()
        {
            ReturnCodeStruct rcs = _logReader.OpenCurrentLogFile(LOG_FILE_PATH);

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));
        }

        [Test]
        public void OpenCurrentLogFileWithOptions()
        {
            ReturnCodeStruct rcs = _logReader.OpenCurrentLogFile();

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));
        }

        [Test]
        public void OpenLogFile()
        {
            ReturnCodeStruct rcs = _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));
        }

        [Test]
        public void ReadLogFileReadHeader()
        {
            ReturnCodeStruct rcs = _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            var header = _logReader.CurrentLogHeader;
            Assert.That(header.LogFilePath, Is.Null);
            Assert.That(header.StartMsgNumber, Is.EqualTo(60380));
            Assert.That(header.MsgCount, Is.EqualTo(5));
            Assert.That(header.EndMsgNumber, Is.EqualTo(60384));
            Assert.That(header.StartFileTime, Is.EqualTo(130943708747330261));
            Assert.That(header.EndFileTime, Is.EqualTo(130943708773045647));
            Assert.That(header.OffsetFirstRecord, Is.EqualTo(160));
            Assert.That(header.OffsetLastRecord, Is.EqualTo(718));
            Assert.That(header.ComputerName, Is.EqualTo("2014R2-VS-WSP"));
            Assert.That(header.Session, Is.EqualTo("Session"));
            Assert.That(header.PrevFileName, Is.EqualTo("2014R2-VS-WSP1449897272.aaLOG"));
            Assert.That(header.HostFQDN, Is.EqualTo(_expectedFqdn));
        }

        [Test]
        public void CloseCurrentLogFile()
        {
            ReturnCodeStruct rcs = _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));

            rcs = _logReader.CloseCurrentLogFile();

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));
        }

        [Test]
        public void CurrentLogFilePath()
        {
            ReturnCodeStruct rcs = _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status, Is.True);
            Assert.That(rcs.Message.Length, Is.EqualTo(0));

            Assert.That(_logReader.CurrentLogFilePath, Is.EqualTo(LOG_FILE_INSTANCE));
        }

        [Test]
        public void GetFirstRecord()
        {
            _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = _logReader.GetFirstRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60380));
            Assert.That(lr.ProcessID, Is.EqualTo(9672));
            Assert.That(lr.ThreadID, Is.EqualTo(12224));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708747330261));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Started."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));
        }

        [Test]
        public void GetNextRecordFromFirst()
        {
            _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = _logReader.GetFirstRecord();

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60381));
            Assert.That(lr.ProcessID, Is.EqualTo(6844));
            Assert.That(lr.ThreadID, Is.EqualTo(6848));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708751488101));
            Assert.That(lr.LogFlag, Is.EqualTo("Error"));
            Assert.That(lr.Component, Is.EqualTo("ScriptRuntime"));
            Assert.That(lr.Message, Is.EqualTo("LogGen_001.scr: Error Message 703"));
            Assert.That(lr.ProcessName, Is.EqualTo("aaEngine"));
            Assert.That(lr.SessionID, Is.EqualTo("40.119.32.23"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60382));
            Assert.That(lr.ProcessID, Is.EqualTo(6844));
            Assert.That(lr.ThreadID, Is.EqualTo(11372));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708752027927));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("ScriptRuntime"));
            Assert.That(lr.Message, Is.EqualTo("ServiceRestarter_001.scr: forcing restart of log viewer"));
            Assert.That(lr.ProcessName, Is.EqualTo("aaEngine"));
            Assert.That(lr.SessionID, Is.EqualTo("40.119.32.23"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60383));
            Assert.That(lr.ProcessID, Is.EqualTo(9672));
            Assert.That(lr.ThreadID, Is.EqualTo(12224));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708752067116));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Shutting down."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));
        }

        [Test]
        public void GetNextRecordFromLast()
        {
            //Open a log file in the middle of the overall list of log files
            _logReader.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);
            Console.WriteLine(_logReader.CurrentLogFilePath);

            LogRecord lr = _logReader.GetLastRecord();

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60385));
            Assert.That(lr.ProcessID, Is.EqualTo(9348));
            Assert.That(lr.ThreadID, Is.EqualTo(10740));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708773045647));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Started."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60386));
            Assert.That(lr.ProcessID, Is.EqualTo(6844));
            Assert.That(lr.ThreadID, Is.EqualTo(11272));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708777415338));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("ScriptRuntime"));
            Assert.That(lr.Message, Is.EqualTo("ServiceRestarter_001.scr: forcing restart of log viewer"));
            Assert.That(lr.ProcessName, Is.EqualTo("aaEngine"));
            Assert.That(lr.SessionID, Is.EqualTo("55.2.254.255"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetNextRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60387));
            Assert.That(lr.ProcessID, Is.EqualTo(9348));
            Assert.That(lr.ThreadID, Is.EqualTo(10740));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708777485254));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Shutting down."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));
        }

        [Test]
        public void GetLastRecord()
        {
            _logReader.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = _logReader.GetLastRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60384));
            Assert.That(lr.ProcessID, Is.EqualTo(9348));
            Assert.That(lr.ThreadID, Is.EqualTo(10740));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708773045647));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Started."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));
        }

        [Test]
        public void GetLogFilePathsForMessageFileTime()
        {
            List<LogHeader> logHeaders = _logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(_logReader.GetLogFilePathsForMessageFileTime(lh.EndFileTime).Exists(x => x == lh.LogFilePath),
                    "End FileTime log path not correctly identified");
                Assert.That(_logReader.GetLogFilePathsForMessageFileTime(lh.StartFileTime).Exists(x => x == lh.LogFilePath),
                    "Start FileTime log path not correctly identified");
                Assert.That(
                    _logReader.GetLogFilePathsForMessageFileTime((lh.StartFileTime + lh.EndFileTime)/2)
                        .Exists(x => x == lh.LogFilePath), "Middle FileTime log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageTimestamp()
        {
            List<LogHeader> logHeaders = _logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(_logReader.GetLogFilePathsForMessageTimestamp(lh.StartDateTime).Exists(x => x == lh.LogFilePath),
                    "End Message Timestamp log path not correctly identified");
                Assert.That(_logReader.GetLogFilePathsForMessageTimestamp(lh.EndDateTime).Exists(x => x == lh.LogFilePath),
                    "Start Mesage Timestamp log path not correctly identified");
                Assert.That(
                    _logReader.GetLogFilePathsForMessageTimestamp(
                        lh.StartDateTime.AddSeconds(lh.EndDateTime.Subtract(lh.StartDateTime).Seconds/2))
                        .Exists(x => x == lh.LogFilePath), "Middle Message Timestamp log path not correctly identified");
            }
        }

        [Test]
        public void GetLogFilePathsForMessageNumber()
        {
            ulong randomUlong;
            Random rnd = new Random(DateTime.Now.Millisecond);

            List<LogHeader> logHeaders = _logReader.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach (LogHeader lh in logHeaders)
            {
                Assert.That(_logReader.GetLogFilePathsForMessageNumber(lh.StartMsgNumber).Exists(x => x == lh.LogFilePath),
                    "End Message Number log path not correctly identified");
                Assert.That(_logReader.GetLogFilePathsForMessageNumber(lh.EndMsgNumber).Exists(x => x == lh.LogFilePath),
                    "Start Mesage Number log path not correctly identified");
                Assert.That(
                    _logReader.GetLogFilePathsForMessageNumber((lh.StartMsgNumber + lh.EndMsgNumber)/2)
                        .Exists(x => x == lh.LogFilePath), "Middle Message Number log path not correctly identified");

                randomUlong = (ulong) rnd.Next(Convert.ToInt32(lh.MsgCount)) + lh.StartMsgNumber;
                Assert.That(_logReader.GetLogFilePathsForMessageNumber(randomUlong).Exists(x => x == lh.LogFilePath),
                    "Random Message Number " + randomUlong + " log path not correctly identified");
            }
        }


        [Test]
        public void GetPrevRecord()
        {
            _logReader.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            LogRecord lr = _logReader.GetLastRecord();

            lr = _logReader.GetPrevRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60383));
            Assert.That(lr.ProcessID, Is.EqualTo(9672));
            Assert.That(lr.ThreadID, Is.EqualTo(12224));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708752067116));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("aaLogger"));
            Assert.That(lr.Message, Is.EqualTo("Logger Shutting down."));
            Assert.That(lr.ProcessName, Is.EqualTo(""));
            Assert.That(lr.SessionID, Is.EqualTo("0.0.0.0"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetPrevRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60382));
            Assert.That(lr.ProcessID, Is.EqualTo(6844));
            Assert.That(lr.ThreadID, Is.EqualTo(11372));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708752027927));
            Assert.That(lr.LogFlag, Is.EqualTo("Info"));
            Assert.That(lr.Component, Is.EqualTo("ScriptRuntime"));
            Assert.That(lr.Message, Is.EqualTo("ServiceRestarter_001.scr: forcing restart of log viewer"));
            Assert.That(lr.ProcessName, Is.EqualTo("aaEngine"));
            Assert.That(lr.SessionID, Is.EqualTo("40.119.32.23"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));

            lr = _logReader.GetPrevRecord();
            Assert.That(lr.MessageNumber, Is.EqualTo(60381));
            Assert.That(lr.ProcessID, Is.EqualTo(6844));
            Assert.That(lr.ThreadID, Is.EqualTo(6848));
            Assert.That(lr.EventFileTime, Is.EqualTo(130943708751488101));
            Assert.That(lr.LogFlag, Is.EqualTo("Error"));
            Assert.That(lr.Component, Is.EqualTo("ScriptRuntime"));
            Assert.That(lr.Message, Is.EqualTo("LogGen_001.scr: Error Message 703"));
            Assert.That(lr.ProcessName, Is.EqualTo("aaEngine"));
            Assert.That(lr.SessionID, Is.EqualTo("40.119.32.23"));
            Assert.That(lr.HostFQDN, Is.EqualTo(_expectedFqdn));
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

        [Test]
        public void GetRecordByFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordByTimestampTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void aaLogReaderTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void DisposeTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void OpenLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void OpenCurrentLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void CloseCurrentLogFileTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void ReadLogHeaderTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void ReadLogHeaderTest1()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetFirstRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetLastRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetNextRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetPrevRecordTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetLogFilePathsForMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetLogFilePathsForMessageTimestampTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetLogFilePathsForMessageFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordByMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByEndMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByMessageNumberAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartandEndMessageNumberTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByEndFileTimeAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByEndTimestampAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartFileTimeAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartTimestampAndCountTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartAndEndFileTimeTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetRecordsByStartAndEndTimeStampTest()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GetUnreadRecordsTest()
        {
            Assert.Fail("TODO");
        }
    }
}