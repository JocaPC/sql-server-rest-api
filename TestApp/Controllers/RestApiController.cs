using Belgrade.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MsSql.RestApi;
using MsSql.RestApi.DAO;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    public class RestApiController : Controller
    {
        private readonly TSqlCommand DbCommand;
        private readonly ILogger _logger;

        public RestApiController(TSqlCommand command, ILogger<RestApiController> logger)
        {
            this.DbCommand = command;
            this._logger = logger;
        }
        
        public async Task GetObjects()
        {
            await DbCommand
                .Sql("SELECT * FROM sys.objects FOR JSON PATH")
                .OnError(e=> { this._logger.LogError("Error: {error}", new { Exc = e }); throw e; })
                .Stream(Response.Body);
        }

        public async Task GetCsv()
        {
            await DbCommand
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
            var tableSpec = new TableSpec(schema: "Application", table: "People", columns: "PersonID,FullName,PhoneNumber,FaxNumber,EmailAddress,ValidTo")
                .AddRelatedTable("Orders", "Sales", "Orders", "Application.People.PersonID = Sales.Orders.CustomerID", "OrderID,OrderDate,ExpectedDeliveryDate,Comments")
                .AddRelatedTable("Invoices", "Sales", "Invoices", "Application.People.PersonID = Sales.Invoices.CustomerID", "InvoiceID,InvoiceDate,IsCreditNote,Comments");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }


        /// <summary>
        /// Endpoint that exposes sys.objects information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET /RestApi/objects
        [HttpGet("objects")]
        public async Task Objects()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,type,type_desc,create_date,modify_date")
                                    .AddRelatedTable("Columns", "sys", "columns", "sys.columns.object_id = sys.objects.object_id", "column_id,name,system_type_id,is_nullable,is_identity")
                                    .AddRelatedTable("Parameters", "sys", "parameters", "sys.parameters.object_id = sys.objects.object_id", "parameter_id,name");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }

        /// <summary>
        /// Endpoint that exposes People information using JQuery DataTables protocol.
        /// </summary>
        /// <returns>JQuery DataTables response.</returns>
        // GET /table
        [HttpGet("table")]
        public async Task Table()
        {
            var tableSpec = new TableSpec(schema: "Application", table: "People", columns: "FullName,EmailAddress,PhoneNumber,FaxNumber");
            await this
                    .Table(tableSpec)
                    .Process(DbCommand);
        }
    }
}