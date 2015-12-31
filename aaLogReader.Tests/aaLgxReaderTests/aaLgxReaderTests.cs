using System.Linq;
using NUnit.Framework;

namespace aaLogReader.Tests.aaLgxReaderTests
{
    [TestFixture]
    public class aaLgxReaderTests : aaLogBaseTest
    {
        private const string ROOT_FILE_PATH = @"aaLgxReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\test.aaLGX";

        static aaLgxReaderTests()
        {
            RootFilePath = ROOT_FILE_PATH;
        }

        [Test]
        public void CanReadHeader()
        {
            var actual = aaLgxReader.ReadLogHeader(LOG_FILE_INSTANCE);
            var expected = new LogHeader
            {
                LogFilePath = LOG_FILE_INSTANCE,
                StartMsgNumber = 0,
                MsgCount = 683,
                StartFileTime = 130783241810947628,
                EndFileTime = 130783316984931276,
                OffsetFirstRecord = 68,
                OffsetLastRecord = 191760,
                ComputerName = "DSA2014R2",
                Session = null,
                PrevFileName = null,
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void ReadsCorrectNumberOfRecords()
        {
            var header = aaLgxReader.ReadLogHeader(LOG_FILE_INSTANCE);
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            Assert.That(header.MsgCount, Is.GreaterThan(0), "MsgCount should not be zero");
            Assert.That(header.MsgCount, Is.EqualTo(records.Count()), "MsgCount does not match records count");
        }

        [Test]
        public void CanReadFirstRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var actual = records.First();
            var expected = new LogRecord
            {
                MessageNumber = 360097,
                RecordLength = 106,
                OffsetToPrevRecord = 0,
                OffsetToNextRecord = 174,
                ProcessID = 1076,
                ProcessName = "",
                SessionID = "0.0.0.0",
                ThreadID = 1096,
                EventFileTime = 130783241810947628,
                LogFlag = "Info",
                Component = "aaLogger",
                Message = "Logger Started.",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void CanReadEighteenthRecord()
        {
            // Picked this one because it has a long-ish message and a warning flag.
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var actual = records.Skip(17).Take(1).First();
            var expected = new LogRecord
            {
                MessageNumber = 360114,
                RecordLength = 1076,
                OffsetToPrevRecord = 6076,
                OffsetToNextRecord = 7492,
                ProcessID = 1164,
                ProcessName = "aaInformationModelService",
                SessionID = "25.46.254.255",
                ThreadID = 1196,
                EventFileTime = 130783242256451623,
                LogFlag = "Warning",
                Component = "IMS.Server.Runtime",
                Message = @"A connection attempt to the database server on machine DSA2014R2 failed (attempt 1 of 10). Will try again in 30 seconds.
A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }

        [Test]
        public void CanReadLastRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var actual = records.Last();
            var expected = new LogRecord
            {
                MessageNumber = 360779,
                RecordLength = 254,
                OffsetToPrevRecord = 191470,
                OffsetToNextRecord = 192014,
                ProcessID = 7912,
                ProcessName = "aahIDASSvc",
                SessionID = "74.119.223.42",
                ThreadID = 6712,
                EventFileTime = 130783316984931276,
                LogFlag = "Info",
                Component = "aahIDASSvc (local)",
                Message = "DSA2014R2_2: Sending SuiteLink time synchronization message DSA2014R2",
                HostFQDN = ExpectedFqdn,
            };
            AreEqual(expected, actual);
        }
    }
}