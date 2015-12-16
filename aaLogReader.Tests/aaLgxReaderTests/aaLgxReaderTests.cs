using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace aaLogReader.Tests.aaLgxReaderTests
{
    [TestFixture]
    public class aaLgxReaderTests : aaLogBaseTest
    {
        public readonly string LOG_FILE_PATH;
        public readonly string REF_FILE_PATH;
        public readonly string LOG_FILE_INSTANCE;

        public static string GetRootFilePath
        {
            get { return Path.Combine(TEST_PATH, @"aaLgxReaderTests"); }
        }

        public aaLgxReaderTests()
            : base(GetRootFilePath)
        {
            LOG_FILE_PATH = Path.Combine(ROOT_FILE_PATH, @"logFiles");
            REF_FILE_PATH = Path.Combine(ROOT_FILE_PATH, @"refFiles");
            LOG_FILE_INSTANCE = Path.Combine(LOG_FILE_PATH, @"test.aaLGX");
        }

        [Test]
        public void CanReadHeader()
        {
            LogHeader actualHeader = aaLgxReader.ReadLogHeader(LOG_FILE_INSTANCE);
            Compare(@"refFiles\CanReadHeader.json", actualHeader);
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
            Compare(@"refFiles\CanReadFirstRecord.json", record);
        }

        [Test]
        public void CanReadEighteenthRecord()
        {
            // Picked this one because it has a long-ish message and a warning flag.
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var record = records.Skip(17).Take(1).First();
            Compare(@"refFiles\CanReadEighteenthRecord.json", record);
        }

        [Test]
        public void CanReadLastRecord()
        {
            var records = aaLgxReader.ReadLogRecords(LOG_FILE_INSTANCE);
            var record = records.Last();
            Compare(@"refFiles\CanReadLastRecord.json", record);
        }
    }
}