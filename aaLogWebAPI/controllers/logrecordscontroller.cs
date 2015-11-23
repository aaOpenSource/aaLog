using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;

using aaLogWebAPI.Datasources;
using Microsoft.OData.Core;
//using aaLogWebAPI.Models;

namespace aaLogWebAPI.Controllers
{
    [EnableQuery]
    public class LogRecordsController:ODataController
    {
        public IHttpActionResult Get(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)        
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetLogRecords(unreadcount, stopmessagepattern, ignorecachefile).AsQueryable());
            }
            catch (Exception ex)
            {
                return Content(
                    HttpStatusCode.InternalServerError,
                    new ODataError
                    {
                        ErrorCode = string.Concat(GetType().Name, ".Get failed"),
                        Message = ex.Message,
                        InnerError = new ODataInnerError(ex),
                    }
                );
            }
        }
    }    
}
