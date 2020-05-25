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
 - $search that search for entities by a keyword,
 - $expand that enables you to get related entities,
 - $apply that enables you to apply some aggregate functions on groups. 
 
 > $count is not supported. Use `$apply=aggregate(object_id with count as c)` as an alternative

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

Then you need to create a controller that will expose OData service using some method, you would need to:
 - Add references to TSql.RestApi namespace in controller,
 - Initialize TSqlCommand field using constructor injection,
 - Create a method that will expose OData REST Api.

```
using TSql.RestApi;

namespace MyMvcApp.Controllers
{
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

When you run this app and open http://......./People/odata Url, you would be able to call all supported functions in OData service.

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

Some clients such as Excel and LinqPad require at least minimal OData metadata info to use OData service. No-metadata OData service implemented using the instructions in this doc provide minimal required set of features that enable you to use rich REST query language over your data. However, no-metadata services don't enable Excelt&linqPad to consule OData service. If you want to enable tools that generate code based on OData endpoint, you need to provide at least minimal meatata about the OData service. 

In this case you can create a OData service with minimal-metadata using the [instructions described here](odata-min-metadata.md).

## Generate table specification

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

## Example

Fully functional code sample can be found in [this branch](https://github.com/JocaPC/sql-server-rest-api/tree/belgrade-odata-api).
