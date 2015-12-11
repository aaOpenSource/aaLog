using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using System.Web.Http;
using System.Diagnostics;
using Microsoft.OData.Edm;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace aaLogWebAPI
{
    public class Startup
    {
        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            var webApiConfiguration = ConfigureWebApi();

            // Use the extension method provided by the WebApi.Owin library:
            app.UseWebApi(webApiConfiguration);            
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            
            // Attribute routing.
            config.MapHttpAttributeRoutes();

            config.MapODataServiceRoute("odata","odata", GetEdmModel(), null);

            config.EnsureInitialized();

            return config;
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "aaLogWebAPI";
            builder.ContainerName = "DefaultContainer";
            builder.EntitySet<aaLogReader.LogRecord>("LogRecords");
            var edmModel = builder.GetEdmModel();
            return edmModel;
        }
    }
}
