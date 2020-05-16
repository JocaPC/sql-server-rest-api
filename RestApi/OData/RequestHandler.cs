// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using TSql.RestApi;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace MsSql.RestApi
{
    [Obsolete("use TSql.RestApi namespace")]
    public class ODataHandler : TSql.RestApi.ODataHandler {
        internal ODataHandler(SqlCommand cmd, HttpResponse response, TableSpec tableSpec, string metadataUrl, Metadata metadata = Metadata.NONE, bool countOnly = false, bool returnSingleResult = false) :
                base(cmd, response, tableSpec, metadataUrl, metadata, countOnly, returnSingleResult)
        { } 
    }
}

namespace TSql.RestApi
{
    public class ODataHandler: RequestHandler
    {
        public enum Metadata { NONE, MINIMAL }
        private readonly TableSpec tableSpec;
        private readonly Metadata metadata;
        private readonly string metadataUrl;
        private bool countOnly = false;

        internal ODataHandler(SqlCommand cmd, HttpResponse response, TableSpec tableSpec, string metadataUrl, Metadata metadata = Metadata.NONE, bool countOnly = false, bool returnSingleResult = false) :
            base(cmd, response, returnSingleResult)
        {
            this.tableSpec = tableSpec;
            this.metadata = metadata;
            this.metadataUrl = metadataUrl;
            this.countOnly = countOnly;
        }

        public override async Task Process(TSqlCommand pipe)
        {
            if (this.countOnly)
            {
                await pipe.Sql(cmd).Stream(response.Body, "-1");
            }
            else if (metadata == Metadata.NONE)
            {
                response.ContentType = "application/json;odata.metadata=none;odata=nometadata";
                await pipe
                .Sql(cmd)
                .OnError(async e => await ReturnClientError(response, e))
                .Stream(response.Body, IsSingletonResponse ? "{}" : "{\"value\":[]}");
            }  else if (metadata == Metadata.MINIMAL)
            {
                response.ContentType = "application/json;odata.metadata=minimal";
                var header = "{\"@odata.context\":\"" + this.metadataUrl + "#" + this.tableSpec.Name + "\",\"value\":";
                await pipe
                    .Sql(cmd)
                    .OnError(async e => await ReturnClientError(response, e))
                    .Stream(response.Body, new Options() { Prefix = header, DefaultOutput = "[]", Suffix = "}" });
            } else
            {
                await ReturnClientError(response, new InvalidOperationException("Cannot generate response for metadata type: " + metadata)); 
            }
        }
        
        public static string GetRootMetadataJsonV4(string ODataMetadataUrl, TableSpec[] tables)
        {
            var sb = new StringBuilder();
            sb.Append("{\"@odata.context\":\"").Append(ODataMetadataUrl).Append("\",\"value\":[");
            for (int i = 0; i < tables.Length; i++)
            {
                var t = tables[i];
                sb.Append("{\"name\":\"").Append(t.Name).Append("\",\"kind\":\"EntitySet\",\"url\":\"").Append(t.Name).Append("\"}");
                if(i<tables.Length-1)
                    sb.Append(",");
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