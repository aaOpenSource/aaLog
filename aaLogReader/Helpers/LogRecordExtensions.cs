using System.IO;
using aaLogReader;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;


namespace aaLogReader.Helpers
{
    /// <summary>
    /// Extension methods for reading different data types from streams.
    /// </summary>ring
    public static class LogRecordExtensions
    {
        public static string ToJSON(this List<LogRecord> records)
        {
            return JsonConvert.SerializeObject(records);
        }

        public static string ToKVP(this List<LogRecord> records)
        {
            StringBuilder returnValue;

            returnValue = new StringBuilder();

            foreach(LogRecord record in records)
            {
                returnValue.AppendLine(record.ToKVP());
            }

            return returnValue.ToString();
        }
    }
}
