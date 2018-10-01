// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using aaLogReader;

namespace SMCtoSplunkHTTP.Helpers
{
    /// <summary>
    /// Extension methods for reading different data types from streams.
    /// </summary>ring
    public static class LogReaderExtensions
    {
        /// <summary>
        /// Render the LogRecords list rows to multi-line Key-Value pair
        /// </summary>
        /// <param name="List<LogRecord>"></param>
        /// <returns></returns>
        public static string ToKVP(this List<LogRecord> logrecords, String additionalKVPValues = "")
        {
            var returnValue = new StringBuilder();

            if (logrecords.Count > 0)
            {
                foreach (LogRecord logRecord in logrecords)
                {
                    returnValue.Append(logRecord.ToKVP(ExportFormat.Full));

                    // Append the additional values             
                    if (additionalKVPValues.Length > 0)
                    {
                        returnValue.Append(additionalKVPValues);
                    }

                    //Trim any trailing commas
                    returnValue.Remove(returnValue.Length - 2, 1);

                    returnValue.AppendLine();
                }
            }
            return returnValue.ToString();
        }
    }
}
