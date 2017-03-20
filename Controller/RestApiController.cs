// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
    public class RequestHandler
    {
        protected SqlCommand cmd;
        protected IQueryPipe pipe;
        protected Stream stream;

        internal RequestHandler(SqlCommand cmd, IQueryPipe pipe, Stream stream)
        {
            this.cmd = cmd;
            this.pipe = pipe;
            this.stream = stream;
        }

        public virtual RequestHandler OnError(Action<Exception> onErrorHandler)
        {
            pipe.OnError(onErrorHandler);
            return this;
        }

        public virtual async Task Process()
        {
            await pipe.Stream(cmd, stream, "[]");
        }
    }

    public class JQueryDataTablesHandler : RequestHandler
    {
        private string draw;
        private int length;
        private int start;

        internal JQueryDataTablesHandler(SqlCommand cmd, string draw, int start, int length, IQueryPipe pipe, Stream stream): base(cmd, pipe, stream)
        {
            this.draw = draw;
            this.start = start;
            this.length = length;
        }
        
        public override async Task Process()
        {
            var header = System.Text.Encoding.UTF8.GetBytes(
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":");
            await stream.WriteAsync(header, 0, header.Length);
            await pipe.Stream(cmd, stream, "[]");
            await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("}"), 0, 1);
        }
    }

    public class ODataHandler: RequestHandler
    {
        internal ODataHandler(SqlCommand cmd, IQueryPipe pipe, Stream stream): base(cmd, pipe, stream)
        {
        }
    }

    public static class RestApiControllerExtensions 
    {
        public static RequestHandler ODataHandler(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var querySpec = SqlServerRestApi.OData.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = SqlServerRestApi.QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
                sql = sql.AsJson("value");
            return new ODataHandler(sql, sqlQuery, ctrl.Response.Body);
        }

        public static RequestHandler JQueryDataTablesHandler(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var querySpec = SqlServerRestApi.JQueryDataTable.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = SqlServerRestApi.QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
                sql = sql.AsJson();
            return new JQueryDataTablesHandler(sql, ctrl.Request.Query["draw"].ToString(), Convert.ToInt32(ctrl.Request.Query["start"]), Convert.ToInt32(ctrl.Request.Query["length"]), sqlQuery, ctrl.Response.Body);
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