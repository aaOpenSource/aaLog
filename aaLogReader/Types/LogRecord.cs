using System;
using Newtonsoft.Json;
using System.Text;

namespace aaLogReader
{
    /// <summary>
    /// A standard log record
    /// </summary>
    public class LogRecord
    {
        [JsonIgnore]
        public int RecordLength;

        [JsonIgnore]
        public int OffsetToPrevRecord;

        [JsonIgnore]
        public int OffsetToNextRecord;

        public ulong MessageNumber;

        public int ProcessID;

        public int ThreadID;

        public DateTime EventDateTime;

        [JsonIgnore]
        public DateTime EventDate
        {
            get
            {
                return this.EventDateTime.Date;
            }
        }

        [JsonIgnore]
        public string EventTime
        {
            get { return this.EventDateTime.ToString("hh:mm:ss.fff tt"); }
        }

        [JsonIgnore]
        public int EventMillisec
        {
            get { return this.EventDateTime.Millisecond;}
        }

        public string LogFlag;

        public string Component;

        public string Message;

        public string ProcessName;

        public string SessionID;

        public string HostFQDN;

        [JsonIgnore]
        public ReturnCode ReturnCode;

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToKVP(KVPFormat format = KVPFormat.Full)
        {
            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {                
                
                localSB.Append("Timestamp=");
                localSB.Append(this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                localSB.Append(", LogFlag=");
                localSB.Append(((char)34).ToString() + this.LogFlag + ((char)34).ToString());

                localSB.Append(", Message=");
                localSB.Append(((char)34).ToString() + this.Message + ((char)34).ToString());

                localSB.Append(", HostFQDN=");
                localSB.Append(((char)34).ToString() + this.HostFQDN + ((char)34).ToString());


                if (format == KVPFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(", MessageNumber=");
                    localSB.Append(((char)34).ToString() + this.MessageNumber.ToString() + ((char)34).ToString());

                    localSB.Append(", ProcessID=");
                    localSB.Append(((char)34).ToString() + this.ProcessID.ToString() + ((char)34).ToString());

                    localSB.Append(", ThreadID=");
                    localSB.Append(((char)34).ToString() + this.ThreadID.ToString() + ((char)34).ToString());

                    localSB.Append(", Component=");
                    localSB.Append(((char)34).ToString() + this.Component + ((char)34).ToString());

                    localSB.Append(", ProcessName=");
                    localSB.Append(((char)34).ToString() + this.ProcessName + ((char)34).ToString());

                    localSB.Append(", SessionID=");
                    localSB.Append(((char)34).ToString() + this.SessionID + ((char)34).ToString());

                }

                returnValue = localSB.ToString();
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;

        }

        public enum KVPFormat
        {
             Full=1
            ,Minimal = 2
        }
    }
}