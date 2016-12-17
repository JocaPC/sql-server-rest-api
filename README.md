# Sql Server REST API

This library enables you to easily create REST services in ASP.NET based on the existing tables or SQL queries in SQL Server.

# Setup

Get REST API library from NuGet:
```
PM> Install-Package Sql-Server-Rest-Api
```

You will need to configure data access components in Startup class (Configure service method):

```
using SqlServerRestApi.SQL;


    public class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSqlClient(Configuration["ConnectionStrings:MyConnection"]);
        }

```
Assumption in this example is that your connection string is stored in application.config file under key MyConnection.

# Create ASP.NET Controller that will serve Http requests

You need to create standard ASP.NET Controller that will handle Http requests from the clients (e.g. browsers).
As a first step you need to setup IQueryPipe interface that will be used to execute queries against the database. Usually you will use standard dependency injection to initialize member of controller that will be used as data access component:

```
using Belgrade.SqlClient;
using System.Threading.Tasks;

namespace Catalog.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        IQueryPipe sqlQuery = null;

        public PeopleController(IQueryPipe sqlQueryService)
        {
            this.sqlQuery = sqlQueryService;
        }

```

## Implement REST API method

Now you need to create async method that will serve requests. The only thing that you need to do is to call Stream method of IQueryPipe interface, provide query that should be executed in database, the output stream (HttpResponse.Body) where results of the query should be sent, and the text that should be returned to the client if SQL query don't return any result.

```
        // GET api/People/Load
        [HttpGet("Load")]
        public async Task Load()
        {
            await sqlQuery.Stream("select * from people for json path", Response.Body, "[]");
        }
```

Note that FOR JSON clause is used in SQL query. FOR JSON clause will generate JSON output from SQL Server instead of tabular result set. This method will just stream JSON result returned by query into Response.Body.

## Implement OData service

To implement OData Service, you would need to add the TableSpec object that describes the structure of the table that will be queried (name and columns). An example is shown in the following code:
```
        IQueryPipe sqlQuery = null;
        
        TableSpec tableSpec = new TableSpec("dbo.People", "name,surname,address,town");
        
        public PeopleController(IQueryPipe sqlQueryService)
        {
            this.sqlQuery = sqlQueryService;
        }
```

Now you need to create async method that will serve OData requests with following classes:
 - UriParser that will parse OData Http request and extract information from $select, $filter, $orderby, $top, and $skip parameters
 - QueryBuilder that will create T-SQL query that will be executed. 
First, you need to parse Request parameters using UriParser in order to extract the definition of query (QuerySpec object). Then you need to use QueryBuilder to create SQL query using the QuerySpec. Then you need to provide sql query to QueryPipe that will stream results to client using Response.Body:

```
       // GET api/People
        [HttpGet]
        public async Task Get()
        {            
            var querySpec = OData.UriParser.Parse(tableSpec, this.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson();
            await sqlQuery.Stream(sql, Response.Body, "[]");
        }
 ```

That's everything that you need to do. With three lines of code you can get OData service on any table.