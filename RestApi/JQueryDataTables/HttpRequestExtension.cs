// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Common.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;
using TSql.TableApi;

namespace TSql.RestApi
{
    public static partial class HttpRequestExtension
    {
        
        public static RequestHandler Table(
            this HttpRequest req,
            TableSpec tableSpec,
            HttpResponse res = null)
        {
            try
            {
                QuerySpec querySpec;
                SqlCommand sql;
                InitializeTable(req, tableSpec, out querySpec, out sql);
                return new JQueryDataTablesHandler(sql, req.Query["draw"].ToString(), Convert.ToInt32(req.Query["start"]), Convert.ToInt32(req.Query["length"]), res ??req.HttpContext.Response);
            }
            catch (Exception ex)
            {
                if (res == null)
                    res = req.HttpContext.Response;
                return new ErrorResponseHandler(res, ex);
            }
        }

        private static void InitializeTable(HttpRequest req, TableSpec tableSpec, out QuerySpec querySpec, out SqlCommand sql)
        {
            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();

            querySpec = TSql.TableApi.UriParser.Parse(tableSpec, req);
            
            sql = QueryBuilder.Build(querySpec, tableSpec);
            sql = sql.AsJson();
        }
    }
}