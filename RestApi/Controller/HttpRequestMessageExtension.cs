// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MsSql.RestApi
{
#if net47 || net46 || NETCOREAPP3_1
    public static class HttpRequestMessageExtension
    { 

        public static async Task<HttpResponseMessage> CreateODataResponse(
#if NETCOREAPP3_1
            this Microsoft.AspNetCore.Http.HttpRequest req,
#else
            this System.Net.Http.HttpRequestMessage req,
#endif
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
#if NETCOREAPP3_1
            this Microsoft.AspNetCore.Http.HttpRequest req,
#else
            this System.Net.Http.HttpRequestMessage req,
#endif
            IQueryPipe pipe,
            SqlCommand sql
        )
        {
            var httpStatus = HttpStatusCode.OK;
            using (var sw = new StringWriter())
            {
                pipe.OnError(ex => httpStatus = HttpStatusCode.InternalServerError);
                await pipe.Sql(sql).Stream(sw, "[]");
                return new HttpResponseMessage() { Content = new StringContent(sw.ToString()), StatusCode = httpStatus };
            }
        }

        public static async Task<HttpResponseMessage> CreateODataResponse(
#if NETCOREAPP3_1
            this Microsoft.AspNetCore.Http.HttpRequest req,
#else
            this System.Net.Http.HttpRequestMessage req,
#endif
            TableSpec tableSpec,
            IQueryMapper mapper)
        {
            var querySpec = OData.UriParser.Parse(tableSpec, req);
            var sql = QueryBuilder.Build(querySpec, tableSpec).AsJson("value");
            return await CreateSqlResponse(req, mapper, sql);
        }

        public static async Task<HttpResponseMessage> CreateSqlResponse(
#if NETCOREAPP3_1
            this Microsoft.AspNetCore.Http.HttpRequest req,
#else
            this System.Net.Http.HttpRequestMessage req,
#endif
            IQueryMapper mapper,
            SqlCommand sql
        )
        {
            var httpStatus = HttpStatusCode.OK;
            string body = await mapper
                                .OnError(ex => { httpStatus = HttpStatusCode.InternalServerError; })
#if NETCOREAPP3_1
                                .GetString(sql);
#else
                                .GetStringAsync(sql);
#endif

            return new HttpResponseMessage() { Content = new StringContent(body), StatusCode = httpStatus };
        }
    }
#endif
    }