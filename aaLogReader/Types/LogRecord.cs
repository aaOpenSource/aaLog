using System;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace aaLogReader
{
    /// <summary>
    /// A standard log record
    /// </summary>
    public class LogRecord : ILogRecord
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public uint ProcessID { get; set; }

        public uint ThreadID { get; set; }

        private ulong _eventFileTime;
        private DateTimeOffset _eventDateTime;

        public ulong EventFileTime
        {
            get { return _eventFileTime; }
            set
            {
                _eventFileTime = value;
                _eventDateTime = DateTimeOffset.FromFileTime((long)value);
            }
        }

        // TODO: Add UTC Offset for Exact Timestamp
        // public int EventUTCOffset;

        public DateTimeOffset EventDateTime
        {
            get { return _eventDateTime; }
        }

        [JsonIgnore]
        public DateTime EventDateTimeLocal
        {
            get { return _eventDateTime.LocalDateTime; }
        }

        [JsonIgnore]
        public DateTime EventDateTimeUtc
        {
            get { return _eventDateTime.UtcDateTime; }
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
            get { return this.EventDateTime.Millisecond; }
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
        /// Return the log record in the form of a Key-Value Pair
        /// </summary>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToKVP(ExportFormat format = ExportFormat.Full)
        {
            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {
                localSB.AppendFormat("Timestamp=\"{0}\"", this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                localSB.AppendFormat(", LogFlag=\"{0}\"", this.LogFlag);
                localSB.AppendFormat(", Message=\"{0}\"", this.Message);
                localSB.AppendFormat(", HostFQDN=\"{0}\"", this.HostFQDN);

                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.AppendFormat(", MessageNumber=\"{0}\"", this.MessageNumber);
                    localSB.AppendFormat(", ProcessID=\"{0}\"", this.ProcessID);
                    localSB.AppendFormat(", ThreadID=\"{0}\"", this.ThreadID);
                    localSB.AppendFormat(", Component=\"{0}\"", this.Component);
                    localSB.AppendFormat(", ProcessName=\"{0}\"", this.ProcessName);
                    localSB.AppendFormat(", SessionID=\"{0}\"", this.SessionID);
                    localSB.AppendFormat(", EventFileTime=\"{0}\"", this.EventFileTime);
                }
                returnValue = localSB.ToString();
            }
            catch (Exception ex)
            {
                LogException(ex);
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
            catch (Exception ex)
            {
                LogException(ex);
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
        ///  Get the log record in the form of a delimited string
        /// </summary>
        /// <param name="Delimiter">Delimiter to Use</param>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToDelimitedString(char Delimiter = ',', ExportFormat format = ExportFormat.Full, DateTimeKind kind = DateTimeKind.Unspecified)
        {

            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {
                if (kind == DateTimeKind.Utc)
                    localSB.Append("\"" + this.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\"");
                else
                    localSB.Append("\"" + this.EventDateTimeUtc.ToString("yyyy-MM-dd HH:mm:ss.fffZ") + "\"");
                localSB.Append(Delimiter + this.LogFlag);
                localSB.Append(Delimiter + "\"" + this.Message + "\"");
                localSB.Append(Delimiter + this.HostFQDN);

                if (format == ExportFormat.Full)
                {
                    // Use all parameters if we want a full format
                    localSB.Append(Delimiter + this.MessageNumber.ToString());
                    localSB.Append(Delimiter + this.ProcessID.ToString());
                    localSB.Append(Delimiter + this.ThreadID.ToString());
                    localSB.Append(Delimiter + "\"" + this.Component + "\"");
                    localSB.Append(Delimiter + "\"" + this.ProcessName + "\"");
                    localSB.Append(Delimiter + this.SessionID);
                    localSB.Append(Delimiter + this.EventFileTime.ToString());
                }

                returnValue = localSB.ToString();
            }
            catch (Exception ex)
            {
                LogException(ex);
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
            return this.ToDelimitedString('\t', format);
        }

#if NET45_OR_GREATER
        private void LogException(Exception ex, [CallerMemberName]string methodName = "")
        {
#else
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void LogException(Exception ex)
        {
            string methodName = new System.Diagnostics.StackFrame(1, false).GetMethod().Name;
#endif
            log.Error(string.Format("{0}: {1} - {2}", methodName, ex.GetType().Name, ex.Message), ex);
        }
    }
}