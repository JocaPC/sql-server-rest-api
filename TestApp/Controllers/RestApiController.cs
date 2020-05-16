using Belgrade.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TSql.RestApi;
using System.Threading.Tasks;
using TSql.OData;

namespace MyApp.Controllers
{
    
    public class RestApiController : Controller
    { 
        static readonly TableSpec[] tables = new TableSpec[]
        {
            new TableSpec("sys", "objects")
                .AddColumn("object_id", "int", isKeyColumn: true)
                .AddColumn("name", "nvarchar", 128)
                .AddColumn("type", "nvarchar", 20)
                .AddColumn("schema_id", "int"),
            new TableSpec("sys", "columns")
                .AddColumn("object_id", "int", isKeyColumn: true)
                .AddColumn("column_id", "int", isKeyColumn: true)
                .AddColumn("name", "nvarchar", 128)
        };


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
                .Sql("SELECT TOP 2 * FROM sys.objects FOR JSON PATH")
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
        // GET /OData/People
        [HttpGet("[controller]/odata/people")]
        public async Task People()
        {
            var tableSpec = new TableSpec(schema: "Application", table: "People", columns: "PersonID,FullName,PhoneNumber,FaxNumber,EmailAddress,ValidTo")
                .AddRelatedTable("Orders", "Sales", "Orders", "Application.People.PersonID = Sales.Orders.CustomerID", "OrderID,OrderDate,ExpectedDeliveryDate,Comments")
                .AddRelatedTable("Invoices", "Sales", "Invoices", "Application.People.PersonID = Sales.Invoices.CustomerID", "InvoiceID,InvoiceDate,IsCreditNote,Comments");
            await this
                    .OData(tableSpec, metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }

        public ActionResult OData()
        {
            return this.GetODataServiceDocumentJsonV4(tables, "restapi/odata/$metadata");
        }

        [HttpGet("[controller]/odata/$metadata")]
        public ActionResult ODataMetadata()
        {
            return this.GetODataMetadataXmlV4(tables);
        }


        /// <summary>
        /// Endpoint that exposes sys.objects information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET /RestApi/objects
        [HttpGet("[controller]/odata/objects")]
        public async Task Objects()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,type,type_desc,create_date,modify_date")
                                    .AddRelatedTable("Columns", "sys", "columns", "sys.columns.object_id = sys.objects.object_id", "column_id,name,system_type_id,is_nullable,is_identity")
                                    .AddRelatedTable("Parameters", "sys", "parameters", "sys.parameters.object_id = sys.objects.object_id", "parameter_id,name");
            await this
                    .OData(tables[0], metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }

        [HttpGet("[controller]/odata/columns")]
        public async Task columns()
        {
            await this
                .OData(tables[1], metadata: ODataHandler.Metadata.MINIMAL)
                .Process(DbCommand);
        }

        //public async Task columnsTable()
        //{
        //    var tableSpec = new TableSpec(schema: "sys", table: "columns", columns: "column_id,name,system_type_id,is_nullable,is_identity");
        //    await this
        //            .Table(tableSpec)
        //            .Process(DbCommand);
        //}

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