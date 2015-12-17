using System.Linq;
using NUnit.Framework;

namespace aaLogReader.Tests.aaLgxReaderTests
{
    [TestFixture]
    public class aaLgxReaderTests
    {
        private const string LOG_FILE_PATH = @"aaLgxReaderTests\logFiles\test.aaLGX";

        [Test]
        public void CanReadHeader()
        {
            var header = aaLgxReader.ReadLogHeader(LOG_FILE_PATH);
            Assert.That(header.LogFilePath, Is.EqualTo(LOG_FILE_PATH), "LogFilePath is wrong");
            Assert.That(header.StartMsgNumber, Is.EqualTo(0), "StartMsgNumber is wrong");
            Assert.That(header.MsgCount, Is.EqualTo(683), "MsgCount is wrong");
            Assert.That(header.EndMsgNumber, Is.EqualTo(682), "EndMsgNumber is wrong");
            Assert.That(header.StartFileTime, Is.EqualTo(130783241810947628), "StartFileTime is wrong");
            Assert.That(header.EndFileTime, Is.EqualTo(130783316984931276), "EndFileTime is wrong");
            Assert.That(header.OffsetFirstRecord, Is.EqualTo(68), "OffsetFirstRecord is wrong");
            Assert.That(header.OffsetLastRecord, Is.EqualTo(191760), "OffsetLastRecord is wrong");
            Assert.That(header.ComputerName, Is.EqualTo("DSA2014R2"), "ComputerName is wrong");
            Assert.That(header.Session, Is.Null, "Session is wrong");
            Assert.That(header.PrevFileName, Is.Null, "PrevFileName is wrong");
            Assert.That(header.HostFQDN, Is.Null, "HostFQDN is wrong");
        }

        [Test]
        public void ReadsCorrectNumberOfRecords()
        {
            var header = aaLgxReader.ReadLogHeader(LOG_FILE_PATH);
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
            Assert.That(header.MsgCount, Is.GreaterThan(0), "MsgCount should not be zero");
            Assert.That(header.MsgCount, Is.EqualTo(records.Count()), "MsgCount does not match records count");
        }

        [Test]
        public void CanReadFirstRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
            var record = records.First();
            Assert.That(record.RecordLength, Is.EqualTo(106), "RecordLength is wrong");
            Assert.That(record.OffsetToPrevRecord, Is.EqualTo(0), "OffsetToPrevRecord is wrong");
            Assert.That(record.OffsetToNextRecord, Is.EqualTo(174), "OffsetToNextRecord is wrong");
            Assert.That(record.MessageNumber, Is.EqualTo(360097), "MessageNumber is wrong");
            Assert.That(record.SessionID, Is.EqualTo("0.0.0.0"), "SessionID is wrong");
            Assert.That(record.ProcessID, Is.EqualTo(1076), "ProcessID is wrong");
            Assert.That(record.ThreadID, Is.EqualTo(1096), "ThreadID is wrong");
            Assert.That(record.EventFileTime, Is.EqualTo(130783241810947628), "EventFileTime is wrong");
            Assert.That(record.LogFlag, Is.EqualTo("Info"), "LogFlag is wrong");
            Assert.That(record.Component, Is.EqualTo("aaLogger"), "Component is wrong");
            Assert.That(record.Message, Is.EqualTo("Logger Started."), "Message is wrong");
            Assert.That(record.ProcessName, Is.EqualTo(""), "ProcessName is wrong");
            Assert.That(record.HostFQDN, Is.Null, "HostFQDN is wrong");
        }

        [Test]
        public void CanReadEighteenthRecord()
        {
            // Picked this one because it has a long-ish message and a warning flag.
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
            var record = records.Skip(17).Take(1).First();
            Assert.That(record.RecordLength, Is.EqualTo(1076), "RecordLength is wrong");
            Assert.That(record.OffsetToPrevRecord, Is.EqualTo(6076), "OffsetToPrevRecord is wrong");
            Assert.That(record.OffsetToNextRecord, Is.EqualTo(7492), "OffsetToNextRecord is wrong");
            Assert.That(record.MessageNumber, Is.EqualTo(360114), "MessageNumber is wrong");
            Assert.That(record.SessionID, Is.EqualTo("25.46.254.255"), "SessionID is wrong");
            Assert.That(record.ProcessID, Is.EqualTo(1164), "ProcessID is wrong");
            Assert.That(record.ThreadID, Is.EqualTo(1196), "ThreadID is wrong");
            Assert.That(record.EventFileTime, Is.EqualTo(130783242256451623), "EventFileTime is wrong");
            Assert.That(record.LogFlag, Is.EqualTo("Warning"), "LogFlag is wrong");
            Assert.That(record.Component, Is.EqualTo("IMS.Server.Runtime"), "Component is wrong");
            Assert.That(record.Message,
                Is.EqualTo(
                    @"A connection attempt to the database server on machine DSA2014R2 failed (attempt 1 of 10). Will try again in 30 seconds.
A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"),
                "Message is wrong");
            Assert.That(record.ProcessName, Is.EqualTo("aaInformationModelService"), "ProcessName is wrong");
            Assert.That(record.HostFQDN, Is.Null, "HostFQDN is wrong");
        }

        [Test]
        public void CanReadLastRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
            var record = records.Last();
            Assert.That(record.RecordLength, Is.EqualTo(254), "RecordLength is wrong");
            Assert.That(record.OffsetToPrevRecord, Is.EqualTo(191470), "OffsetToPrevRecord is wrong");
            Assert.That(record.OffsetToNextRecord, Is.EqualTo(192014), "OffsetToNextRecord is wrong");
            Assert.That(record.MessageNumber, Is.EqualTo(360779), "MessageNumber is wrong");
            Assert.That(record.SessionID, Is.EqualTo("74.119.223.42"), "SessionID is wrong");
            Assert.That(record.ProcessID, Is.EqualTo(7912), "ProcessID is wrong");
            Assert.That(record.ThreadID, Is.EqualTo(6712), "ThreadID is wrong");
            Assert.That(record.EventFileTime, Is.EqualTo(130783316984931276), "EventFileTime is wrong");
            Assert.That(record.LogFlag, Is.EqualTo("Info"), "LogFlag is wrong");
            Assert.That(record.Component, Is.EqualTo("aahIDASSvc (local)"), "Component is wrong");
            Assert.That(record.Message,
                Is.EqualTo("DSA2014R2_2: Sending SuiteLink time synchronization message DSA2014R2"),
                "Message is wrong");
            Assert.That(record.ProcessName, Is.EqualTo("aahIDASSvc"), "ProcessName is wrong");
            Assert.That(record.HostFQDN, Is.Null, "HostFQDN is wrong");
        }
    }
}