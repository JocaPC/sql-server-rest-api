// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections;
using System.Data.SqlClient;
using System.Text;

namespace SqlServerRestApi.SQL
{
    public static class QueryBuilder
    {
        
        public static SqlCommand Build(QuerySpec spec, TableSpec table)
        {
            SqlCommand res = new SqlCommand();
            StringBuilder sql = new StringBuilder();

            BuildSelectFromClause(spec, table, sql);
            if (!spec.count)
            {
                BuildWherePredicate(spec, res, sql, table);
                BuildOrderByClause(spec, sql);
                BuildOffsetFetchClause(spec, sql);
            }

            res.CommandText = sql.ToString();
            return res;
        }

        private static void BuildSelectFromClause(QuerySpec spec, TableSpec table, StringBuilder sql)
        {
            sql.Append("SELECT ");

            if (spec.count) {
                sql.Append("CAST(count(*) as nvarchar(50)) ");
            }
            else
            {
                if (spec.top != 0 && spec.skip == 0)
                    sql.Append("TOP ").Append(spec.top).Append(" ");

                sql.Append(spec.select ?? table.columnList);
            }
            sql.Append(" FROM ");
            sql.Append(table.FullName);
        }

        /// <summary>
        /// co11 LIKE @kwd OR col2 LIKE @kwd ...
        /// OR
        /// (col1 LIKE @srch1 AND col2 LIKE srch2 ...)
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="res"></param>
        private static void BuildWherePredicate(QuerySpec spec, SqlCommand res, StringBuilder sql, TableSpec table)
        {
            bool isWhereClauseAdded = false;

            // If there is a global search by keyword in JQuery Datatable or $search param in OData add this parameter.
            if ( !string.IsNullOrEmpty(spec.keyword) )
            {
                var p = new SqlParameter("kwd", System.Data.SqlDbType.NVarChar, 4000);
                p.Value = "%" + spec.keyword + "%";
                res.Parameters.Add(p);
            }

            // If there are some literals that are transformed to parameters add them in command.
            if (spec.parameters != null && spec.parameters.Count > 0)
            {
                foreach (var parameter in spec.parameters)
                {
                    res.Parameters.Add(parameter);
                }
            }

            // If there are filters per columns add them as parameters
            if (spec.columnFilter != null && spec.columnFilter.Count > 0)
            {
                foreach (DictionaryEntry entry in spec.columnFilter)
                {
                    if (string.IsNullOrEmpty(entry.Value.ToString()))
                        continue;
                    var p = new SqlParameter(entry.Key.ToString(), System.Data.SqlDbType.NVarChar, 4000);
                    p.Value = "%"+entry.Value+"%";
                    res.Parameters.Add(p);
                }
            }

            if ( !string.IsNullOrEmpty(spec.predicate) )
            {
                // This is T-SQL predicate that is provided via Url (e.g. using OData $filter clause)
                sql.Append(" WHERE (").Append(spec.predicate).Append(")");
                isWhereClauseAdded = true;
            }

            if (!string.IsNullOrEmpty(spec.keyword) )
            {
                if (!isWhereClauseAdded)
                {
                    sql.Append(" WHERE (");
                    isWhereClauseAdded = true;
                }
                else
                {
                    sql.Append(" OR (");
                }

                bool isFirstColumn = true;
                foreach (var column in table.columns)
                {
                    if (!isFirstColumn)
                    {
                        sql.Append(" OR ");
                    }
                    sql.Append("(").Append(column).Append(" like @kwd)");
                    isFirstColumn = false;
                }
                sql.Append(" ) "); // Add closing ) for WHERE ( or OR ( that is added in this block
            }

            // Add filter predicates for individual columns.
            if (spec.columnFilter != null && spec.columnFilter.Count > 0)
            {
                bool isFirstColumn = true, isWhereClauseAddedInColumnFiler = false;
                foreach (DictionaryEntry entry in spec.columnFilter)
                {
                    if (!string.IsNullOrEmpty(entry.Value.ToString())) {

                        if (isFirstColumn)
                        {
                            if (!isWhereClauseAdded)
                            {
                                sql.Append(" WHERE (");
                            }
                            else
                            {
                                sql.Append(" OR (");
                            }
                            isWhereClauseAddedInColumnFiler = true;
                        } else
                        {
                            sql.Append(" AND ");
                        }

                        sql.Append("(").Append(entry.Key.ToString()).Append(" LIKE @").Append(entry.Key.ToString()).Append(")");
                        isFirstColumn = false;
                    }
                }
                if (isWhereClauseAddedInColumnFiler)
                {
                    sql.Append(")");
                }
            }
            
        }

        private static void BuildOrderByClause(QuerySpec spec, StringBuilder sql)
        {
            if (spec.order != null && spec.order.Count > 0)
            {
                bool first = true;
                foreach (DictionaryEntry entry in spec.order)
                {
                    if (first)
                    {
                        sql.Append(" ORDER BY ");
                        first = false;
                    }
                    else
                        sql.Append(" , ");

                    sql.Append(entry.Key.ToString()).Append(" ").Append(entry.Value.ToString());
                }
            }
        }
        
        private static void BuildOffsetFetchClause(QuerySpec spec, StringBuilder sql)
        {
            if (spec.top == 0 && spec.skip == 0)
                return; // Nothing to generate
            if (spec.top != 0 && spec.skip == 0)
                return; // This case is covered with TOP

            // At this point we know that spec.skip is != null

            if (spec.order == null || spec.order.Keys.Count == 0)
                sql.Append(" ORDER BY 1 "); // Add mandatory order by clause if it is not yet there.

            sql.AppendFormat(" OFFSET {0} ROWS ", spec.skip);

            if (spec.top != 0)
                sql.AppendFormat(" FETCH NEXT {0} ROWS ONLY ", spec.top);
            
        }

        public static SqlCommand AsJson(this SqlCommand cmd)
        {
            cmd.CommandText += " FOR JSON PATH";
            return cmd;
        }

        public static SqlCommand AsSingleJson(this SqlCommand cmd)
        {
            cmd.CommandText += " FOR JSON PATH, WITHOUT_ARRAY_WRAPPER";
            return cmd;
        }
    }
}
