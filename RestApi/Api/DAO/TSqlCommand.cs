using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace MsSql.RestApi.DAO
{
    [Obsolete("Use TSql.RestApi namespace")]
    public abstract class TSqlCommand {    }
}

namespace TSql.RestApi
{
    public abstract class TSqlCommand
    {
        public abstract TSqlCommand Sql(SqlCommand cmd);

        public TSqlCommand Sql(string cmd) { return Sql(new SqlCommand(cmd)); }

        public abstract TSqlCommand OnError(Action<Exception> handler);

        public abstract Task Stream(Stream output, string defaultOnNoResult = "");

        public abstract Task Stream(StringWriter output, string defaultOnNoResult);

        public abstract Task<string> GetString(string defaultOnNoResult = "");

        public abstract Task Stream(Stream body, Options options);
    }
}
