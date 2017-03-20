# Sql Server REST API - OData Service

Sql Server REST API library enables you to easily create OData services. In this page you can find out how to create
OData service and what features from OData standard are supported.

# Supported query parameters

Sql Server REST API library enables you to create OData REST services that support the following operations:
 - $select that enables the caller to choose what fields should be returned in response,
 - $filter that can filter entities using entity properties, and expressions with:
   - Arithmetical operators 'add', 'sub', 'mul', 'div' , 'mod'
   - Relational operators 'eq', 'ne', 'gt', 'ge', 'lt', 'le'
   - Logical operators 'and', 'or', 'not', 	
   - Build-in functions: 'length', 'tolower', 'toupper', 'year', 'month', 'day', 'hour', 'minute', 'second', 'json_value', 'json_query', and 'isjson'
 - $orderby that can sort entities by some column(s)
 - $top and $skip that can be used for pagination,
 - $count that enables you to get the total number of entities,
 - $search that search for entities by a keyword

OData services implemented using Sql Server REST API library provide minimal interface that web clients can use to
query data without additional overhead introduced by advanced OData operators (e.g. $extend, all/any), or verbose response format.

> The goal of this project is not to support all standard OData features. Library provides the most important features, and
> excludes features that cannot provide optimal performance. The most important benefits that this library provides are simplicity and speed. 
> If you need full compatibility with official OData spec, you can chose other implementations.

# Metadata information

OData services implemented using Sql Server REST API library return minimal response format that is compliant to the
OData spec (aka. metadata-none format).
In this library is supported only minimal output format that do not include any metadata information in the REST API body.

# Implement OData service

OData service can be implemented using any .Net project, such ASP.NET Core, ASP.NET Web Api, Azure Function (C#). You just need to reference nuget package in project.json file:

''project.json''
```
{
  "frameworks": {
    "net46":{
      "dependencies": {
        "Antlr4.Runtime": "4.5.3",
        "Sql-Server-Rest-Api": "0.2.7"
      }
    }
   }
}
```
This setting will take Sql Server Rest Api from NuGet and also load Antlr4.Runtime that is used to parse requests. Once you reference these nuget packages, you can create your OData service.

## OData service and ASP.NET Core

You can implement OData service using ASP.NET Core application as a method of any controller. 
First, you need to setup Sql Client in Startup class: 
 - Add the reference to ''SqlServerRestApi'' in Startup class
 - Initialize SqlClient component that will be used to read data from table and return it as OData response.

Example of code is shown in the following listing:

''Startup.cs''
```
using SqlServerRestApi;

namespace MyMvcApp
{
    public class Startup {

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSqlClient(Configuration["ConnectionStrings:azure-db-connection"]);
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
        IQueryPipe pipe = null;
        public PeopleController(IQueryPipe sqlQueryService)
        {
            this.pipe = sqlQueryService;
        }

        /// <summary>
        /// Endpoint that exposes People information using OData protocol.
        /// </summary>
        /// <returns>OData response.</returns>
        // GET api/People/odata
        [HttpGet("odata")]
        public async Task OData()
        {
            var tableSpec = new TableSpec("dbo.People", "name,surname,address,town");
            await this
                    .ODataHandler(tableSpec, pipe)
                    .Process();
        }
    }
}
```

When you run this app and open http://......./api/People/odata Url, you would be able to call all supported functions in OData service.

## OData service and Azure Function

Azure Functions are lightweight components that you can use to create some function in C#, Node.JS, or other languages, and expose the function
as API. This might be combined with OData since you can create single Azure Function that will handle 

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
