using System;
using Newtonsoft.Json;

namespace aaLogReader
{
    public class LogHeader
    {
        public ulong MsgStartingNumber;

        public ulong MsgCount;

        public ulong MsgLastNumber;

        public DateTime StartDateTime;

        public DateTime EndDateTime;

        public int OffsetFirstRecord;

        public int OffsetLastRecord;

        public string ComputerName;

        public string Session;

        public string PrevFileName;

        public string HostFQDN;

        [JsonIgnore]
        public ReturnCode ReturnCode;

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}