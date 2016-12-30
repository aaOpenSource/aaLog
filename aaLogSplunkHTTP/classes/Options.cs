// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Newtonsoft.Json;
//using aaLogReader;
using System.ComponentModel;

namespace aaLogSplunkHTTP
{
    public class Options : aaLogReader.OptionsStruct
    {

        [Description("Time interval in milliseconds to read data from server")]
        public int ReadInterval = 5000;

        [Description("Maximum interval used when backoff timer slows down due to bad connections")]
        public int MaximumReadInterval = 50000;

        public ulong MaxUnreadRecords = 1000;

        //public string SplunkBaseAddress = "http://localhost:8088";
        //public string AuthorizationToken = "ADD81F16-6D0D-4803-82C9-8A959A311A4B";

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

        [Description("Custom cache file filename")]
        public string CacheFilename;

    }
}
