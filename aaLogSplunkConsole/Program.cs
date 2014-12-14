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
            /*
             * Arguments
             * Filename             Override the path to the log file if specfied
             * Maximum Messages     Override the default max messages to specify a larger or smaller value to return
             */

            List<LogRecord> logRecords = new List<LogRecord>();
            aaLogReader.aaLogReader logReader;

            //// All defaults
            //if(args.Length == 0)
            //{
                logReader = new aaLogReader.aaLogReader();
                logRecords = logReader.GetUnreadRecords();
            //}

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
