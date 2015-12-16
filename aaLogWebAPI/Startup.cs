using Microsoft.OData.Edm;
using Owin;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace aaLogWebAPI
{
    public class Startup
    {
        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            var webApiConfiguration = ConfigureWebApi(app);

            // Use the extension method provided by the WebApi.Owin library:
            app.Use(typeof(TraceMessageMiddleware));
            app.UseWebApi(webApiConfiguration);
        }

        private HttpConfiguration ConfigureWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            // Attribute routing.
            config.MapHttpAttributeRoutes();

            config.MapODataServiceRoute("odata", "odata", GetEdmModel(), null);

            config.EnsureInitialized();

            return config;
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "aaLogWebAPI";
            builder.ContainerName = "DefaultContainer";
            builder.EntitySet<aaLogReader.LogRecord>("UnreadRecords");
            builder.EntitySet<aaLogReader.LogRecord>("RecordByMessageNumber");
            builder.EntitySet<aaLogReader.LogRecord>("RecordByFileTime");
            builder.EntitySet<aaLogReader.LogRecord>("RecordByTimestamp");
            builder.EntitySet<aaLogReader.LogRecord>("RecordsByStartMessageNumberAndCount");

            var edmModel = builder.GetEdmModel();
            return edmModel;
        }
    }
}
