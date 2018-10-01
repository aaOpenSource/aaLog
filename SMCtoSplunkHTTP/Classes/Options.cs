// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel;
using aaLogReader;

namespace SMCtoSplunkHTTP
{
    /// <summary>
    /// The otpions to be
    /// </summary>
    public class Options
    {
        [Description("Time interval in milliseconds to read data from server")]
        public int ReadInterval = 5000;

        [Description("Maximum interval used when backoff timer slows down due to bad connections")]
        public int MaximumReadInterval = 50000;

        [Description("Maximum number of records to retrieve in a single read of the database when use the {{MaxRecords}} element in the base query.  If the available records based on cache data exceeds this value, then only the first XX records as sorted in descending order according to the sequence field will be retrieved.")]    
        public ulong MaxRecords = 1000;
                
        [Description("Base HTTP address including port number")]
        public string SplunkBaseAddress = "http://localhost:8088";

        [Description("HTTP event collector authorization token")]
        public string SplunkAuthorizationToken;
        
        [Description("Unique client ID - will be generated automatically in the event one is not specified")]
        public Guid SplunkClientID = Guid.NewGuid();

        [Description("Ignore SSL errors")]
        public bool SplunkIgnoreSSLErrors = false; // TODO

        [Description("Name of the host that is the source of data - defaults to current machine name")]
        public string SplunkSourceHost = Environment.MachineName;

        [Description("Unique name of the data source that can be used for searching within Splunk")]
        public string SplunkSourceData;

        [Description("Timestamp format for writing event timestamp to Splunk - can be any legal format that can be used with string.format")]
        public string SplunkEventTimestampFormat = "yyyy-MM-dd HH:mm:ss.ffffff zz";

        [Description("Filename for the cache file written to disk to persist details about the last record successfully transmitted to SPLUNNK")]
        public string CacheFilename;

        [Description("Directory where aalog files are located.")]
        public string LogDirectory = @"C:\ProgramData\ArchestrA\LogFiles";

        [Description("Directory where aalgx files are located waiting to be processed.")]
        public string AALGXDirectory = @"";

        //[Description("Base filename for cache file.")]
        //public string CacheFileBaseName = "aaLogReaderCache";

        //[Description("Custom static filename for cache file.")]
        //public string CacheFileNameCustom = "";

        //[Description("Append the process name to cache file base name for uniqueness.")]
        //public bool CacheFileAppendProcessNameToBaseFileName = true;

        //[Description("Ignore the cache on first read and read all entries up to maxium specificed.")]
        //public bool IgnoreCacheFileOnFirstRead = false;

        [Description("List of filters for specifying specifc log records to retrieve.")]
        public List<LogRecordFilterStruct> LogRecordPostFilters = new List<LogRecordFilterStruct>();

    }
}