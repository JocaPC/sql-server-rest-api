// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServerRestApi.SQL
{
    public class TableSpec
    {
        public string FullName;
        public string[] columns;
        public string columnList;
        private ISet<string> columnSet;

        public TableSpec(string fullTableName, string columnList) {
            this.FullName = fullTableName;
            this.columnList = columnList;
            this.columns = columnList.Split(',');
            this.columnSet = new HashSet<string>(columns);
        }

        public void Validate(QuerySpec querySpec, bool parametersAreColumnNames = false)
        {
            if (querySpec.order != null)
            {
                foreach (string column in querySpec.order.Keys)
                {
                    if (this.columns.All(col => col != column))
                        throw new ArgumentException("Column " + column + "does not exists in table.");
                }
            }

            // In JQuery DataTables key in column filter should match column names; however, in general that should not be the case.
            if (parametersAreColumnNames && querySpec.columnFilter != null)
            {
                foreach (string column in querySpec.columnFilter.Keys)
                {
                    if (this.columns.All(col => col != column))
                        throw new ArgumentException("Column " + column + "does not exists in table.");
                }
            }
        }

        public void HasColumn(string column)
        {
            if (!this.columnSet.Contains(column))
                throw new ArgumentOutOfRangeException($"Column {column} does not belong to the table.");
        }
    }
}
