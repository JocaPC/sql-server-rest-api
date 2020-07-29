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
using TSql.TableApi;
using Belgrade.SqlClient.SqlDb;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace CdmFunction
{
    public static class TableFunction
    {
        [FunctionName("Table")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "table/{entity}")] HttpRequest req,
            string entity,
            ILogger log)
        {
            Command cmd = new Command(Repository.GetConnectionString());
            await req.Table(Repository.GetTable(entity)).Process(cmd);
        }
    }
}
