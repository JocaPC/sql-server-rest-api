// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
    public static class HttpRequestMessageExtension
    { 

        public static async Task<HttpResponseMessage> CreateODataResponse(
            this HttpRequestMessage req,
            TableSpec tableSpec,
            IQueryPipe sqlQuery)
        {
            var querySpec = OData.UriParser.Parse(tableSpec, req);
            var sql = QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
                sql = sql.AsJson("value");
            return await CreateSqlResponse(req, sqlQuery, sql);
        }

        public static async Task<HttpResponseMessage> CreateSqlResponse(
            this HttpRequestMessage req,
            IQueryPipe pipe,
            SqlCommand sql
        )
        {
            var httpStatus = HttpStatusCode.OK;
            using (var sw = new StringWriter())
            {
                pipe.OnError(ex => httpStatus = HttpStatusCode.InternalServerError);
                await pipe.Stream(sql, sw, "[]");
                return new HttpResponseMessage() { Content = new StringContent(sw.ToString()), StatusCode = httpStatus };
            }
        }

        public static async Task<HttpResponseMessage> CreateODataResponse(
            this HttpRequestMessage req,
            TableSpec tableSpec,
            IQueryMapper mapper)
        {
            var querySpec = OData.UriParser.Parse(tableSpec, req);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson("value");
            return await CreateSqlResponse(req, mapper, sql);
        }

        public static async Task<HttpResponseMessage> CreateSqlResponse(
            this HttpRequestMessage req,
            IQueryMapper mapper,
            SqlCommand sql
        )
        {
            var httpStatus = HttpStatusCode.OK;
            string body = await mapper
                                .OnError(ex => { httpStatus = HttpStatusCode.InternalServerError; })
                                .GetStringAsync(sql);

            return new HttpResponseMessage() { Content = new StringContent(body), StatusCode = httpStatus };
        }
    }
}