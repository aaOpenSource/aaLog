using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace aaLogReader
{
    public class Options
    {
        public string LogDirectory = @"C:\ProgramData\ArchestrA\LogFiles";
        public string CacheFileBaseName = "aaLogReaderCache";
        public string CacheFileNameCustom = "";
        public bool CacheFileAppendProcessNameToBaseFileName = true;

        public DateTime EarliestDateTime = DateTime.MinValue;
        public DateTime LatestDate = DateTime.MaxValue;
    }
}
