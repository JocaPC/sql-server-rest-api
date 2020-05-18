# Database-driven SQL OData API with Belgrade SQL data access library

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

This sample application exposes information about the system objects, columns, and parameters in your database. You can easily add new OData web services that expose informaiton from your tables.

To add new OData endpoints you need to create new MVC action methods in `ODataController` class (or create new controller class where you want create OData action methods). Use some ofthe existing methods as a template, copy, paste and modify the code by entering information about the table that you want to expose.

In the example below you can see how to add a new OData REST endpoint for `Application.People` table that exposes the columns `PersonID`, `FirstName`, and `LastName`.
```
public async Task People() {
    var tableSpec = new TableSpec(schema: "Application", table: "People", "PersonID,FirstName,LastName");
    await this
            .OData(tableSpec)
            .Process(DbCommand);
}
```

You need to set schema and name of you table with a list of table columns that you want to expose. One you run the modified application and open `https://localhost:5001/odata/People` URL you will be able to access the rows from your table.

## Deployment to Azure Service

Use the following [deployment template](https://github.com/Azure/azure-quickstart-templates/tree/master/201-web-app-github-deploy) to deploy
code from GitHub repository with the following parameters:
- Repository URL: https://github.com/JocaPC/sql-server-rest-api
- branch: belgrade-odata-api 

Once the application deployment is finished, add connection string in portal using Configure > Connection strings and call it `Database`
