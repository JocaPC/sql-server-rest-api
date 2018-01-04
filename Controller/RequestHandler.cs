// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;
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
            await pipe
                .OnError(e => ReturnClientError(response))
                .Stream(cmd, response.Body, "[]");
        }

        public virtual async Task Get()
        {
            response.ContentType = "application/json";
            await pipe
                .OnError(e => ReturnClientError(response))
                .Stream(cmd, response.Body, "[]");
        }

        protected void ReturnClientError(HttpResponse response)
        {
            try
            {
                response.StatusCode = 500;
            }
            catch { };
            try
            {
#if net46 || netcoreapp2_0
                response.Body.Close();
#else
                response.Body.Dispose();
#endif
            }
            catch { };
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
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes("}"), 0, 1);
        }

        public override async Task Get()
        {
            response.ContentType = "application/json";
            var header = Encoding.UTF8.GetBytes(
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":");
            await response.Body.WriteAsync(header, 0, header.Length);
            await pipe.Stream(cmd, response.Body, "[]");
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes("}"), 0, 1);
        }
    }

    public class ODataHandler: RequestHandler
    {
        public enum Metadata { NONE, MINIMAL }
        private readonly TableSpec tableSpec;
        private readonly Metadata metadata;
        private readonly string metadataUrl;
        private bool countOnly = false;

        internal ODataHandler(SqlCommand cmd, IQueryPipe pipe, HttpResponse response, TableSpec tableSpec, string metadataUrl, Metadata metadata = Metadata.NONE, bool countOnly = false) : base(cmd, pipe, response)
        {
            this.tableSpec = tableSpec;
            this.metadata = metadata;
            this.metadataUrl = metadataUrl;
            this.countOnly = countOnly;
        }

        public async override Task Get()
        {
            if (tableSpec.columns.Count == 0)
                throw new Exception("Columns are not defined in table definition for table " + this.tableSpec.Schema + "." + this.tableSpec.Name);
            if (this.countOnly)
            {
                await pipe.Stream(cmd, response.Body, "-1");
            }
            else if(metadata == Metadata.NONE)
            {
                response.ContentType = "application/json;odata.metadata=none;odata=nometadata";
                await pipe
                    .OnError(e => ReturnClientError(response))
                    .Stream(cmd, response.Body, "{\"value\":[]}");
            }
            else
            {
                response.ContentType = "application/json;odata.metadata=minimal";
                await response.WriteAsync("{\"@odata.context\":\""+ this.metadataUrl + "#" + this.tableSpec.Name + "\",\"value\":");
                await pipe
                    .OnError(e => ReturnClientError(response))
                    .Stream(cmd, response.Body, "[]");
                await response.WriteAsync("}");
            }
        }

        public static string GetRootMetadataJsonV4(string ODataMetadataUrl, TableSpec[] tables)
        {
            var sb = new StringBuilder();
            sb.Append("{\"@odata.context\":\"").Append(ODataMetadataUrl).Append("\",\"value\":[");
            foreach (var t in tables)
            {
                sb.Append("{\"name\":\"").Append(ODataMetadataUrl).Append("\",\"kind\":\"EntitySet\",\"url\":\"").Append(ODataMetadataUrl).Append("\"}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        public static string GetMetadataXmlV4(TableSpec[] tables, string Namespace, string DefaultContainerName = "Default")
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

                // Generate Keys
                bool isKeyGenerated = false;
                for (int i = 0; i < tableSpec.columns.Count; i++)
                {
                    var column = tableSpec.columns[i];
                    if (column.IsKey)
                    {
                        if (!isKeyGenerated)
                        {
                            metadata.Append("<Key>");
                            isKeyGenerated = true;
                        }
                        metadata.AppendFormat(@"<PropertyRef Name=""{0}""/>", column.Name);
                    }
                }
                if (isKeyGenerated)
                    metadata.Append("</Key>");

                // Generate properties
                for (int i = 0; i < tableSpec.columns.Count; i++)
                {
                    var column = tableSpec.columns[i];
                    if(column.IsKey)
                        metadata.AppendFormat(@"<Property Name=""{0}"" Type=""{1}"" Nullable=""false""/>", column.Name, SqlTypeToEdmType(column.SqlType));
                    else
                        metadata.AppendFormat(@"<Property Name=""{0}"" Type=""{1}""/>", column.Name, SqlTypeToEdmType(column.SqlType));
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

}