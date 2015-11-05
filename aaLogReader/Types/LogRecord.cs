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

        /// <summary>
        /// Return the record in the form of a Key-Value Pair
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
                localSB.Append("\"" + this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\"");

                localSB.Append(", LogFlag=");
                localSB.Append("\"" + this.LogFlag + "\"");

                localSB.Append(", Message=");
                localSB.Append("\"" + this.Message + "\"");

                localSB.Append(", HostFQDN=");
                localSB.Append("\"" + this.HostFQDN + "\"");


                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(", MessageNumber=");
                    localSB.Append("\"" + this.MessageNumber.ToString() + "\"");

                    localSB.Append(", ProcessID=");
                    localSB.Append("\"" + this.ProcessID.ToString() + "\"");

                    localSB.Append(", ThreadID=");
                    localSB.Append("\"" + this.ThreadID.ToString() + "\"");

                    localSB.Append(", Component=");
                    localSB.Append("\"" + this.Component + "\"");

                    localSB.Append(", ProcessName=");
                    localSB.Append("\"" + this.ProcessName + "\"");

                    localSB.Append(", SessionID=");
                    localSB.Append("\"" + this.SessionID + "\"");

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
        ///  Get the record in the form of a delimited string
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

                localSB.Append(this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                localSB.Append(Delimiter + this.LogFlag);
                localSB.Append(Delimiter + "\"" + this.Message + "\"");
                localSB.Append(Delimiter + this.HostFQDN);

                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(Delimiter +this.MessageNumber.ToString());
                    localSB.Append(Delimiter +this.ProcessID.ToString());
                    localSB.Append(Delimiter +this.ThreadID.ToString());
                    localSB.Append(Delimiter + "\"" + this.Component + "\"");
                    localSB.Append(Delimiter + "\"" + this.ProcessName + "\"");
                    localSB.Append(Delimiter +this.SessionID);
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

        public enum ExportFormat
        {
             Full=1
            ,Minimal = 2
        }
    }
}