using Belgrade.SqlClient.SqlDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SqlServerRestApi;
using System;
using System.Threading.Tasks;

namespace TestFunction
{
    public static class ODataResult
    {
        [FunctionName("ODataResult")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var mapper = new QueryMapper(Environment.GetEnvironmentVariable("SqlDb"));
                var tableSpec = new TableSpec(schema: "sys", name: "objects", columnList: "object_id,name,type,schema_id,create_date");
                return await req.OData(tableSpec).GetResult(mapper);
            }
            catch (Exception ex)
            {
                log.LogError($"C# Http trigger function exception: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
