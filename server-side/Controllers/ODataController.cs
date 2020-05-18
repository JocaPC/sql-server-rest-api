using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TSql.RestApi;

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
        
        public async Task Objects()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,type,type_desc,create_date,modify_date")
                                    .AddRelatedTable("columns", "sys", "columns", "sys.columns.object_id = sys.objects.object_id", "column_id,name,system_type_id,is_nullable,is_identity")
                                    .AddRelatedTable("parameters", "sys", "parameters", "sys.parameters.object_id = sys.objects.object_id", "parameter_id,name");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }

        public async Task Columns()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "columns", "column_id,name,system_type_id,max_length,precision,scale,is_nullable,is_identity");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }

        public async Task Parameters()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "parameters", "parameter_id,name,object_id,system_type_id,max_length,precision,scale,is_readonly,is_nullable,is_output");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }

        public async Task People()
        {
            var tableSpec = new TableSpec(schema: "Application", table: "dbo.People", "PersonID,FirstName,LastName");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }
    }
}