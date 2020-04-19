using MsSql.RestApi.DAO;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MsSql.RestApi
{
    public class ErrorResponseHandler : RequestHandler
    {
        Exception ex;
        internal ErrorResponseHandler(HttpResponse response, Exception ex) : base(null, response, true)
        {
            this.ex = ex;
        }

        public override async Task Process(TSqlCommand pipe)
        {
            await ReturnClientError(response, ex);
        }

    }
}
