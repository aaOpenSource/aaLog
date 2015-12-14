﻿using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;

namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests
    {
        private const string ROOT_FILE_PATH = @"aaLogReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";
        private const string LOG_FILE_INSTANCE_MIDDLE = LOG_FILE_PATH + @"\2014R2-VS-WSP1449897274.aaLog";

        private const string TEMP_PATH = @"C:\LocalRepo\aaLog\aaLogReader.Tests\aaLogReaderTests";

        [TestFixtureSetUp]
        public void Setup()
        {

            // Cleanup any old cache files
            foreach(string filename in Directory.GetFiles(LOG_FILE_PATH,"*cache*"))
            {
                File.Delete(filename);
            }
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
            
            string refHeaderJSON = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\ReadLogFileReadHeader.json");
            string thisHeaderJSON = JsonConvert.SerializeObject(alr.CurrentLogHeader);

            Assert.AreEqual(refHeaderJSON, thisHeaderJSON);
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

            Assert.AreEqual(alr.CurrentLogFilePath, LOG_FILE_INSTANCE,"Current log file path did not match");
            
        }

        [Test]
        public void GetFirstRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = alr.GetFirstRecord();

            string json = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetFirstRecord.json");

            Assert.AreEqual(json, JsonConvert.SerializeObject(lr),"Record contents did not match");

        }

        [Test]
        public void GetNextRecordFromFirst()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);

            LogRecord lr = alr.GetFirstRecord();
            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecord01.json"), JsonConvert.SerializeObject(lr),"Next Record contents did not match for Iteration 1");           

            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecord02.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match for Iteration 2");
            
            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecord03.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match for Iteration 3");
        }

        [Test]
        public void GetNextRecordFromLast()
        {
            aaLogReader alr = new aaLogReader();

            //Open a log file in the middle of the overall list of log files
            alr.OpenLogFile(LOG_FILE_INSTANCE_MIDDLE);

            LogRecord lr = alr.GetLastRecord();
            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecordFromLast01.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");
      
            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecordFromLast02.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");
      
            lr = alr.GetNextRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetNextRecordFromLast03.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");
        }

        [Test]
        public void GetLastRecord()
        {
            aaLogReader alr = new aaLogReader();

            alr.OpenLogFile(LOG_FILE_INSTANCE);
            
            LogRecord lr = alr.GetLastRecord();


            
            string json = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetLastRecord.json");
            
            Assert.AreEqual(json, JsonConvert.SerializeObject(lr), "Record contents did not match");

        }

        [Test]
        public void GetLogFilePathsForMessageFileTime()
        {

            OptionsStruct options = new OptionsStruct();
            options.LogDirectory = LOG_FILE_PATH;

            aaLogReader alr = new aaLogReader(options);
            
            List<LogHeader> logHeaders = alr.LogHeaderIndex;

            // Loop through all entries in the header index and verify we correctly identify the first, last, and middle message

            foreach(LogHeader lh in logHeaders)
            {
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.EndFileTime).Exists(x=>x==lh.LogFilePath),"End FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime(lh.StartFileTime).Exists(x => x == lh.LogFilePath), "Start FileTime log path not correctly identified");
                Assert.That(alr.GetLogFilePathsForMessageFileTime((lh.StartFileTime + lh.EndFileTime)/2).Exists(x => x == lh.LogFilePath), "Middle FileTime log path not correctly identified");                
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
                Assert.That(alr.GetLogFilePathsForMessageTimestamp(lh.StartDateTime.AddSeconds(lh.EndDateTime.Subtract(lh.StartDateTime).Seconds/2)).Exists(x => x == lh.LogFilePath), "Middle Message Timestamp log path not correctly identified");
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
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetPrevRecord01.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");

            lr = alr.GetPrevRecord();
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetPrevRecord02.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");

            lr = alr.GetPrevRecord();
            //File.WriteAllText(TEMP_PATH + @"\refFiles\GetPrevRecord03.json", JsonConvert.SerializeObject(lr));
            Assert.AreEqual(File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\GetPrevRecord03.json"), JsonConvert.SerializeObject(lr), "Next Record contents did not match");            
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