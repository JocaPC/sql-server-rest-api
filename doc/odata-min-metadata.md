# TSql REST API - OData Service with minimal metadata

TSql REST API library enables you to easily create OData services. Default option that will work in most of the scenarios that 
you need is [OData service without metadata](odata.md) . However, some tools like Excel, LinqPad need to have at list some minimal metadata that wll enable them to load data from OData service. 
In this page you can find out how to create OData service with minimal metadata.

# Implement OData service

OData service can be implemented using any .Net project.

## No-metadata OData service and ASP.NET Core

You can implement OData service using ASP.NET Core application as a method of any controller. 
First, you need to setup Sql Client in Startup class: 
 - Add the reference to `TSql.RestApi` in Startup class
 - Initialize SqlClient component that will be used to read data from table and return it as OData response.

Example of code is shown in the following listing:

''Startup.cs''
```
using TSql.RestApi;

namespace MyMvcApp
{
    public class Startup {

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
             services.AddBelgradeSqlClient(Configuration["ConnectionStrings:WWI"]);

            // If you are using Dapper version use this command:
            //services.AddDapperSqlConnection(Configuration["ConnectionStrings:WWI"]);

            // Add framework services.
            services.AddMvc();
        }

    }
}
```

Then you need to create a controller that will expose OData service using some method.

 - Create a method that will expose OData REST Api.


## Minimal metadata OData service with ASP.NET Core

Some clients such as LinqPad require at least minimal OData metadata infor to use OData service. In this case you can create a OData service with minimal-metadata.

Then you need to create a controller that will expose OData service using some method.
 - Add references to TSql.RestApi namespace in controller,
 - Add constructor that gets TSqlCommand that will be used to execute queries,
 - Define properties of the tables that will be exposed via OData endpoints by overriding `TableSpec` property. This porpoery has an array of tables with definition of columns (and their types). See section below that can help you to generate specification from databases.
 - Create methods that will expose OData REST Api for every table in specification.

The following example shows how to create simple OData service that exposes `sys.objects` and `sys.columns` views:

```
using TSql.RestApi;

namespace MyMvcApp.Controllers
{
    public class SysMetadataController : ODataController
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
```

Example of OData service with minimal meta-data is shown in [this class](https://github.com/JocaPC/sql-server-rest-api/blob/master/TestApp/Controllers/SysODataController.cs).

## Generate table specification

You can generate table specification directly using T-SQL query by querying system views:

```
select CONCAT('new TableSpec("',schema_name(t.schema_id), '","', t.name, '")') +
	string_agg(CONCAT('.AddColumn("', c.name, '", "', tp.name, '", isKeyColumn:', IIF(ix.is_primary_key = 1, 'true', 'false'), ')'),'')
from sys.tables t
	join sys.columns c on t.object_id = c.object_id
	join sys.types tp on c.system_type_id = tp.system_type_id
	left join sys.index_columns ic on c.column_id = ic.column_id and c.object_id = ic.object_id
	left join sys.indexes ix on ic.index_id = ix.index_id and ic.object_id = ix.object_id
--where t.name in ('','','') --> specify target tables if needed.
group by t.schema_id, t.name
```

Text generated with this query can be copied into controller body.
