using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using aaLogWebAPI.Datasources;

namespace aaLogWebAPI.Controllers
{
    [EnableQuery]
    public class UnreadRecordsController:ODataController
    {
        public IHttpActionResult Get(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)        
        {
            return Ok(aalogDataSource.Instance.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile).AsQueryable());
        }
    }

    [EnableQuery]
    public class RecordByMessageNumberController : ODataController
    {
        public IHttpActionResult Get(ulong messageNumber)
        {
            return Ok(aalogDataSource.Instance.GetRecordByMessageNumber(messageNumber));
        }
    }

    [EnableQuery]
    public class RecordByFileTimeController : ODataController
    {
        public IHttpActionResult Get(ulong messageNumber, aaLogReader.EarliestOrLatest TimestampEarlyOrLate = aaLogReader.EarliestOrLatest.Earliest)
        {
            return Ok(aalogDataSource.Instance.GetRecordByFileTime(messageNumber, TimestampEarlyOrLate));
        }
    }

    [EnableQuery]
    public class RecordByTimestampController : ODataController
    {
        public IHttpActionResult Get(DateTime messageTimestamp, aaLogReader.EarliestOrLatest TimestampEarlyOrLate = aaLogReader.EarliestOrLatest.Earliest)
        {
            return Ok(aalogDataSource.Instance.GetRecordByTimestamp(messageTimestamp, TimestampEarlyOrLate));
        }
    }

    [EnableQuery]
    public class RecordsByStartMessageNumberAndCountController : ODataController
    {
        public IHttpActionResult Get(ulong messageNumber, int count = 10)
        {
            return Ok(aalogDataSource.Instance.GetRecordsByStartMessageNumberAndCount(messageNumber, count));
        }
    }

    [EnableQuery]
    public class RecordsByEndMessageNumberAndCountController : ODataController
    {
        public IHttpActionResult Get(ulong messageNumber, int count = 10)
        {
            return Ok(aalogDataSource.Instance.GetRecordsByEndMessageNumberAndCount(messageNumber, count));
        }
    }
}
