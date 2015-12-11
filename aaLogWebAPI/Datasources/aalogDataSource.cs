using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaLogWebAPI.Datasources
{
    class aalogDataSource
    {
        private aaLogReader.aaLogReader logreader;

        private static Lazy<aalogDataSource> instance =
            new Lazy<aalogDataSource>(() => new aalogDataSource());

        public static aalogDataSource Instance
        {
            get { return instance.Value; }
        }

        private aalogDataSource()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            logreader = new aaLogReader.aaLogReader();
        } 

        public List<aaLogReader.LogRecord> GetUnreadRecords(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)
        {
            List<aaLogReader.LogRecord> returnLogs = new List<aaLogReader.LogRecord>();

            try
            {            
                returnLogs = logreader.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile);
            }
            catch
            {
                // Do nothing, just return empty set
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code
            }

           return returnLogs;

        }

        public aaLogReader.LogRecord GetRecordByMessageNumber(ulong messageNumber)
        {
            aaLogReader.LogRecord returnLogRecord = new aaLogReader.LogRecord();

            try
            {
                returnLogRecord = logreader.GetRecordByMessageNumber(messageNumber);
            }
            catch
            {
                // Do nothing
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code
            }

            return returnLogRecord;

        }

        public aaLogReader.LogRecord GetRecordByFileTime(ulong messageFileTime, aaLogReader.EarliestOrLatest TimestampEarlyOrLate)
        {
            aaLogReader.LogRecord returnLogRecord = new aaLogReader.LogRecord();

            try
            {
                returnLogRecord = logreader.GetRecordByFileTime(messageFileTime, TimestampEarlyOrLate);
            }
            catch
            {
                // Do nothing
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code
            }

            return returnLogRecord;            
        }

        public aaLogReader.LogRecord GetRecordByTimestamp(DateTime messageTimestamp, aaLogReader.EarliestOrLatest TimestampEarlyOrLate)
        {
            aaLogReader.LogRecord returnLogRecord = new aaLogReader.LogRecord();

            try
            {
                returnLogRecord = logreader.GetRecordByTimestamp(messageTimestamp, TimestampEarlyOrLate);
            }
            catch
            {
                // Do nothing
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code

            }

            return returnLogRecord;
        }

        //GetRecordsByStartMessageNumberAndCount

        public List<aaLogReader.LogRecord> GetRecordsByStartMessageNumberAndCount(ulong messageNumber, int count)
        {
            List<aaLogReader.LogRecord> returnLogRecords = new List<aaLogReader.LogRecord>();

            try
            {
                returnLogRecords = logreader.GetRecordsByStartMessageNumberAndCount(messageNumber,count);
            }
            catch
            {
                // Do nothing
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code

            }

            return returnLogRecords;
        }

        public List<aaLogReader.LogRecord> GetRecordsByEndMessageNumberAndCount(ulong messageNumber, int count)
        {
            List<aaLogReader.LogRecord> returnLogRecords = new List<aaLogReader.LogRecord>();

            try
            {
                returnLogRecords = logreader.GetRecordsByEndMessageNumberAndCount(messageNumber, count);
            }
            catch
            {
                // Do nothing
                // TODO: Consider more sophisticated return to cue the caller to present a proper HTTP Response code

            }

            return returnLogRecords;
        }

    }
}
