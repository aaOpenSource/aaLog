using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aaLogReader;

namespace aaLogSplunkConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader();
            List<LogRecord> logRecords = logReader.GetUnreadRecords();

            // If we have any records then output kvp format to the console
            if(logRecords.Count > 0)
            {
                foreach(LogRecord record in logRecords)
                {
                    Console.WriteLine(record.ToKVP());
                }
            }

        }
    }
}
