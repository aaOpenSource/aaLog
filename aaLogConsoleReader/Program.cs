using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aaLogReader;

namespace aaLogConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string answer;
            aaLogReader.aaLogReader logReader = new aaLogReader.aaLogReader();

            answer = "y";

            while (answer.ToLower() == "y")
            {
                Console.WriteLine("Read Unread Records (Y=Yes, N=Exit)");
                answer = Console.ReadLine();

                if (answer.ToLower() == "y")
                {
                    List<LogRecord> records = logReader.GetUnreadRecords(5);

                    Console.WriteLine("Record count : " + records.Count.ToString());

                    foreach (LogRecord lr in records)
                    {
                        Console.WriteLine(lr.ToKVP());
                    }
                }
            }
        }
    }
}
