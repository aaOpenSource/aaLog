using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;


namespace aaLogReader.Tests.aaLgxReaderTests
{
  [TestFixture]
  public class aaLgxReaderTests
  {
        private const string ROOT_FILE_PATH = @"aaLgxReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string REF_FILE_PATH = ROOT_FILE_PATH + @"\refFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\test.aaLGX";

        [Test]
        public void CanReadHeader()
        {            
           LogHeader header = aaLgxReader.ReadLogHeader(LOG_FILE_INSTANCE);
           string compareJSON = System.IO.File.ReadAllText(Path.Combine(REF_FILE_PATH, "CanReadHeader.json"));
            Assert.AreEqual(header.ToJSON(), compareJSON);
        }

    [Test]
        public void ReadsCorrectNumberOfRecords()
        {
          var header = aaLgxReader.ReadLogHeader(LOG_FILE_INSTANCE);
          var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
          Assert.That(header.MsgCount, Is.GreaterThan(0));
          Assert.That(header.MsgCount, Is.EqualTo(records.Count()));
        }

    [Test]
        public void CanReadFirstRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var record = records.First();
            string compareJSON = System.IO.File.ReadAllText(Path.Combine(REF_FILE_PATH, "CanReadFirstRecord.json"));
            Assert.AreEqual(record.ToJSON(), compareJSON);
        }

        [Test]
        public void CanReadEighteenthRecord()
        {
            // Picked this one because it has a long-ish message and a warning flag.
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var record = records.Skip(17).Take(1).First();
            string compareJSON = System.IO.File.ReadAllText(Path.Combine(REF_FILE_PATH, "CanReadEighteenthRecord.json"));
            Assert.AreEqual(record.ToJSON(), compareJSON);
        }

        [Test]
        public void CanReadLastRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var record = records.Last();
            string compareJSON = System.IO.File.ReadAllText(Path.Combine(REF_FILE_PATH, "CanReadLastRecord.json"));
            Assert.AreEqual(record.ToJSON(), compareJSON);            
        }
    }
}