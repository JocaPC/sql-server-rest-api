// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
    public class RequestHandler
    {
        protected SqlCommand cmd;
        protected IQueryPipe pipe;
        protected HttpResponse response;

        internal RequestHandler(SqlCommand cmd, IQueryPipe pipe, HttpResponse response)
        {
            this.cmd = cmd;
            this.pipe = pipe;
            this.response = response;
        }

        public virtual RequestHandler OnError(Action<Exception> onErrorHandler)
        {
            pipe.OnError(onErrorHandler);
            return this;
        }

        public virtual async Task Process()
        {
            await pipe.Stream(cmd, response.Body, "[]");
        }


        public virtual async Task Get()
        {
            response.ContentType = "application/json";
            await pipe.Stream(cmd, response.Body, "[]");
        }
    }

    public class JQueryDataTablesHandler : RequestHandler
    {
        private string draw;
        private int length;
        private int start;

        internal JQueryDataTablesHandler(SqlCommand cmd, string draw, int start, int length, IQueryPipe pipe, HttpResponse response): base(cmd, pipe, response)
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
            await response.Body.WriteAsync(header, 0, header.Length);
            await pipe.Stream(cmd, response.Body, "[]");
            await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("}"), 0, 1);
        }

        public override async Task Get()
        {
            response.ContentType = "application/json";
            var header = System.Text.Encoding.UTF8.GetBytes(
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":");
            await response.Body.WriteAsync(header, 0, header.Length);
            await pipe.Stream(cmd, response.Body, "[]");
            await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("}"), 0, 1);
        }
    }

    public class ODataHandler: RequestHandler
    {
        public enum Metadata { NONE, MINIMAL }
        private readonly TableSpec tableSpec;
        private readonly Metadata metadata;
        private readonly string metadataUrl;

        internal ODataHandler(SqlCommand cmd, IQueryPipe pipe, HttpResponse response, TableSpec tableSpec, string metadataUrl, Metadata metadata = Metadata.NONE) : base(cmd, pipe, response)
        {
            this.tableSpec = tableSpec;
            this.metadata = metadata;
            this.metadataUrl = metadataUrl;
        }

        public async override Task Get()
        {
            if (metadata == Metadata.NONE)
            {
                response.ContentType = "application/json;odata.metadata=none;odata=nometadata";
                await pipe.Stream(cmd, response.Body, "{\"value\":[]}");
            }
            else
            {
                response.ContentType = "application/json;odata.metadata=minimal";
                await response.WriteAsync("{\"@odata.context\":\""+ this.metadataUrl + "/$metadata#" + this.tableSpec.Name + "\",\"value\":");
                await pipe.Stream(cmd, response.Body, "[]");
                await response.WriteAsync("}");
            }
            
        }

        public static string GetMetadataV4(TableSpec[] tables, string Namespace, string DefaultContainerName = "Default")
        {
            var metadata = new StringBuilder();
            metadata
                .AppendFormat(@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
    <edmx:DataServices>
        <Schema Namespace=""{0}"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">", Namespace);
            foreach (var tableSpec in tables) {
                metadata.AppendFormat(@"
            <EntityType Name=""{0}"">", tableSpec.Name);
                for (int i = 0; i < tableSpec.columns.Count; i++)
                {
                    var column = tableSpec.columns[i];
                    if (column.IsKey)
                        metadata.AppendFormat(@"
                <Key><PropertyRef Name=""{0}""/></Key>", column.Name);
                    metadata.AppendFormat(@"
                <Property Name=""{0}"" Type=""{1}""/>", column.Name, SqlTypeToEdmType(column.SqlType));
                }

                metadata
                    .AppendFormat(@"
            </EntityType>");
            }
        
            metadata.AppendFormat(@"
            <EntityContainer Name=""{0}"">", DefaultContainerName);
            foreach (var tableSpec in tables)
            {
                metadata.AppendFormat(@"
                <EntitySet Name=""{0}"" EntityType=""{1}"" />", tableSpec.Name, Namespace + "." + tableSpec.Name);
            }
            metadata.Append(@"
            </EntityContainer>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>");

            return metadata.ToString();
        }


private static string SqlTypeToEdmType(string sqlType)
        {
            switch (sqlType)
            {
                case "bigint": return "Edm.Int64";
                case "binary": return "Edm.Byte[]";
                case "bit": return "Edm.Boolean";
                case "char": return "Edm.String";
                case "date": return "Edm.DateTime";
                case "datetime": return "Edm.DateTime";
                case "datetime2": return "Edm.DateTime";
                case "datetimeoffset": return "Edm.DateTimeOffset";
                case "decimal": return "Edm.Decimal";
                case "float": return "Edm.Double";
                case "image": return "Edm.Byte[]";
                case "int": return "Edm.Int32";
                case "money": return "Edm.Decimal";
                case "nchar": return "Edm.String";
                case "ntext": return "Edm.String";
                case "numeric": return "Edm.Decimal";
                case "nvarchar": return "Edm.String";
                case "real": return "Edm.Single";
                case "rowversion": return "Edm.Byte[]";
                case "smalldatetime": return "Edm.DateTime";
                case "smallint": return "Edm.Int16";
                case "smallmoney": return "Edm.Decimal";
                case "sql_variant": return "Edm.Object";
                case "text": return "Edm.String";
                case "time": return "Edm.TimeSpan";
                case "timestamp": return "Edm.Byte[]";
                case "tinyint": return "Edm.Byte";
                case "uniqueidentifier": return "Edm.Guid";
                case "varbinary": return "Edm.Byte[]";
                case "varchar": return "Edm.String";
                case "xml": return "Edm.Xml";
                default: return "Edm.String"; // throw new ArgumentException("Unsupported type: " + sqlType, "sqlType");
            }
        }
    }

    public static class RestApiControllerExtensions 
    {
        public static RequestHandler ODataHandler(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery,
            ODataHandler.Metadata metadata = SqlServerRestApi.ODataHandler.Metadata.NONE,
            string metadataUrl = null)
        {
            var querySpec = OData.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
            {
                if (metadata == SqlServerRestApi.ODataHandler.Metadata.NONE)
                    sql = sql.AsJson("value");
                else
                    sql = sql.AsJson();
            }
            return new ODataHandler(sql, sqlQuery, ctrl.Response, tableSpec, metadataUrl??((ctrl.Request.Scheme + "://" + ctrl.Request.Host + ctrl.Request.Path.Value.Replace("/"+tableSpec.Name, ""))), metadata);
        }

        public static RequestHandler JQueryDataTablesHandler(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var querySpec = JQueryDataTable.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
                sql = sql.AsJson();
            return new JQueryDataTablesHandler(sql, ctrl.Request.Query["draw"].ToString(), Convert.ToInt32(ctrl.Request.Query["start"]), Convert.ToInt32(ctrl.Request.Query["length"]), sqlQuery, ctrl.Response);
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