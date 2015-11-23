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
        private aaLogReader.aaLogReader logreader;

        private static Lazy<aalogDataSource> instance =
            new Lazy<aalogDataSource>(() => new aalogDataSource());

        public static aalogDataSource Instance
        {
            get { return instance.Value; }
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
            return logreader.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile);
        }
    }
}
