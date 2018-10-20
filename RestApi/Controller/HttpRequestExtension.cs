// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;

namespace SqlServerRestApi
{
    public static class HttpRequestMessageExtension
    {
        private static ILog _log = null;
        public static RequestHandler OData(
            this HttpRequest req,
            TableSpec tableSpec,
            IQueryPipe sqlQuery,
            HttpResponse res,
            ODataHandler.Metadata metadata = ODataHandler.Metadata.NONE,
            string metadataUrl = null,
            object id = null)
        {
            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();
            try
            {
                var querySpec = SqlServerRestApi.OData.UriParser.Parse(tableSpec, req);
                if (id != null)
                {
                    querySpec.predicate = tableSpec.primaryKey + " = @Id";
                    var p = new SqlParameter("Id", id);
                    if (querySpec.parameters == null)
                    {
                        querySpec.parameters = new System.Collections.Generic.LinkedList<SqlParameter>();
                    }
                    querySpec.parameters.AddFirst(p);
                }
                var sql = QueryBuilder.Build(querySpec, tableSpec);

                if (id != null)
                {
                    sql = sql.AsSingleJson();
                }
                else if (!querySpec.count)
                {
                    if (metadata == ODataHandler.Metadata.NONE)
                        sql = sql.AsJson("value");
                    else
                        sql = sql.AsJson();
                }

                return new ODataHandler(sql, sqlQuery, res, tableSpec,
                    metadataUrl, metadata,
                    countOnly: querySpec.count,
                    returnSingleResult: (id != null));
            }
            catch (Exception ex)
            {
                return new ErrorResponseHandler(res, ex);
            }
        }

    }
}