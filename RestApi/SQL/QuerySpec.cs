﻿// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MsSql.RestApi
{
    [Obsolete("use TSql.RestApi namespace")]
    public class QuerySpec : TSql.RestApi.QuerySpec { }
}

namespace TSql.RestApi
{
    public class QuerySpec
    {
        public string select = "*";
        public bool count = false;
        public int top = -1;
        public int skip = -1;
        public Hashtable columnFilter;
        public LinkedList<SqlParameter> parameters;
        public string predicate;
        public string groupBy;
        public string systemTimeAsOf;
        public Hashtable order;
        internal Dictionary<string,QuerySpec> expand = null;
        internal bool IsOrderClauseValidated = false;
        internal string keyword;
    }
}
