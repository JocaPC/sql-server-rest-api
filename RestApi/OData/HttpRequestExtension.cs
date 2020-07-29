// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;

namespace TSql.RestApi
{
    public static partial class HttpRequestExtension
    {
        private static ILog _log = null;
        
        public static RequestHandler OData(
            this HttpRequest req,
            TableSpec tableSpec,
            HttpResponse res = null,
            ODataHandler.Metadata metadata = ODataHandler.Metadata.NONE,
            string metadataUrl = null,
            object id = null)
        {
            try
            {
                QuerySpec querySpec;
                SqlCommand sql;
                Initialize(req, tableSpec, metadata, id, out querySpec, out sql);

                return new ODataHandler(sql, res??req.HttpContext.Response, tableSpec,
                    metadataUrl, metadata,
                    countOnly: querySpec.count,
                    returnSingleResult: (id != null));
            }
            catch (Exception ex)
            {
                if (res == null)
                    res = req.HttpContext.Response;
                return new ErrorResponseHandler(res, ex);
            }
        }

        private static void Initialize(HttpRequest req, TableSpec tableSpec, ODataHandler.Metadata metadata, object id, out QuerySpec querySpec, out SqlCommand sql)
        {
            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();

            querySpec = TSql.OData.UriParser.Parse(tableSpec, req);
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
            sql = QueryBuilder.Build(querySpec, tableSpec);
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
        }
    }
}