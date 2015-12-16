using System;
using System.Collections.Generic;

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

        private void Initialize()
        {
            logreader = new aaLogReader.aaLogReader();
        } 

        public List<aaLogReader.LogRecord> GetUnreadRecords(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)
        {
            return logreader.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile);
        }

        public aaLogReader.LogRecord GetRecordByMessageNumber(ulong messageNumber)
        {
            return logreader.GetRecordByMessageNumber(messageNumber);
        }

        public aaLogReader.LogRecord GetRecordByFileTime(ulong messageFileTime, aaLogReader.EarliestOrLatest TimestampEarlyOrLate)
        {
            return logreader.GetRecordByFileTime(messageFileTime, TimestampEarlyOrLate);
        }

        public aaLogReader.LogRecord GetRecordByTimestamp(DateTime messageTimestamp, aaLogReader.EarliestOrLatest TimestampEarlyOrLate)
        {
            return logreader.GetRecordByTimestamp(messageTimestamp, TimestampEarlyOrLate);
        }

        //GetRecordsByStartMessageNumberAndCount

        public List<aaLogReader.LogRecord> GetRecordsByStartMessageNumberAndCount(ulong messageNumber, int count)
        {
            return logreader.GetRecordsByStartMessageNumberAndCount(messageNumber,count);
        }

        public List<aaLogReader.LogRecord> GetRecordsByEndMessageNumberAndCount(ulong messageNumber, int count)
        {
            return logreader.GetRecordsByEndMessageNumberAndCount(messageNumber, count);
        }

    }
}
