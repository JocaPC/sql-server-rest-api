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

namespace CdmFunction
{
    public static class ODataMetadataFunction
    {
        [FunctionName("ODataMetadata")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "odatametadata")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a $metadata request ");


            var ModelNamespace = "CDM.Models";
            var metadata = ODataHandler.GetMetadataXmlV4(Repository.GetTables(), ModelNamespace);

            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(metadata, Encoding.Default, "application/xml")
            };

            res.Headers.Add("ContentType", "application/xml");
            res.Headers.Add("OData-Version", "4.0");

            return res;

        }
    }
}
