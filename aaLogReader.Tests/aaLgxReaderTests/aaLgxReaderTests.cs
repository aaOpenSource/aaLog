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
      Assert.That(header.LogFilePath, Is.EqualTo(LOG_FILE_PATH));
      Assert.That(header.StartMsgNumber, Is.EqualTo(0));
      Assert.That(header.MsgCount, Is.EqualTo(683));
      Assert.That(header.EndMsgNumber, Is.EqualTo(682));
      Assert.That(header.StartFileTime, Is.EqualTo(130783241810947628));
      Assert.That(header.EndFileTime, Is.EqualTo(130783316984931276));
      Assert.That(header.OffsetFirstRecord, Is.EqualTo(68));
      Assert.That(header.OffsetLastRecord, Is.EqualTo(191760));
      Assert.That(header.ComputerName, Is.EqualTo("DSA2014R2"));
      Assert.That(header.Session, Is.Null);
      Assert.That(header.PrevFileName, Is.Null);
      Assert.That(header.HostFQDN, Is.Null);
    }

    [Test]
    public void ReadsCorrectNumberOfRecords()
    {
      var header = aaLgxReader.ReadLogHeader(LOG_FILE_PATH);
      var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
      Assert.That(header.MsgCount, Is.GreaterThan(0));
      Assert.That(header.MsgCount, Is.EqualTo(records.Count()));
    }

    [Test]
    public void CanReadFirstRecord()
    {
      var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
      var record = records.First();
      Assert.That(record.RecordLength, Is.EqualTo(106));
      Assert.That(record.OffsetToPrevRecord, Is.EqualTo(0));
      Assert.That(record.OffsetToNextRecord, Is.EqualTo(174));
      Assert.That(record.MessageNumber, Is.EqualTo(360097));
      Assert.That(record.SessionID, Is.EqualTo("0.0.0.0"));
      Assert.That(record.ProcessID, Is.EqualTo(1076));
      Assert.That(record.ThreadID, Is.EqualTo(1096));
      Assert.That(record.EventFileTime, Is.EqualTo(130783241810947628));
      Assert.That(record.LogFlag, Is.EqualTo("Info"));
      Assert.That(record.Component, Is.EqualTo("aaLogger"));
      Assert.That(record.Message, Is.EqualTo("Logger Started."));
      Assert.That(record.ProcessName, Is.EqualTo(""));
      Assert.That(record.HostFQDN, Is.Null);
    }

    [Test]
    public void CanReadEighteenthRecord()
    {
      // Picked this one because it has a long-ish message and a warning flag.
      var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
      var record = records.Skip(17).Take(1).First();
      Assert.That(record.RecordLength, Is.EqualTo(1076));
      Assert.That(record.OffsetToPrevRecord, Is.EqualTo(6076));
      Assert.That(record.OffsetToNextRecord, Is.EqualTo(7492));
      Assert.That(record.MessageNumber, Is.EqualTo(360114));
      Assert.That(record.SessionID, Is.EqualTo("25.46.254.255"));
      Assert.That(record.ProcessID, Is.EqualTo(1164));
      Assert.That(record.ThreadID, Is.EqualTo(1196));
      Assert.That(record.EventFileTime, Is.EqualTo(130783242256451623));
      Assert.That(record.LogFlag, Is.EqualTo("Warning"));
      Assert.That(record.Component, Is.EqualTo("IMS.Server.Runtime"));
      Assert.That(record.Message, Is.EqualTo(@"A connection attempt to the database server on machine DSA2014R2 failed (attempt 1 of 10). Will try again in 30 seconds.
A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"));
      Assert.That(record.ProcessName, Is.EqualTo("aaInformationModelService"));
      Assert.That(record.HostFQDN, Is.Null);
    }

    [Test]
    public void CanReadLastRecord()
    {
      var records = aaLgxReader.ReadLogRecords(LOG_FILE_PATH);
      var record = records.Last();
      Assert.That(record.RecordLength, Is.EqualTo(254));
      Assert.That(record.OffsetToPrevRecord, Is.EqualTo(191470));
      Assert.That(record.OffsetToNextRecord, Is.EqualTo(192014));
      Assert.That(record.MessageNumber, Is.EqualTo(360779));
      Assert.That(record.SessionID, Is.EqualTo("74.119.223.42"));
      Assert.That(record.ProcessID, Is.EqualTo(7912));
      Assert.That(record.ThreadID, Is.EqualTo(6712));
      Assert.That(record.EventFileTime, Is.EqualTo(130783316984931276));
      Assert.That(record.LogFlag, Is.EqualTo("Info"));
      Assert.That(record.Component, Is.EqualTo("aahIDASSvc (local)"));
      Assert.That(record.Message, Is.EqualTo("DSA2014R2_2: Sending SuiteLink time synchronization message DSA2014R2"));
      Assert.That(record.ProcessName, Is.EqualTo("aahIDASSvc"));
      Assert.That(record.HostFQDN, Is.Null);
    }
  }
}