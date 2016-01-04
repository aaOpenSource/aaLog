using aaLogWebAPI.Datasources;
using Microsoft.OData.Core;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;

namespace aaLogWebAPI.Controllers
{
    // Instead of a subclass, this could be an extensions class
    public class aaLogODataController : ODataController
    {
        /// <summary>
        /// Returns an ODataError message with the Controller.Method name and exception information</summary>
        /// <param name="ex"></param>
        /// <param name="code"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public NegotiatedContentResult<ODataError> HandleException(Exception ex, string code = null, [CallerMemberName]string methodName = null)
        {
            string message = string.Concat(GetType().Name, ".", methodName, " failed - ", ex.Message);
            return Content(
                HttpStatusCode.InternalServerError,
                new ODataError
                {
                    ErrorCode = code,
                    Message = message,
                    InnerError = new ODataInnerError(ex),
                }
            );
        }

        /// <summary>
        /// Wrap a function with an exception handler</summary>
        /// <example>
        /// Wrap(() => { aalogDataSource.Instance.GetRecordByMessageNumber(messageNumber); });</example>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpActionResult Wrap<T>(Func<T> value)
        {
            try
            {
                return Ok(value());
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class UnreadRecordsController : aaLogODataController
    {
        public IHttpActionResult Get(ulong unreadcount = 1000, string stopmessagepattern = "", bool ignorecachefile = false)        
        {
            try
            {
                // First get the client ID, if set from the request headers
                string localClientID = Request.Headers.GetValues("clientid").First();

                //Create a GUID if the passed clientID is null or empty
                if(string.IsNullOrEmpty(localClientID))
                {
                    localClientID = Guid.NewGuid().ToString();
                }
                
                FlexibleNegotiatedContentResult<IQueryable<aaLogReader.LogRecord>> localResponse;
                
                // Use the clientID in the call to get the unread records.  This allows for tracking custom cache files on a client by client basis
                // Add the clientID to the headers in the response
                localResponse = new FlexibleNegotiatedContentResult<IQueryable<aaLogReader.LogRecord>>(HttpStatusCode.OK, aalogDataSource.Instance.GetUnreadRecords(unreadcount, stopmessagepattern, ignorecachefile,localClientID).AsQueryable(), this, response => response.Headers.Add("clientid", localClientID));
                
                return localResponse;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class RecordByMessageNumberController : aaLogODataController
    {
        public IHttpActionResult Get(ulong messageNumber)
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetRecordByMessageNumber(messageNumber));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class RecordByFileTimeController : aaLogODataController
    {
        public IHttpActionResult Get(ulong messageNumber, aaLogReader.EarliestOrLatest TimestampEarlyOrLate = aaLogReader.EarliestOrLatest.Earliest)
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetRecordByFileTime(messageNumber, TimestampEarlyOrLate));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class RecordByTimestampController : aaLogODataController
    {
        public IHttpActionResult Get(DateTime messageTimestamp, aaLogReader.EarliestOrLatest TimestampEarlyOrLate = aaLogReader.EarliestOrLatest.Earliest)
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetRecordByTimestamp(messageTimestamp, TimestampEarlyOrLate));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class RecordsByStartMessageNumberAndCountController : aaLogODataController
    {
        public IHttpActionResult Get(ulong messageNumber, int count = 10)
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetRecordsByStartMessageNumberAndCount(messageNumber, count));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    [EnableQuery]
    public class RecordsByEndMessageNumberAndCountController : aaLogODataController
    {
        public IHttpActionResult Get(ulong messageNumber, int count = 10)
        {
            try
            {
                return Ok(aalogDataSource.Instance.GetRecordsByEndMessageNumberAndCount(messageNumber, count));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }    
}
