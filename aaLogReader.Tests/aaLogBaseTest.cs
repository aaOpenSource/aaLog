using aaLogReader.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;

namespace aaLogReader.Tests
{
    public class aaLogBaseTest
    {
        public static readonly string TEST_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static readonly string _expectedFqdn;
        private static string _rootFilePath;

        static aaLogBaseTest()
        {
            _expectedFqdn = Fqdn.GetFqdn();
        }

        public static string ExpectedFqdn
        {
            get { return _expectedFqdn; }
        }

        public static string RootFilePath
        {
            get
            {
                if (_rootFilePath == null)
                    throw new NotImplementedException();
                return _rootFilePath;
            }
            protected set { _rootFilePath = value; }
        }

        public void AreEqual(string fileName, LogHeader actualObj, string optionalMessage = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(RootFilePath, fileName));
            var expectedObj = JsonConvert.DeserializeObject<LogHeader>(expectedText);
            AreEqual(expectedObj, actualObj, optionalMessage);
        }

        public void AreEqual(LogHeader expected, LogHeader actual, string optionalMessage = null)
        {
            Assert.AreEqual(expected.LogFilePath, actual.LogFilePath, "LogFilePath is wrong");
            Assert.AreEqual(expected.StartMsgNumber, actual.StartMsgNumber, "StartMsgNumber is wrong");
            Assert.AreEqual(expected.MsgCount, actual.MsgCount, "MsgCount is wrong");
            Assert.AreEqual(expected.EndMsgNumber, actual.EndMsgNumber, "EndMsgNumber is wrong");
            Assert.AreEqual(expected.StartFileTime, actual.StartFileTime, "StartFileTime is wrong");
            Assert.AreEqual(expected.EndFileTime, actual.EndFileTime, "EndFileTime is wrong");
            Assert.AreEqual(expected.OffsetFirstRecord, actual.OffsetFirstRecord, "OffsetFirstRecord is wrong");
            Assert.AreEqual(expected.OffsetLastRecord, actual.OffsetLastRecord, "OffsetLastRecord is wrong");
            Assert.AreEqual(expected.ComputerName, actual.ComputerName, "ComputerName is wrong");
            Assert.AreEqual(expected.Session, actual.Session, "Session is wrong");
            Assert.AreEqual(expected.PrevFileName, actual.PrevFileName, "PrevFileName is wrong");
            // This is not really a property of the source LogHeader, but of the reader
            //Assert.AreEqual(expected.HostFQDN, ExpectedFqdn, "HostFQDN is wrong");
        }

        public void AreEqual(string fileName, LogRecord actualObj, string optionalMessage = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(RootFilePath, fileName));
            var expectedObj = JsonConvert.DeserializeObject<LogRecord>(expectedText);
            AreEqual(expectedObj, actualObj, optionalMessage);
        }

        public void AreEqual(LogRecord expected, LogRecord actual, string optionalMessage = null)
        {
            Assert.AreEqual(expected.RecordLength, actual.RecordLength, "RecordLength is wrong");
            Assert.AreEqual(expected.OffsetToPrevRecord, actual.OffsetToPrevRecord, "OffsetToPrevRecord is wrong");
            Assert.AreEqual(expected.OffsetToNextRecord, actual.OffsetToNextRecord, "OffsetToNextRecord is wrong");
            Assert.AreEqual(expected.MessageNumber, actual.MessageNumber, "MessageNumber is wrong");
            Assert.AreEqual(expected.ProcessID, expected.ProcessID, "ProcessID is wrong");
            Assert.AreEqual(expected.ThreadID, expected.ThreadID, "ThreadID is wrong");
            Assert.AreEqual(expected.EventFileTime, actual.EventFileTime, "EventFileTime is wrong");
            Assert.AreEqual(expected.LogFlag, actual.LogFlag, "LogFlag is wrong");
            Assert.AreEqual(expected.Component, actual.Component, "Component is wrong");
            Assert.AreEqual(expected.Message, actual.Message, "Message is wrong");
            Assert.AreEqual(expected.ProcessName, actual.ProcessName, "ProcessName is wrong");
            Assert.AreEqual(expected.SessionID, actual.SessionID, "SessionID is wrong");
            // This is not really a property of the source LogRecord, but of the reader
            //Assert.AreEqual(expected.HostFQDN, ExpectedFqdn, "HostFQDN is wrong");
        }
    }
}
