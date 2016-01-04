using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace aaLogWebAPI
{
    public class FlexibleNegotiatedContentResult<T> : NegotiatedContentResult<T>
    {

        /*
            http://stackoverflow.com/questions/21440307/how-to-set-custom-headers-when-using-ihttpactionresult

            Here is a solution I use in my common Web API 2 library code that can easily support setting any headers--or any other properties 
            on the HttpResponseMessage provided in ExecuteAsync--without being tied to any specific derived NegotiatedContentResult implementation:
        
            and an example usage:

            new FlexibleNegotiatedContentResult<string>(HttpStatusCode.Created, "Entity created!", controller, response => response.Headers.Location = new Uri("https://myapp.com/api/entity/1"));

        */

        private readonly Action<HttpResponseMessage> _responseMessageDelegate;

        public FlexibleNegotiatedContentResult(HttpStatusCode statusCode, T content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(statusCode, content, contentNegotiator, request, formatters)
        {
        }

        public FlexibleNegotiatedContentResult(HttpStatusCode statusCode, T content, ApiController controller, Action<HttpResponseMessage> responseMessageDelegate = null)
            : base(statusCode, content, controller)
        {
            _responseMessageDelegate = responseMessageDelegate;
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage = await base.ExecuteAsync(cancellationToken);

            if (_responseMessageDelegate != null)
            {
                _responseMessageDelegate(responseMessage);
            }

            return responseMessage;
        }
    }

    public class CustomOkResult<T> : OkNegotiatedContentResult<T>
    {
        /*

            http://stackoverflow.com/questions/21440307/how-to-set-custom-headers-when-using-ihttpactionresult

            For your scenario, you would need to create a custom IHttpActionResult. 
            Following is an example where I derive from OkNegotiatedContentResult<T> as it runs Content-Negotiation and sets the Ok status code.

            Controller:

            public class ValuesController : ApiController
            {
                public IHttpActionResult Get()
                {
                    return new CustomOkResult<string>(content: "Hello World!", controller: this)
                        {
                                ETagValue = "You ETag value"
                        };
                }
            }

            Note that you can also derive from NegotiatedContentResult<T>, in which case you would need to supply the StatusCode yourself. Hope this helps.

            You can find the source code of OkNegotiatedContentResult<T> and NegotiatedContentResult<T>, which as you can imagine are simple actually.

        */
        public CustomOkResult(T content, ApiController controller)
            : base(content, controller)
        { }

        public CustomOkResult(T content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(content, contentNegotiator, request, formatters)
        { }

        public HttpHeaders CustomHeaders { get; set; }
        //public string ETagValue { get; set; }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.ExecuteAsync(cancellationToken);

            foreach(var customHeader in CustomHeaders)
            {
                response.Headers.Add(customHeader.Key, customHeader.Value);
            }

            //response.Headers.Add()

            //response.Headers.ETag = new EntityTagHeaderValue(this.ETagValue);

            return response;
        }
    }
}
