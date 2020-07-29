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
using System.Text;
using System.Net.Http;
using System.Net;

namespace CdmFunction
{
    public static class ODataRootMatadataFunction
    {
        [FunctionName("ODataRootMetadata")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "odata")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a root request.");

            var metadata = ODataHandler.GetRootMetadataJsonV4(
                                req.Scheme + "://" + req.Host + "/" + "api/odata/$metadata",
                                Repository.GetTables(),
                                req.Scheme + "://" + req.Host + "/" + "api/odata/");

            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(metadata, Encoding.Unicode, "application/json")
            };

            req.HttpContext.Response.StatusCode = 200;
            req.HttpContext.Response.ContentType = "application/json; odata.metadata=minimal";
            await req.HttpContext.Response.WriteAsync(metadata);
   



            //return res;
        }
    }
}
