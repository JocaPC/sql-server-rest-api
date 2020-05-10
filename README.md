# Azure SQL OData API with Dapper

This shows how to expose three OData endpoints using OData protocol and `Dapper.TSql.RestApi` library.

# Setup

Get the sample from [GitHub reporsitory](https://github.com/JocaPC/sql-server-rest-api/tree/dapper-odata-api). you cna donload the sample or get the code from `dapper-odata-api` branch.

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
- https://localhost:5001/odata/objects?$top=2&$filter=type eq 'P'
- https://localhost:5001/odata/objects?$top=10&$expand=columns