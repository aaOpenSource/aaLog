using System;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace aaLogReader
{
    /// <summary>
    /// A standard log lastRecord
    /// </summary>
    public class LogRecord : ILogRecord
    {
        // Default constructor
        public LogRecord()
        {
            this.ReturnCode.Status = false;
            this.ReturnCode.Message = "";
        }

        [JsonIgnore]        
        public int RecordLength { get; set; }

        [JsonIgnore]        
        public int OffsetToPrevRecord { get; set; }

        [JsonIgnore]
        public int OffsetToNextRecord { get; set; }

        [Key]
        public ulong MessageNumber { get; set; }

        public int ProcessID { get; set; }

        public int ThreadID { get; set; }

        public ulong EventFileTime { get; set; }

        // TODO: Add UTC Offset for Exact Timestamp
        // public int EventUTCOffset; 

        public DateTime EventDateTime
        {
            get { return DateTime.FromFileTime((long)this.EventFileTime); }
        }

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

        public string LogFlag { get; set; }

        public string Component { get; set; }

        public string Message { get; set; }

        public string ProcessName { get; set; }

        public string SessionID { get; set; }

        public string HostFQDN { get; set; }

        [JsonIgnore]
        public ReturnCodeStruct ReturnCode;

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Return the lastRecord in the form of a Key-Value Pair
        /// </summary>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToKVP(ExportFormat format = ExportFormat.Full)
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


                if (format == ExportFormat.Full)
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

                    localSB.Append(", EventFileTime=");
                    localSB.Append(((char)34).ToString() + this.EventFileTime.ToString() + ((char)34).ToString());

                }
                returnValue = localSB.ToString();
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }
        
        /// <summary>
        /// Get a header for a series of log records with a delimiter
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private string localHeader(char Delimiter = ',', ExportFormat format = ExportFormat.Full)
        {
            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {

                localSB.Append("EventDateTime");
                localSB.Append(Delimiter + "LogFlag");
                localSB.Append(Delimiter + "Message");
                localSB.Append(Delimiter + "HostFQDN");


                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(Delimiter + "MessageNumber");
                    localSB.Append(Delimiter + "ProcessID");
                    localSB.Append(Delimiter + "ThreadID");
                    localSB.Append(Delimiter + "Component");
                    localSB.Append(Delimiter + "ProcessName");
                    localSB.Append(Delimiter + "SessionID");
                    localSB.Append(Delimiter + "EventFileTime");
                }

                returnValue = localSB.ToString();
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }
        
        public static string Header(char Delimiter = ',', ExportFormat format = ExportFormat.Full)
        {
            LogRecord lr = new LogRecord();
            return lr.localHeader(Delimiter, format);
        }

        public static string HeaderCSV(ExportFormat format = ExportFormat.Full)
        {
            return LogRecord.Header(',', format);
        }

        public static string HeaderTSV(ExportFormat format = ExportFormat.Full)
        {
            return LogRecord.Header('\t', format);
        }

        /// <summary>
        ///  Get the lastRecord in the form of a delimited string
        /// </summary>
        /// <param name="Delimiter">Delimiter to Use</param>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToDelimitedString(char Delimiter = ',', ExportFormat format = ExportFormat.Full)
        {

            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {

                localSB.Append(((char)34).ToString() + this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ((char)34).ToString());
                localSB.Append(Delimiter + this.LogFlag);
                localSB.Append(Delimiter + ((char)34).ToString() + this.Message + ((char)34).ToString());
                localSB.Append(Delimiter + this.HostFQDN);

                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(Delimiter + this.MessageNumber.ToString());
                    localSB.Append(Delimiter + this.ProcessID.ToString());
                    localSB.Append(Delimiter + this.ThreadID.ToString());
                    localSB.Append(Delimiter + ((char)34).ToString() + this.Component + ((char)34).ToString());
                    localSB.Append(Delimiter + ((char)34).ToString() + this.ProcessName + ((char)34).ToString());
                    localSB.Append(Delimiter + this.SessionID);
                    localSB.Append(Delimiter + this.EventFileTime.ToString());
                }

                returnValue = localSB.ToString();
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }

        public string ToCSV(ExportFormat format = ExportFormat.Full)
        {
            return this.ToDelimitedString(',', format);
        }

        public string ToTSV(ExportFormat format = ExportFormat.Full)
        {
            return this.ToDelimitedString('\t',format);
        }
    }
}