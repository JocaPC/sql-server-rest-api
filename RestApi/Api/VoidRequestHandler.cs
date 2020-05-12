using TSql.RestApi;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace TSql.RestApi
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
