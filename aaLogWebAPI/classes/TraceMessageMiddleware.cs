using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaLogWebAPI
{


    public class TraceMessageMiddleware : OwinMiddleware
    {
        
        public TraceMessageMiddleware(OwinMiddleware next)
            : base(next)
        {   
        }

        /// <summary>
        /// Print the request information to the Console for debugging purposes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            var env = context.Request.Environment;
            var requestScheme = (string)env["owin.RequestScheme"];
            var requestRemoteIpAddress = (string)env["server.RemoteIpAddress"];
            var requestMethod = (string)env["owin.RequestMethod"];
            var requestQueryString = (string)env["owin.RequestQueryString"];
            var requestPathString = (string)env["owin.RequestPath"];

            string logMessage = string.Format("{0:yyyy-MM-dd HH:mm:ss} {1} {2} {3} {4}",
                DateTime.Now, requestRemoteIpAddress, requestMethod, requestPathString, requestQueryString);

            Console.WriteLine(logMessage);

            return Next.Invoke(context);
        }
    }
}
