using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aaLogReader;


namespace aaLogReader.Testing
{
    public class HeaderTesting
    {
        public static void TestHeaderIndexingSearches(aaLogReader logreader, bool includePassingTests = false)
        {
            // Get Header Index
            List<LogHeader> localLogHeaderIndex = logreader.LogHeaderIndex;
            string msgBase;
            ulong RandomUlong;
            System.Random rnd = new Random(DateTime.Now.Millisecond);

                WriteMessage("Starting Test", ConsoleColor.Cyan);
                foreach (LogHeader localHeader in localLogHeaderIndex)
                {
                    try
                    {
                        WriteMessage(string.Format("Starting Test {0}",localHeader.LogFilePath), ConsoleColor.Cyan);

                        WriteMessage("Starting Message Number Test", ConsoleColor.Cyan);
                        msgBase = "Start Message in " + localHeader.LogFilePath;
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageNumber(localHeader.MsgStartingNumber) == localHeader.LogFilePath, includePassingTests);

                        msgBase = "Last Message in " + localHeader.LogFilePath;
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageNumber(localHeader.MsgLastNumber) == localHeader.LogFilePath, includePassingTests);

                        RandomUlong = (ulong)rnd.Next(Convert.ToInt32(localHeader.MsgCount)) + localHeader.MsgStartingNumber;
                        msgBase = string.Format("Random Message {0} in {1}", RandomUlong, localHeader.LogFilePath);
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageNumber(RandomUlong) == localHeader.LogFilePath, includePassingTests);
                        WriteMessage("Completed Message Number Test", ConsoleColor.Cyan);

                        WriteMessage("Starting Time Test", ConsoleColor.Cyan);
                        msgBase = "Start Time in " + localHeader.LogFilePath;
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageTimestamp(localHeader.StartFileTime) == localHeader.LogFilePath, includePassingTests);

                        msgBase = "Last Time in " + localHeader.LogFilePath;
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageTimestamp(localHeader.EndFileTime) == localHeader.LogFilePath, includePassingTests);

                        RandomUlong = (ulong)rnd.Next(10000) + localHeader.StartFileTime;
                        msgBase = string.Format("Random FileTime {0} in {1}", RandomUlong, localHeader.LogFilePath);
                        WriteResultMessage(msgBase, logreader.GetLogFilePathForMessageTimestamp(RandomUlong) == localHeader.LogFilePath, includePassingTests);

                        WriteMessage("Completed Time Test", ConsoleColor.Cyan);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                WriteMessage("Completed All Tests", ConsoleColor.Cyan);            
        }

        private static void WriteMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteResultMessage(string msgBase, bool result, bool includePassingTests = false)
        {
            if (result)
           {
               if (includePassingTests)
               {
                   Console.ForegroundColor = ConsoleColor.Green;
                   Console.WriteLine("passed" + '\t' + msgBase);
                   Console.ResetColor();
               }
           }
           else
           {
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("FAILED" + '\t' + msgBase);
               Console.ResetColor();
           }
        }
    }
}
