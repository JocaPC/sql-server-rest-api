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
                .AddColumn("name", "nvarchar", 128),
            new TableSpec("sys","parameters")
                .AddColumn("object_id", "int", isKeyColumn:true)
                .AddColumn("name", "nvarchar", isKeyColumn:false)
                .AddColumn("parameter_id", "int", isKeyColumn:false)
                .AddColumn("system_type_id", "tinyint", isKeyColumn:false)
                .AddColumn("max_length", "smallint", isKeyColumn:false)
                .AddColumn("precision", "tinyint", isKeyColumn:false)
                .AddColumn("scale", "tinyint", isKeyColumn:false)
                .AddColumn("is_output", "bit", isKeyColumn:false)
                .AddColumn("default_value", "sql_variant", isKeyColumn:false)
                .AddColumn("is_readonly", "bit", isKeyColumn:false)
                .AddColumn("is_nullable", "bit", isKeyColumn:false)
        };

        public ODataController(TSqlCommand command, ILogger<ODataController> logger)
        {
            this.DbCommand = command;
            this._logger = logger;
        }
        
        public ActionResult Index()
        {
            return this.GetODataServiceDocumentJsonV4(tables, "odata/$metadata");
            // @@IMPORTANT "odata/$metadata" value MUST match route of metadata below
        }

        [HttpGet("odata/$metadata")]
        public ActionResult Metadata()
        {
            return this.GetODataMetadataXmlV4(tables);
        }

        public async Task Objects()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "objects", columns: "object_id,name,type,type_desc,create_date,modify_date")
                                    .AddRelatedTable("columns", "sys", "columns", "sys.columns.object_id = sys.objects.object_id", "column_id,name,system_type_id,is_nullable,is_identity")
                                    .AddRelatedTable("parameters", "sys", "parameters", "sys.parameters.object_id = sys.objects.object_id", "parameter_id,name");
            await this
                    .OData(tables[0],metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }

        public async Task Columns()
        {
            var tableSpec = new TableSpec(schema: "sys", table: "columns", "column_id,name,system_type_id,max_length,precision,scale,is_nullable,is_identity");
            await this
                    .OData(tables[1], metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }

        public async Task Parameters()
        {
            await this
                    .OData(tables[2], metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }
    }
}