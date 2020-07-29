using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TSql.RestApi;

namespace SynapseCDM.Controllers
{
    public class SynapseSql : Controller
    {
        private readonly ILogger<CdmController> logger;
        private readonly TSqlCommand command;
        public SynapseSql(TSqlCommand command, ILogger<CdmController> logger)
        {
            this.logger = logger;
            this.command = command;
        }

        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        [Produces("application/json")]
        public async Task Location()
        {
            string query = "select name, create_date from sys.credentials where credential_identity = 'SHARED ACCESS SIGNATURE' order by create_date desc for json path, root('data')";

            await this.command
                    .Sql(query)
                    .Stream(this.HttpContext.Response.Body);

        }

        [HttpPost]
        public async Task Location(string url, string sas)
        {
            if (string.IsNullOrEmpty(url))
                throw new System.Exception("URL is required");
            if (string.IsNullOrEmpty(sas))
                throw new System.Exception("Shared Access signature is required");

            string query = $"BEGIN TRY DROP CREDENTIAL [{url}]; END TRY BEGN CATCH END CATCH;";
            query += $"CREATE CREDENTIAL [{url}] WITH IDENTITY='SHARED ACCESS SIGNATURE', SECRET = '{sas}'";

            await this.command
                    .Sql(query)
                    .Execute(null);

            await Location();
        }
    }
}
