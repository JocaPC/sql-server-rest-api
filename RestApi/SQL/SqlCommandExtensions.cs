// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections;
using System.Data.SqlClient;
using System.Text;

namespace MsSql.RestApi
{
    public static class SqlCommandExtensions
    {
        public static SqlCommand AsJson(this SqlCommand cmd)
        {
            cmd.CommandText += " FOR JSON PATH";
            return cmd;
        }

        public static SqlCommand AsJson(this SqlCommand cmd, string root)
        {
            cmd.CommandText += " FOR JSON PATH, ROOT('"+root+"')";
            return cmd;
        }

        public static SqlCommand AsSingleJson(this SqlCommand cmd)
        {
            cmd.CommandText += " FOR JSON PATH, WITHOUT_ARRAY_WRAPPER";
            return cmd;
        }
    }
}
