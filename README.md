# Database-driven SQL OData min-metadata API with Belgrade SQL data access library

This sample application shows how to expose three OData endpoints using OData protocol and `Belgrade.TSql.RestApi` library. With this sample application you can create OData REST API on any database engine that uses latest T-SQL language, for example, SQL Server 2016+, Azure SQL, or Synapse SQL.

## Setup

Get the sample from [GitHub repository](https://github.com/JocaPC/sql-server-rest-api/tree/belgrade-odata-api). You can download the sample or get the code from `belgrade-odata-api` branch.

Open 'appsettings.json` file and change the connection string:

```
  "ConnectionStrings": {
    "Database": "Server=.;Database=master;Integrated Security=True"
  }
```

## Run sample

Open the folder using command line and run the following command `dotnet run`. As an alternative you can
open project in Visual Studio or visual Studio code and start it from there.

Open some of the following URL locations:
- https://localhost:5001/odata/objects
- https://localhost:5001/odata/columns
- https://localhost:5001/odata/parameters
- [https://localhost:5001/odata/objects?$top=2&$filter=type eq 'P'](https://localhost:5001/odata/objects?$top=2&$filter=type eq 'P')
- https://localhost:5001/odata/objects?$top=10&$expand=columns

## Modify sample

This sample application exposes information about the system objects, columns, and parameters in your database.
You can easily add new OData web services that expose information from your tables. 

To add new OData endpoints you need to:

1. Add table definition in some arraay (see tables static property in sample).
The following SQL query might generate table metadata from SQL database:

```
select CONCAT('new TableSpec("',schema_name(t.schema_id), '","', t.name, '")') +
	string_agg(CONCAT('.AddColumn("', c.name, '", "', tp.name, '"', 
						case when tp.name LIKE '%char%' AND c.max_length <> -1 then concat(',',c.max_length) else '' end,
						IIF(ix.is_primary_key = 1, ', isKeyColumn:true', ''), ')'),'')
from sys.objects t
	join sys.all_columns c on t.object_id = c.object_id
	join sys.types tp on c.system_type_id = tp.system_type_id
	left join sys.index_columns ic on c.column_id = ic.column_id and c.object_id = ic.object_id
	left join sys.indexes ix on ic.index_id = ix.index_id and ic.object_id = ix.object_id
--where t.name in ('','','') --> specify target tables if needed.
group by t.schema_id, t.name
```

2. Create new MVC action method in `ODataController` class and use `metadata: ODataHandler.Metadata.MINIMAL` parameter. 
In the example below you can see how to add a new OData REST endpoint for `Application.People` table that exposes the columns `PersonID`, `FirstName`, and `LastName`.
```
public async Task People() {
    var tableSpec = new TableSpec(schema: "Application", table: "People", "PersonID,FirstName,LastName");
    await this
            .OData(table, metadata: ODataHandler.Metadata.MINIMAL)
            .Process(DbCommand);
}
```

You need to set schema and name of you table with a list of table columns that you want to expose. One you run the modified application and open `https://localhost:5001/odata/People` URL you will be able to access the rows from your table.

## Setup new OData API

If you need to create new controller, you need to setup API endpoints that return the following metadata:

1. One root location that returns OData service metadata based on the list of tables that should be exposed. This method shoudl call `GetODataServiceDocumentJsonV4` and provide array of tables that should be exposed and URL of metadata API.
2. Setup metadata that will return XML metadata document that returns metadata information about the entities.

> IMPORTANT: URL that us isued for XML metadata MUST match second parameters in `GetODataServiceDocumentJsonV4` call in 1)

Here is example how to setup metadata:

```
        public ActionResult Index()
        {
            return this.GetODataServiceDocumentJsonV4(tables, "odata/$metadata");
        }

        [HttpGet("odata/$metadata")]
        public ActionResult Metadata()
        {
            return this.GetODataMetadataXmlV4(tables);
        }
``


## Deployment to Azure Service

Use the following [deployment template](https://github.com/Azure/azure-quickstart-templates/tree/master/201-web-app-github-deploy) to deploy
code from GitHub repository with the following parameters:
- Repository URL: https://github.com/JocaPC/sql-server-rest-api
- branch: belgrade-odata-api 

Once the application deployment is finished, add connection string in portal using Configure > Connection strings and call it `Database`
