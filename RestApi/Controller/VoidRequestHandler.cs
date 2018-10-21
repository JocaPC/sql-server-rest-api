using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;

namespace MsSql.RestApi
{
    public class ErrorResponseHandler : RequestHandler
    {
        Exception ex;
        internal ErrorResponseHandler(HttpResponse response, Exception ex) : base(null, response, true)
        {
            this.ex = ex;
        }

        public override async Task Process(IQueryPipe pipe, bool useDefaultContentType = true)
        {
            await ReturnClientError(response, ex);
        }

    }
}
