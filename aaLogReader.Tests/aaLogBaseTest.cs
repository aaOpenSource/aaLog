using aaLogReader.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;

namespace aaLogReader.Tests
{
    public class aaLogBaseTest
    {
        public static readonly string TEST_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        protected readonly string ROOT_FILE_PATH = "INVALID$DIRECTORY";

        private readonly string _expectedFqdn;

        public aaLogBaseTest(string rootFilePath)
        {
            //ROOT_FILE_PATH = Path.Combine(TEST_PATH, rootFilePath);
            ROOT_FILE_PATH = rootFilePath;
            _expectedFqdn = Fqdn.GetFqdn();
        }

        public string ExpectedFqdn
        {
            get { return _expectedFqdn; }
        }

        public void AreEqual(string fileName, LogHeader actualObj, string optionalMessage = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(ROOT_FILE_PATH, fileName));
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
            // This is not really a property of the source LogRecord, but of the reader
            //Assert.AreEqual(expected.HostFQDN, ExpectedFqdn, "HostFQDN is wrong");
        }

        public void AreEqual(string fileName, LogRecord actualObj, string optionalMessage = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(ROOT_FILE_PATH, fileName));
            var expectedObj = JsonConvert.DeserializeObject<LogRecord>(expectedText);
            AreEqual(expectedObj, actualObj, optionalMessage);
        }

        public void AreEqual(LogRecord expected, LogRecord actual, string optionalMessage = null)
        {
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
