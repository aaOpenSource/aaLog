using System;
using Newtonsoft.Json;
using System.Text;


namespace aaLogReader
{
    public class LogHeader : ILogHeader
    {
        public string LogFilePath { get; set; }

        public ulong StartMsgNumber { get; set; }

        public ulong MsgCount { get; set; }

        public ulong EndMsgNumber { 
            get
            {
                return (ulong)(checked(this.StartMsgNumber + this.MsgCount) - 1);
            }                                
        }

        public ulong StartFileTime { get; set; }

        public DateTime StartDateTime
        {
            get { return DateTime.FromFileTime((long)this.StartFileTime); }
        }
        
        public ulong EndFileTime { get; set; }

        public DateTime EndDateTime
        {
            get { return DateTime.FromFileTime((long)this.EndFileTime); }
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

                localSB.Append("MsgStartingNumber=");
                localSB.Append(((char)34).ToString() + this.StartMsgNumber.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                localSB.Append(", MsgCount=");
                localSB.Append(((char)34).ToString() + this.MsgCount + ((char)34).ToString());

                localSB.Append(", MsgLastNumber=");
                localSB.Append(((char)34).ToString() + this.EndMsgNumber + ((char)34).ToString());

                localSB.Append(", StartDateTime=");
                localSB.Append(((char)34).ToString() + this.StartDateTime + ((char)34).ToString());

                localSB.Append(", EndDateTime=");
                localSB.Append(((char)34).ToString() + this.EndDateTime + ((char)34).ToString());

                localSB.Append(", OffsetFirstRecord=");
                localSB.Append(((char)34).ToString() + this.OffsetFirstRecord + ((char)34).ToString());

                localSB.Append(", OffsetLastRecord=");
                localSB.Append(((char)34).ToString() + this.OffsetLastRecord + ((char)34).ToString());

                localSB.Append(", ComputerName=");
                localSB.Append(((char)34).ToString() + this.ComputerName + ((char)34).ToString());

                localSB.Append(", Session=");
                localSB.Append(((char)34).ToString() + this.Session + ((char)34).ToString());

                localSB.Append(", PrevFileName=");
                localSB.Append(((char)34).ToString() + this.PrevFileName + ((char)34).ToString());

                localSB.Append(", HostFQDN=");
                localSB.Append(((char)34).ToString() + this.HostFQDN + ((char)34).ToString());

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
            catch
            {
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
                localSB.Append(((char)34).ToString() + this.LogFilePath + ((char)34).ToString());
                localSB.Append(Delimiter + this.StartMsgNumber.ToString());
                localSB.Append(Delimiter + this.MsgCount.ToString());
                localSB.Append(Delimiter + this.EndMsgNumber.ToString());
                localSB.Append(Delimiter + ((char)34).ToString() + this.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ((char)34).ToString());
                localSB.Append(Delimiter + this.StartFileTime.ToString());
                localSB.Append(Delimiter + ((char)34).ToString() + this.EndDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ((char)34).ToString());
                localSB.Append(Delimiter + this.EndFileTime.ToString());
                localSB.Append(Delimiter + this.OffsetFirstRecord.ToString());
                localSB.Append(Delimiter + this.OffsetLastRecord.ToString());
                localSB.Append(Delimiter + ((char)34).ToString() + this.ComputerName + ((char)34).ToString());
                localSB.Append(Delimiter + this.Session);
                localSB.Append(Delimiter + ((char)34).ToString() + this.PrevFileName + ((char)34).ToString());
                localSB.Append(Delimiter + ((char)34).ToString() + this.HostFQDN + ((char)34).ToString());

                returnValue = localSB.ToString();
            }
            catch
            {
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
    }
}