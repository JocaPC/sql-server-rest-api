// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
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
            var header = 
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":";
            await pipe.Stream(cmd, response.Body, new Options() { Prefix = header, DefaultOutput = "[]", Suffix = "}" });
        }

        public override async Task Get()
        {
            response.ContentType = "application/json";
            var header =
$@"{{ 
    ""draw"":""{draw}"",
    ""recordsTotal"":""{start + length + 1}"",
    ""recordsFiltered"":""{start + length + 1}"",
    ""data"":";
            await pipe.Stream(cmd, response.Body, new Options { Prefix = header, DefaultOutput="[]", Suffix = "}"});
        }
    }

}