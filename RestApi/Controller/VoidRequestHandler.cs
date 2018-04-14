using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;

namespace SqlServerRestApi
{
    public class ErrorResponseHandler : RequestHandler
    {
        Exception ex;
        internal ErrorResponseHandler(HttpResponse response, Exception ex) : base(null, null, response, true)
        {
            this.ex = ex;
        }

        public override async Task Process(bool useDefaultContentType = true)
        {
            await ReturnClientError(response, ex);
        }

    }
}
