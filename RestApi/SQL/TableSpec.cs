// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Common.Logging;
using System;
using System.Collections.Generic;

namespace MsSql.RestApi
{
    [Obsolete("use TSql.RestApi namespace")]
    public class ColumnSpec : TSql.RestApi.ColumnSpec { }

    [Obsolete("use TSql.RestApi namespace")]
    public class TableSpec : TSql.RestApi.TableSpec {
        public TableSpec(string schema, string table, string columns = null, string primaryKey = null)
            : base(schema, table, columns, primaryKey) { }
    }
}

namespace TSql.RestApi
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

        internal void InferPrimaryKey()
        {
            ColumnSpec primaryKeyColumn = null;
            if (string.IsNullOrEmpty(this.primaryKey))
                foreach (var c in this.columns)
                {
                    if (c.IsKey)
                    {
                        this.primaryKey = c.Name;
                        return;
                    }
                    if (c.Name.ToLower() == "id" ||
                        this.Name.ToLower() + c.Name.ToLower() == this.Name.ToLower() + "id" ||
                        this.Name.ToLower() + "_" + c.Name.ToLower() == this.Name.ToLower() + "_id")
                    {
                        this.primaryKey = c.Name;
                        primaryKeyColumn = c;
                    }
                }
            if (primaryKeyColumn != null)
                primaryKeyColumn.IsKey = true;
        }

        public string primaryKey;
        public Dictionary<string, TableSpec> Relations = null;

        private static ILog _log = null;

        public TableSpec(string schema, string table, string columns = null, string primaryKey = null)
        {
            this.Name = table;
            this.Schema = schema;
            this.FullName = string.IsNullOrEmpty(schema)? table : (schema+"."+table);
            this.columnList = columns;
            this.primaryKey = primaryKey;
            this.columnSet = new HashSet<string>();
            this.columns = new List<ColumnSpec>();
            if (columns != null)
            {
                foreach (var col in columns.Split(','))
                {
                    columnSet.Add(col);
                    this.columns.Add(new ColumnSpec() { Name = col });
                    if(this.primaryKey == null)
                    {
                        this.primaryKey = col;
                    }
                }
            }

            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();
        }
        
        public TableSpec AddRelatedTable(string relation, string schema, string name, string joinCondition, string columns = null)
        {
            if (this.Relations == null)
                this.Relations = new Dictionary<string, TableSpec>();

            if (Relations.ContainsKey(relation))
            {
                if (_log != null) _log.ErrorFormat("The relation {relation} already exists in the {table}.", name, this);
                throw new ArgumentException("The relation " + name + " already exists in the table.");
            }
            this.Relations.Add(relation, new TableSpec(schema, name, columns, primaryKey: joinCondition));
            return this;
        }

        public TableSpec AddRelatedTable(string relation, TableSpec table)
        {
            if (this.Relations == null)
                this.Relations = new Dictionary<string, TableSpec>();

            if (Relations.ContainsKey(relation))
            {
                if (_log != null) _log.ErrorFormat("The relation {relation} already exists in the {table}.", relation, this.FullName);
                throw new ArgumentException("The relation " + relation + " already exists in the table.");
            }
            this.Relations.Add(relation, table);
            return this;
        }

        public TableSpec AddColumn(string name, string sqlType = null, int typeSize = 0, bool isKeyColumn = false)
        {
            if (columnSet.Contains(name))
            {
                if (_log != null) _log.ErrorFormat("The column {column} already exists in the {table}.", name, this);
                throw new ArgumentException("The column " + name + " already exists in the table.");
            }
            columnSet.Add(name);
            columns.Add(new ColumnSpec() { Name = name, SqlType = sqlType, Size = typeSize, IsKey = isKeyColumn });

            if (string.IsNullOrEmpty(columnList))
                this.columnList = name;
            else
                this.columnList += "," + name;

            if (isKeyColumn)
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
                    if (!this.columnSet.Contains(column) && !(this.derivedColumns != null && this.derivedColumns.Contains(column)))
                    {
                        if (_log != null) _log.ErrorFormat("Invalid column {column} referenced in table {table} in {query}", column, this, querySpec);
                        throw new ArgumentException("The column " + column + " does not exists in the table.");
                    }
                }
            }

            // In JQuery DataTables key in column filter should match column names; however, in general that should not be the case.
            if (parametersAreColumnNames && querySpec.columnFilter != null)
            {
                foreach (string column in querySpec.columnFilter.Keys)
                {
                    if (!this.columnSet.Contains(column))
                    {
                        if (_log != null) _log.ErrorFormat("The column {column} from query {query} does not exists in the {table}.", column, querySpec, this);
                        throw new ArgumentException("The column " + column + " does not exists in the table " + this.Name);
                    }
                }
            }
        }

        public void HasColumn(string column)
        {
            if(this.columnSet == null || this.columnSet.Count == 0)
            {
                if (_log != null) _log.ErrorFormat("The column {column} cannot be validated because the columns are not defined in the table {table}.", column, this);
                throw new ArgumentOutOfRangeException($"The column '{column}' cannot be validated because the columns are not defined in the table.");
            }
            if (!this.columnSet.Contains(column))
            {
                if (_log != null) _log.ErrorFormat("The column {column} does not exists in the {table}.", column, this);
                throw new ArgumentOutOfRangeException($"Column '{column}' does not belong to the table.");
            }
        }
    }
}
