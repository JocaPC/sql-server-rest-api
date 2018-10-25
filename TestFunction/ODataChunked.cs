using Belgrade.SqlClient.SqlDb;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MsSql.RestApi;
using System;
using System.Threading.Tasks;

namespace TestFunction
{
    public static class ODataChunked
    {
        [FunctionName("ODataChunked")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,type,schema_id,create_date");
                await req.OData(tableSpec).Process(Environment.GetEnvironmentVariable("SqlDb"));
            }
            catch (Exception ex)
            {
                log.LogError($"C# Http trigger function exception: {ex.Message}");
            }
        }
    }
}
