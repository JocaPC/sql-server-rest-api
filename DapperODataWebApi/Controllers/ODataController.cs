using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MsSql.RestApi;
using MsSql.RestApi.DAO;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    public class ODataController : Controller
    {
        private readonly TSqlCommand DbCommand;
        private readonly ILogger _logger;

        public ODataController(TSqlCommand command, ILogger<ODataController> logger)
        {
            this.DbCommand = command;
            this._logger = logger;
        }

        public string Index() { return "Hello"; }
        
        public async Task Objects()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,create_date")
                                    .AddRelatedTable("Columns", "sys", "columns", "sys.columns.object_id = sys.objects.object_id", "column_id,name,system_type_id,is_nullable,is_identity")
                                    .AddRelatedTable("Parameters", "sys", "parameters", "sys.parameters.object_id = sys.objects.object_id", "parameter_id,name");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }

        /// <summary>
        /// Endpoint that exposes People information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET /RestApi/odata
        public async Task People()
        {
            var tableSpec = new TableSpec(schema: "Application", table: "People", columns: "PersonID,FullName,PhoneNumber,FaxNumber,EmailAddress,ValidTo")
                .AddRelatedTable("Orders", "Sales", "Orders", "Application.People.PersonID = Sales.Orders.CustomerID", "OrderID,OrderDate,ExpectedDeliveryDate,Comments")
                .AddRelatedTable("Invoices", "Sales", "Invoices", "Application.People.PersonID = Sales.Invoices.CustomerID", "InvoiceID,InvoiceDate,IsCreditNote,Comments");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }
    }
}