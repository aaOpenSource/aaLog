using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;

namespace aaLogReader.Tests
{
    public class aaLogBaseTest
    {
        public static readonly string TEST_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        protected readonly string ROOT_FILE_PATH = "INVALID$DIRECTORY";

        public aaLogBaseTest(string rootFilePath)
        {
            ROOT_FILE_PATH = Path.Combine(TEST_PATH, rootFilePath);
        }

        /// <summary>
        /// Removes system specific entries from JSON objects
        /// </summary>
        /// <param name="obj"></param>
        public void Cleanse(JObject obj)
        {
            obj.Remove("EventDateTime");
            obj.Remove("StartDateTime");
            obj.Remove("EndDateTime");
            obj.Remove("HostFQDN");
        }

        public void Compare<T>(string fileName, T actualObj, string message = null)
        {
            Compare<T>(ROOT_FILE_PATH, fileName, actualObj, message);
        }

        public void Compare<T>(string path, string fileName, T actualObj, string message = null)
        {
            var actualJson = JObject.FromObject(actualObj);
            Compare(path, fileName, actualJson, message);
        }

        /// <summary>
        /// Compares an actual JSON object result to a reference JSON file
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="fileName"></param>
        /// <param name="actualJson"></param>
        public void Compare(string fileName, JObject actualJson, string message = null)
        {
            Compare(ROOT_FILE_PATH, fileName, actualJson, message);
        }

        /// <summary>
        /// Compares an actual JSON object result to a reference JSON file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="actualJson"></param>
        /// <param name="message"></param>
        public void Compare(string path, string fileName, JObject actualJson, string message = null)
        {
            string expectedText = File.ReadAllText(Path.Combine(path, fileName));
            var expectedJson = JObject.Parse(expectedText);

            Cleanse(actualJson);
            Cleanse(expectedJson);

            Assert.AreEqual(expectedJson, actualJson, message);
        }
    }
}
