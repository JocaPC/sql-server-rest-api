// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MsSql.RestApi;

namespace MsSql.TableApi
{
    public class JQueryDataTablesHandler : RequestHandler
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
        
        public override async Task Process(IQueryPipe pipe)
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