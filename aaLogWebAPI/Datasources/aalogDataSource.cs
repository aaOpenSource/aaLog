using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using aaLogWebAPI.Models;

namespace aaLogWebAPI.Datasources
{
    class aalogDataSource
    {
        private static aaLogReader.aaLogReader logreader;

        private static aalogDataSource instance = null;

        public static aalogDataSource Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new aalogDataSource();
                }
                return instance;
            }
        }

        private aalogDataSource()
        {
            //this.Reset();
            this.Initialize();
        }

        public void Initialize()
        {
            logreader = new aaLogReader.aaLogReader();
        } 

        public List<aaLogReader.LogRecord> GetLogRecords(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)
        {
            List<aaLogReader.LogRecord> returnLogs = new List<aaLogReader.LogRecord>();

            try
            {            
                returnLogs = logreader.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile);
            }
            catch
            {
                // Do nothing
            }

           return returnLogs;

        }
    }
}
