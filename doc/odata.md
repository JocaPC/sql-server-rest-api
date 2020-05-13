# TSql REST API - OData Service

TSql REST API library enables you to easily create OData services. In this page you can find out how to create
OData service and what features from OData standard are supported.

# Supported query parameters

TSql REST API library enables you to create OData REST services that support the following operations:
 - $select that enables the caller to choose what fields should be returned in response,
 - $filter that can filter entities using entity properties, and expressions with:
   - Arithmetical operators 'add', 'sub', 'mul', 'div' , 'mod'
   - Relational operators 'eq', 'ne', 'gt', 'ge', 'lt', 'le'
   - Logical operators 'and', 'or', 'not', 	
   - Build-in functions: 'length', 'tolower', 'toupper', 'year', 'month', 'day', 'hour', 'minute', and 'second',
   - Non-standard functions: 'json_value', 'json_query', and 'isjson'
 - $orderby that can sort entities by some column(s)
 - $top and $skip that can be used for pagination,
 - $count that enables you to get the total number of entities,
 - $search that search for entities by a keyword.
 - $expand that enables you to get related entities.

OData services implemented using TSql REST API library provide minimal interface that web clients can use to
query data without additional overhead introduced by advanced OData operators (e.g. $extend, all/any), or verbose response format.

> The goal of this project is not to support all standard OData features. Library provides the most important features, and
> excludes features that cannot provide optimal performance. The most important benefits that this library provides are simplicity and speed. 
> If you need full compatibility with official OData spec, you can chose other implementations.

# Metadata information

OData services implemented using Sql Server REST API library return minimal response format that is compliant to the
OData spec. By default, it returns no-metadata; however, it can be configured to output minimal metadata.

# Implement OData service

OData service can be implemented using any .Net project, such ASP.NET Core, ASP.NET Web Api, Azure Function (C#). You just need to reference nuget package.

This setting will take TSql Rest Api from NuGet and also load Antlr4.Runtime that is used to parse requests. Once you reference these nuget packages, you can create your OData service.

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
 - Add references to Belgrade.SqlClient and SqlServerApi namespace in controller,
 - Initialize IQueryPipe field using constructor injection,
 - Create a method that will expose OData REST Api.

```
using Belgrade.SqlClient;
using SqlServerRestApi;

namespace MyMvcApp.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        private readonly TSqlCommand DbCommand;
        
        public PeopleController(TSqlCommand command)
        {
            this.DbCommand = command;
        }
        

        /// <summary>
        /// Endpoint that exposes People information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET api/People/odata
        [HttpGet("odata")]
        public async Task OData()
        {
            var tableSpec = new TableSpec(schema: "dbo", table: "People", columns: "PersonID,FullName,PhoneNumber,FaxNumber,EmailAddress,ValidTo")
                .AddRelatedTable("Orders", "Sales", "Orders", "Application.People.PersonID = Sales.Orders.CustomerID", "OrderID,OrderDate,ExpectedDeliveryDate,Comments")
                .AddRelatedTable("Invoices", "Sales", "Invoices", "Application.People.PersonID = Sales.Invoices.CustomerID", "InvoiceID,InvoiceDate,IsCreditNote,Comments");
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }
    }
}
```

When you run this app and open http://......./api/People/odata Url, you would be able to call all supported functions in OData service.

## No-metadata OData service and Azure Function

Azure Functions are lightweight components that you can use to create some function in C#, Node.JS, or other languages, and expose the function
as API. This might be combined with OData since you can create single Azure Function that will handle OData requests.

You can go to Azure portal, create new Aure Function as HttpTrigger, add project.json, and put something lik the following code in Function body:

```
using Belgrade.SqlClient.SqlDb;
using System.Net;
using System.Configuration;
using SqlServerRestApi;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Started execution...");

    try{
        string ConnectionString = ConfigurationManager.ConnectionStrings["azure-db-connection"].ConnectionString;
        var sqlpipe = new QueryPipe(ConnectionString);
        var tableSpec = new TableSpec("sys.objects", "object_id,name,type,schema_id,create_date");
        return await req.CreateODataResponse(tableSpec, sqlpipe);
        
    } catch (Exception ex) {
        log.Error($"C# Http trigger function exception: {ex.Message}");
        return new HttpResponseMessage() { Content = new StringContent(ex.Message), StatusCode = HttpStatusCode.InternalServerError };
    }
}
```

You need to setup connection string to your database in some key (e.g. "azure-db-connection"), get that connection string, create SqlPipe that
will stream results into Response output stream. You will need to create ''TableSpec'' object that describes source table or view with the name
object and list of columns that will be exposed. In this example, OData exposes five columns from sys.object view.

When you call this Url, you can add any OData parameter to filter or sort results.

## Minimal metadata OData service with ASP.NET Core

Some clients such as LinqPad require at least minimal OData metadata infor to use OData service. In this case you can create a OData service with minimal-metadata.

Then you need to create a controller that will expose OData service using some method.
 - Add references to Belgrade.SqlClient and SqlServerApi namespace in controller,
 - Derive Controller from OData Controller,
 - Initialize IQueryPipe field using constructor injection,
 - Define properties of the tables that will be exposed via OData endpoints by overriding GetTableSpec method,
 - Create methods that will expose OData REST Api.

```
using Belgrade.SqlClient;
using SqlServerRestApi;

namespace MyMvcApp.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : ODataController
    {
        IQueryPipe pipe = null;
        public PeopleController(IQueryPipe sqlQueryService)
        {
            this.pipe = sqlQueryService;
        }

        public override TableSpec[] GetTableSpec { get { return tables; } }

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

        // GET api/values/objects
        // GET api/values/objects/$count
        [HttpGet("objects")]
        [HttpGet("objects/$count")]
        public async Task Objects()
        {
            await this
                .OData(tables[0], sqlQueryService, ODataHandler.Metadata.MINIMAL)
                .OnError(ex => Response.Body.Write(Encoding.UTF8.GetBytes(ex.Message), 0, (ex.Message).Length))
                .Get();                                           
        }

        // GET api/values/columns
        // GET api/values/columns/$count
        [HttpGet("columns")]
        [HttpGet("columns/$count")]
        public async Task Columns()
        {
            await this
                .OData(tables[1], sqlQueryService, ODataHandler.Metadata.MINIMAL)
                .OnError(ex => Response.Body.Write(Encoding.UTF8.GetBytes(ex.Message), 0, (ex.Message).Length))
                .Get();
        }

    }
}
```

You can generate table specification directly using T-SQL query by querying system views:

```
select CONCAT('new TableSpec("',schema_name(t.schema_id), '","', t.name, '")') +
	string_agg(CONCAT('.AddColumn("', c.name, '", "', tp.name, '", isKeyColumn:', IIF(ix.is_primary_key = 1, 'true', 'false'), '))'),'')
from sys.tables t
	join sys.columns c on t.object_id = c.object_id
	join sys.types tp on c.system_type_id = tp.system_type_id
	left join sys.index_columns ic on c.column_id = ic.column_id and c.object_id = ic.object_id
	left join sys.indexes ix on ic.index_id = ix.index_id and ic.object_id = ix.object_id
--where t.name in ('','','') --> specify target tables if needed.
group by t.schema_id, t.name
```

Text generated with this query can be copied into controller body.
