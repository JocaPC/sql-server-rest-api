// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using SqlServerRestApi.SQL;
using System;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
    public static class RestApiControllerExtensions 
    {
        public static async Task ProcessODataRequest(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var querySpec = OData.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson();
            await sqlQuery.Stream(sql, ctrl.Response.Body, "[]");
        }
        
        public static async Task ProcessJQueryDataTablesRequest(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var Request = ctrl.Request;
            var draw = Request.Query["draw"].ToString();
            var start = Convert.ToInt32(Request.Query["start"]);
            var length = Convert.ToInt32(Request.Query["length"]);
            var header = System.Text.Encoding.UTF8.GetBytes(
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":");
            await ctrl.Response.Body.WriteAsync(header, 0, header.Length);

            var querySpec = JQueryDataTable.UriParser.Parse(tableSpec, Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson();
            await sqlQuery.Stream(sql, ctrl.Response.Body, "[]");

            await ctrl.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("}"), 0, 1);
        }
    }
}