# Sql Server REST API

Sql Server REST API library enables you to easily create REST API using standard T-SQL queries.
In this page you can find out how to create Rest Api service using this library.

## Configure project

1. Get REST API library from [NuGet](https://www.nuget.org/packages/Sql-Server-Rest-Api):
```
PM> Install-Package Sql-Server-Rest-Api
```

2. Put the connection string to your settings file.

''appsettings.json''
```
{
  "ConnectionStrings": {
    "WWI": "Server=.;Database=WideWorldImporters;User=WebApi;Password=Sp1d3rman!"
  }
}
```

3. Initialize data access components in `Startup` class (`ConfigureServices` method) and provide connection string:
```
using SqlServerRestApi;

namespace MyApp {

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSqlClient(Configuration["ConnectionStrings:WWI"]);


			// Add framework services.
            services.AddMvc();
        }
	}
}
```
Assumption in this example is that your connection string is stored in `appsettings.config` file under key `WWI`.

# Create Rest Api Controller

When you call `AddSqlClient`, you will have `ICommand` service that can be used to execute T-SQL queries.
You can add this service to your controllers using standard constructor injection mechanism:

```
using Belgrade.SqlClient;
using SqlServerRestApi;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    public class RestApiController : Controller
    {
        ICommand queryService = null;
        public RestApiController(ICommand sqlQueryService)
        {
            this.queryService = sqlQueryService;
        }

    }
}
```

## Create Rest Api end-point

Once you initialize your controller, you cna start creating REST API endpoints as methods of your controller. 
The following example implements /GetObjects end-point that returns content of sys.object view formatted as JSON response:

```
using Belgrade.SqlClient;
using SqlServerRestApi;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    public class RestApiController : Controller
    {
        public async Task GetObjects()
        {
            await queryService
                .Sql("SELECT * FROM sys.objects FOR JSON PATH")
                .Stream(Response.Body);
        }
    }
}
```
You just need to specify what T-SQL query should be executed to fetch the data and to stream it into the Http response.
Result is an async task that will stream the results of the query into HttpResponse body. 
Content that is produced by T-SQL query will be returned to the client as a response text.
Content can be created using `FOR JSON`, `FOR XML`, or any other query that generates text response.
The following example shows how to create Rest APi that generates CSV content using `STRING_AGG` aggregate:

```
using Belgrade.SqlClient;
using SqlServerRestApi;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    public class RestApiController : Controller
    {
        public async Task GetCsv()
        {
            await queryService
                .Sql(
@"SELECT STRING_AGG(CONCAT_WS( CAST(',' as nvarchar(max)),object_id, name, create_date), CHAR(0x0d)+CHAR(0x0a))
  FROM sys.objects")
                .Stream(Response.Body);
        }
    }
}
```

This library enables you to easily expose content of any T-SQL query that you execute to the web clients.