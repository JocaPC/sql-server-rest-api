using Belgrade.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlServerRestApi;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    public class RestApiController : Controller
    {
        private readonly ICommand queryService;
        private readonly ILogger _logger;

        public RestApiController(ICommand queryService, ILogger<RestApiController> logger)
        {
            this.queryService = queryService;
            this._logger = logger;
        }
        
        public async Task GetObjects()
        {
            await queryService
                .Sql("SELECT * FROM sys.objects FOR JSON PATH")
                .Stream(Response.Body);
        }

        public async Task GetCsv()
        {
            await queryService
                .Sql("SELECT STRING_AGG(CONCAT_WS(cast(',' as nvarchar(max)),object_id, name, create_date),CHAR(0x0d)+CHAR(0x0a)) FROM sys.objects")
                .Stream(Response.Body);
        }

        /// <summary>
        /// Endpoint that exposes People information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET /RestApi/odata
        [HttpGet("odata")]
        public async Task OData()
        {
            var tableSpec = new TableSpec("Application", "People", "PersonID,FullName,PhoneNumber,FaxNumber,EmailAddress,ValidTo");
            await this.OData(tableSpec, queryService).Process();
        }


        /// <summary>
        /// Endpoint that exposes People information using JQuery DataTables protocol.
        /// </summary>
        /// <returns>JQuery DataTables response.</returns>
        // GET /table
        [HttpGet("table")]
        public async Task Table()
        {
            var tableSpec = new TableSpec("Application", "People", "FullName,EmailAddress,PhoneNumber,FaxNumber");
            await this.Table(tableSpec, queryService).Process();
        }
    }
}
