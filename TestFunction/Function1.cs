
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Belgrade.SqlClient.SqlDb;
using SqlServerRestApi;

namespace TestFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                /*
                string ConnectionString = "azure-db-connection";
                var sqlpipe = new QueryPipe(ConnectionString);
                var tableSpec = new TableSpec(schema: "sys", name: "objects", columnList: "object_id,name,type,schema_id,create_date");
                var querySpec = SqlServerRestApi.OData.UriParser.Parse(tableSpec, req);
                await req.OData(tableSpec, sqlpipe);
                */

            }
            catch (Exception)
            {
                //log.Error($"C# Http trigger function exception: {ex.Message}");
                //return new HttpResponseMessage() { Content = new StringContent(ex.Message), StatusCode = HttpStatusCode.InternalServerError };
            }
        }
    }
}
