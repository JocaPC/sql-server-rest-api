// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace MsSql.RestApi.DAO
{
    public class Options
    {
        public Options()
        {
        }

        public string Prefix { get; set; }
        public string DefaultOutput { get; set; }
        public string Suffix { get; set; }
    }
}