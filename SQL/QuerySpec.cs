// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections;

namespace SqlServerRestApi.SQL
{
    public class QuerySpec
    {
        public string select = "*";
        public bool count = false;
        public int top;
        public int skip;
        public Hashtable columnFilter;
        public string predicate;
        public Hashtable order;
        internal string keyword;
    }
}
