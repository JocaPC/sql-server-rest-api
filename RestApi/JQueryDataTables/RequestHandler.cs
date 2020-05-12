// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TSql.RestApi;
using System;

namespace MsSql.TableApi
{
    [Obsolete("use TSql.RestApi namespace")]
    public class JQueryDataTablesHandler : TSql.TableApi.JQueryDataTablesHandler
    {
        internal JQueryDataTablesHandler(SqlCommand cmd, string draw, int start, int length, HttpResponse response)
            : base(cmd, draw, start, length, response) { }
    }
}

namespace TSql.TableApi
{
    public class JQueryDataTablesHandler : TSql.RestApi.RequestHandler
    {
        private string draw;
        private int length;
        private int start;

        internal JQueryDataTablesHandler(SqlCommand cmd, string draw, int start, int length, HttpResponse response): base(cmd, response)
        {
            this.draw = draw;
            this.start = start;
            this.length = length;
        }
        
        public override async Task Process(TSqlCommand pipe)
        {
            response.ContentType = "application/json";
            var header = 
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":";
            await pipe
                .Sql(cmd)
                .Stream(response.Body, new Options() { Prefix = header, DefaultOutput = "[]", Suffix = "}" });
        }
    }

}