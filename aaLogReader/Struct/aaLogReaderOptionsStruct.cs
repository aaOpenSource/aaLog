using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace aaLogReader
{
    public class aaLogReaderOptionsStruct
    {
        public string LogDirectory = @"C:\ProgramData\ArchestrA\LogFiles";
        public string CacheFileBaseName = "aaLogReaderCache";
        public string CacheFileNameCustom = "";
        public bool CacheFileAppendProcessNameToBaseFileName = true;
        public bool IgnoreCacheFileOnFirstRead = false;
        public List<LogRecordFilterStruct> LogRecordPostFilters = new List<LogRecordFilterStruct>();
    }
}
