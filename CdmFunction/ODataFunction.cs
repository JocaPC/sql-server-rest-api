using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TSql.OData;
using TSql.RestApi;
using Belgrade.SqlClient.SqlDb;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net;
using System.Web.Http;

namespace CdmFunction
{
    public static class ODataFunction
    {
        [FunctionName("OData")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "odata/{entity}")] HttpRequest req,
            string entity,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request: " + entity);

            HttpResponseMessage res = null;
            if (entity == "$metadata")
            {
                var ModelNamespace = "CDM.Models";
                var metadata = ODataHandler.GetMetadataXmlV4(Repository.GetTables(), ModelNamespace);

                res = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(metadata, Encoding.Default, "application/xml")
                };
                res.Headers.Add("OData-Version", "4.0");
            }
            else
            {
                var connection = Repository.GetConnectionString();
                var rh = req.OData(Repository.GetTable(entity), metadata: ODataHandler.Metadata.MINIMAL);
                
                
                var content = await rh.GetString(connection);

                content = "{\"@odata.context\":\""+req.Scheme+"://" + req.Host + req.Path.Value.Replace("/" + entity, "#" + entity) + "\",\"value\":" + content + "}";

                var httpContent = new StringContent(content);
                httpContent.Headers.Remove("content-type");
                httpContent.Headers.Add("content-type", "application/json; odata.metadata=minimal");

                res = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = httpContent
                };
                
            }

            //return res;
        }
    }
}
