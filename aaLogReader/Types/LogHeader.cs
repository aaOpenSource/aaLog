﻿using System;
using Newtonsoft.Json;
using System.Text;


namespace aaLogReader
{
    public class LogHeader : ILogHeader
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string LogFilePath { get; set; }

        public ulong StartMsgNumber { get; set; }

        public ulong MsgCount { get; set; }

        public ulong EndMsgNumber { 
            get
            {
                return (ulong)(checked(this.StartMsgNumber + this.MsgCount) - 1);
            }                                
        }

        private ulong _startFileTime;
        private DateTime _startDateTime;

        public ulong StartFileTime
        {
            get { return _startFileTime; }
            set
            {
                _startFileTime = value;
                _startDateTime = DateTime.FromFileTime((long)value);
            }
        }

        [JsonIgnore]
        public DateTime StartDateTime
        {
            get { return _startDateTime; }
        }

        public DateTime StartDateTimeUtc
        {
            get { return _startDateTime.ToUniversalTime(); }
        }

        private ulong _endFileTime;
        private DateTime _endDateTime;

        public ulong EndFileTime
        {
            get { return _endFileTime; }
            set
            {
                _endFileTime = value;
                _endDateTime = DateTime.FromFileTime((long)value);
            }
        }

        [JsonIgnore]
        public DateTime EndDateTime
        {
            get { return _endDateTime; }
        }

        public DateTime EndDateTimeUtc
        {
            get { return _endDateTime.ToUniversalTime(); }
        }

        public int OffsetFirstRecord { get; set; }

        public int OffsetLastRecord { get; set; }

        public string ComputerName { get; set; }

        public string Session { get; set; }

        public string PrevFileName { get; set; }

        public string HostFQDN { get; set; }

        [JsonIgnore]
        public ReturnCodeStruct ReturnCode { get; set; }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Return the log header data in the form of a Key-Value Pair
        /// </summary>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToKVP()
        {
            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {

                localSB.AppendFormat("MsgStartingNumber=\"{0}\"", this.StartMsgNumber.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                localSB.AppendFormat(", MsgCount=\"{0}\"", this.MsgCount);
                localSB.AppendFormat(", MsgLastNumber=\"{0}\"", this.EndMsgNumber);
                localSB.AppendFormat(", StartDateTime=\"{0}\"", this.StartDateTime);
                localSB.AppendFormat(", EndDateTime=\"{0}\"", this.EndDateTime);
                localSB.AppendFormat(", OffsetFirstRecord=\"{0}\"", this.OffsetFirstRecord);
                localSB.AppendFormat(", OffsetLastRecord=\"{0}\"", this.OffsetLastRecord);
                localSB.AppendFormat(", ComputerName=\"{0}\"", this.ComputerName);
                localSB.AppendFormat(", Session=\"{0}\"", this.Session);
                localSB.AppendFormat(", PrevFileName=\"{0}\"", this.PrevFileName);
                localSB.AppendFormat(", HostFQDN=\"{0}\"", this.HostFQDN);

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
        private string localHeader(char Delimiter = ',')
        {
            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {
                localSB.Append("LogFilePath");
                localSB.Append(Delimiter + "MsgStartingNumber");
                localSB.Append(Delimiter + "MsgCount");
                localSB.Append(Delimiter + "MsgLastNumber");
                localSB.Append(Delimiter + "StartDateTime");
                localSB.Append(Delimiter + "StartFileTime");
                localSB.Append(Delimiter + "EndDateTime");
                localSB.Append(Delimiter + "EndFileTime");
                localSB.Append(Delimiter + "OffsetFirstRecord");
                localSB.Append(Delimiter + "OffsetLastRecord");
                localSB.Append(Delimiter + "ComputerName");
                localSB.Append(Delimiter + "Session");
                localSB.Append(Delimiter + "PrevFileName");
                localSB.Append(Delimiter + "HostFQDN");

                returnValue = localSB.ToString();
            }
            catch (Exception ex)
            {
                LogException(ex);
                returnValue = "";
            }

            return returnValue;
        }

        public static string Header(char Delimiter = ',')
        {
            LogHeader lh = new LogHeader();
            return lh.localHeader(Delimiter);
        }

        public static string HeaderCSV()
        {
            return LogHeader.Header(',');
        }

        public static string HeaderTSV()
        {
            return LogHeader.Header('\t');
        }

        /// <summary>
        ///  Get the lastRecordRead in the form of a delimited string
        /// </summary>
        /// <param name="Delimiter">Delimiter to Use</param>
        /// <param name="format">Full or Minimal</param>
        /// <returns></returns>
        public string ToDelimitedString(char Delimiter = ',')
        {

            string returnValue;
            StringBuilder localSB = new StringBuilder();

            try
            {
                localSB.Append("\"" + this.LogFilePath + "\"");
                localSB.Append(Delimiter + this.StartMsgNumber.ToString());
                localSB.Append(Delimiter + this.MsgCount.ToString());
                localSB.Append(Delimiter + this.EndMsgNumber.ToString());
                localSB.Append(Delimiter + "\"" + this.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\"");
                localSB.Append(Delimiter + this.StartFileTime.ToString());
                localSB.Append(Delimiter + "\"" + this.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\"");
                localSB.Append(Delimiter + this.EndFileTime.ToString());
                localSB.Append(Delimiter + this.OffsetFirstRecord.ToString());
                localSB.Append(Delimiter + this.OffsetLastRecord.ToString());
                localSB.Append(Delimiter + "\"" + this.ComputerName + "\"");
                localSB.Append(Delimiter + this.Session);
                localSB.Append(Delimiter + "\"" + this.PrevFileName + "\"");
                localSB.Append(Delimiter + "\"" + this.HostFQDN + "\"");

                returnValue = localSB.ToString();
            }
            catch (Exception ex)
            {
                LogException(ex);
                returnValue = "";
            }

            return returnValue;
        }

        public string ToCSV()
        {
            return this.ToDelimitedString(',');
        }

        public string ToTSV()
        {
            return this.ToDelimitedString('\t');
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