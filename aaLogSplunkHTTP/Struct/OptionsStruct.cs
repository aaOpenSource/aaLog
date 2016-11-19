using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using aaLogReader;

namespace aaLogSplunkHTTP
{
    public class OptionsStruct : aaLogReader.OptionsStruct
    {        
        public int ReadInterval = 5000;
        public int MaximumReadInterval = 50000;
        public ulong MaxUnreadRecords = 1000;
        public string SplunkBaseAddress = "http://localhost:8088";
        public string AuthorizationToken = "ADD81F16-6D0D-4803-82C9-8A959A311A4B";
        public Guid ClientID = Guid.NewGuid();
    }
}
