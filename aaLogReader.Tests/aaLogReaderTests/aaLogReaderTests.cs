using System.Linq;
using aaLogReader.Helpers;
using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;


namespace aaLogReader.Tests.aaLogReaderTests
{
    [TestFixture]
    public class aaLogReaderTests
    {
        private const string ROOT_FILE_PATH = @"aaLogReaderTests";
        private const string LOG_FILE_PATH = ROOT_FILE_PATH + @"\logFiles";
        private const string LOG_FILE_INSTANCE = LOG_FILE_PATH + @"\2014R2-VS-WSP1445626381.aaLog";

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


            string refHeaderJSON = File.ReadAllText(ROOT_FILE_PATH + @"\refFiles\2014R2-VS-WSP1445626381.header.json");
            string thisHeaderJSON = JsonConvert.SerializeObject(alr.CurrentLogHeader);

            Assert.AreEqual(refHeaderJSON, thisHeaderJSON);
        }

        /// <summary>
        /// Verify log file can be opened and closed without error
        /// </summary>
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

        /// <summary>
        /// Verify log file can be opened and closed without error
        /// </summary>
        [Test]
        public void CurrentLogFilePath()
        {
            aaLogReader alr = new aaLogReader();

            ReturnCodeStruct rcs = alr.OpenLogFile(LOG_FILE_INSTANCE);

            Assert.That(rcs.Status);
            Assert.That(rcs.Message.Length == 0);

            Assert.AreEqual(alr.CurrentLogFilePath, LOG_FILE_INSTANCE);
            
        }
    }
}