using System.Threading.Tasks;
using TSql.OData;
using TSql.RestApi;

namespace MyApp.Controllers
{
    public class SysODataController : ODataController
    {
        public SysODataController(TSqlCommand command) : base(command) { }

        public override TableSpec[] TableSpec => tables;

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

        public async Task Objects()
        {
            await this
                    .OData(tables[0], metadata: ODataHandler.Metadata.MINIMAL)
                    .Process(DbCommand);
        }

        public async Task columns()
        {
            await this
                .OData(tables[1], metadata: ODataHandler.Metadata.MINIMAL)
                .Process(DbCommand);
        }
    }
}