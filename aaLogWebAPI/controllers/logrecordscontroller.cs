using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using aaLogWebAPI.Datasources;
//using aaLogWebAPI.Models;

namespace aaLogWebAPI.Controllers
{
    [EnableQuery]
    public class LogRecordsController:ODataController
    {
        public IHttpActionResult Get(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)        
        {
            return Ok(aalogDataSource.Instance.GetLogRecords(unreadcount, stopmessagepattern, ignorecachefile).AsQueryable());
        }
    }    
}
