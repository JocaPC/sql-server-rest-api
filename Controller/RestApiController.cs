using Belgrade.SqlClient;
using SqlServerRestApi.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerRestApi.Controller
{
    public class RestApiController : Microsoft.AspNetCore.Mvc.Controller
    {
        IQueryPipe sqlQuery = null;
        TableSpec tableSpec = null;

        public RestApiController(TableSpec tableSpec, IQueryPipe sqlQueryService)
        {
            this.sqlQuery = sqlQueryService;
            this.tableSpec = tableSpec;
        }

        public async Task ProcessODataRequest(string defaultOutput)
        {
            var querySpec = OData.UriParser.Parse(this.tableSpec, this.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson();
            await sqlQuery.Stream(sql, Response.Body, defaultOutput);
        }

        
        public async Task ProcessJQueryDataTablesRequest(string defaultOutput)
        {
            var querySpec = JQueryDataTable.UriParser.Parse(this.tableSpec, this.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson();
            await sqlQuery.Stream(sql, Response.Body, defaultOutput);
        }
    }
}
