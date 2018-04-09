// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SqlServerRestApi
{
    public class ColumnSpec
    {
        public string Name;
        public string SqlType;
        public int Size;
        public bool IsKey = false;
    }
    public class TableSpec
    {
        public string Schema;
        public string Name;
        public string FullName;
        public List<ColumnSpec> columns;
        private ISet<string> derivedColumns;
        public string columnList;
        private ISet<string> columnSet;
        public string primaryKey;

        public TableSpec(string schema, string name, string columnList = null, string primaryKey = null)
        {
            this.Name = name;
            this.Schema = schema;
            this.FullName = schema+"."+name;
            this.columnList = columnList;
            this.primaryKey = primaryKey;
            this.columnSet = new HashSet<string>();
            this.columns = new List<ColumnSpec>();
            if (columnList != null)
            {
                foreach (var col in columnList.Split(','))
                {
                    columnSet.Add(col);
                    columns.Add(new ColumnSpec() { Name = col });
                    if(this.primaryKey == null)
                    {
                        this.primaryKey = col;
                    }
                }
            }
        }
        
        public TableSpec AddColumn(string name, string sqlType = null, int typeSize = 0, bool isKeyColumn = false)
        {
            if(columnSet.Contains(name))
                throw new ArgumentException("The column " + name + " already exists in the table.");
            columnSet.Add(name);
            columns.Add(new ColumnSpec() { Name = name, SqlType = sqlType, Size = typeSize, IsKey = isKeyColumn });

            if (string.IsNullOrEmpty(columnList))
                this.columnList = name;
            else
                this.columnList += "," + name;

            if (this.primaryKey == null)
            {
                this.primaryKey = name;
            }

            return this;
        }


        public TableSpec AddDerivedColumn(string name)
        {
            if (derivedColumns == null)
                derivedColumns = new HashSet<string>();
            derivedColumns.Add(name);
            
            return this;
        }

        public void Validate(QuerySpec querySpec, bool parametersAreColumnNames = false)
        {
            if (querySpec.order != null && !querySpec.IsOrderClauseValidated)
            {
                foreach (string column in querySpec.order.Keys)
                {
                    if (!this.columnSet.Contains(column) && !(this.derivedColumns!=null && this.derivedColumns.Contains(column)))
                        throw new ArgumentException("The column " + column + " does not exists in the table.");
                }
            }

            // In JQuery DataTables key in column filter should match column names; however, in general that should not be the case.
            if (parametersAreColumnNames && querySpec.columnFilter != null)
            {
                foreach (string column in querySpec.columnFilter.Keys)
                {
                    if (!this.columnSet.Contains(column))
                        throw new ArgumentException("The column " + column + " does not exists in the table.");
                }
            }
        }

        public void HasColumn(string column)
        {
            if (!this.columnSet.Contains(column))
                throw new ArgumentOutOfRangeException($"Column '{column}' does not belong to the table.");
        }
    }
}
