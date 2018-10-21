using Belgrade.SqlClient.SqlDb;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SqlServerRestApi;
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
                var sqlpipe = new QueryPipe(Environment.GetEnvironmentVariable("SqlDb"));
                var tableSpec = new TableSpec(schema: "sys", name: "objects", columnList: "object_id,name,type,schema_id,create_date");
                await req.OData(tableSpec).Process(sqlpipe);
            }
            catch (Exception ex)
            {
                log.LogError($"C# Http trigger function exception: {ex.Message}");
            }
        }
    }
}
