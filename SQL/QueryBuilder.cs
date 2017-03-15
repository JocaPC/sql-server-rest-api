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
                BuildWherePredicate(spec, res, sql);
                BuildOrderByClause(spec, sql);
                BuildOffsetFetchClause(spec, sql);
            }

            res.CommandText = sql.ToString();
            return res;
        }

        private static void BuildSelectFromClause(QuerySpec spec, TableSpec table, StringBuilder sql)
        {
            sql.Append("select ");

            if (spec.count) {
                sql.Append("cast(count(*) as nvarchar) ");
            }
            else
            {
                if (spec.top != 0 && spec.skip == 0)
                    sql.Append("top ").Append(spec.top).Append(" ");

                sql.Append(spec.select ?? table.columnList);
            }
            sql.Append(" from ");
            sql.Append(table.FullName);
        }

        /// <summary>
        /// co11 LIKE @kwd OR col2 LIKE @kwd ...
        /// OR
        /// (col1 LIKE @srch1 AND col2 LIKE srch2 ...)
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="res"></param>
        private static void BuildWherePredicate(QuerySpec spec, SqlCommand res, StringBuilder sql)
        {
            if (spec.predicate != null)
            {
                sql.Append(" WHERE ").Append(spec.predicate);
                if (spec.columnFilter != null && spec.columnFilter.Count > 0)
                {
                    foreach (DictionaryEntry entry in spec.columnFilter)
                    {
                        res.Parameters.AddWithValue(entry.Key.ToString(), entry.Value);
                    }
                }
            } else
            {
                StringBuilder columnfilter = null;
                if (spec.keyword != "" && spec.columnFilter != null)
                    res.Parameters.AddWithValue("kwd", "%" + spec.keyword + "%");
                else
                    columnfilter = new StringBuilder();

                if (spec.columnFilter != null && spec.columnFilter.Count > 0)
                {
                    bool first = true;
                    bool firstColumnFilter = true;
                    foreach (DictionaryEntry entry in spec.columnFilter)
                    {
                        if (spec.keyword != "")
                        {
                            if (first)
                            {
                                sql.Append(" where ");
                                first = false;
                            }
                            else if (spec.keyword != "")
                                sql.Append(" or "); // if there is a keyword, let's start with or logic.

                            sql.Append(entry.Key.ToString()).Append(" LIKE @kwd");

                            if (!string.IsNullOrWhiteSpace(entry.Value.ToString()))
                            {
                                if (firstColumnFilter)
                                    firstColumnFilter = false;
                                else
                                    sql.Append(" or ");

                                columnfilter.Append(entry.Key.ToString()).Append(" LIKE @").Append(entry.Key.ToString());
                                res.Parameters.AddWithValue(entry.Key.ToString(), "%" + entry.Value.ToString() + "%");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(entry.Value.ToString()))
                            {
                                if (first)
                                {
                                    sql.Append(" where ");
                                    first = false;
                                }
                                else
                                    sql.Append(" and ");
                                sql.Append(entry.Key.ToString()).Append(" LIKE @").Append(entry.Key.ToString());
                                res.Parameters.AddWithValue(entry.Key.ToString(), "%" + entry.Value.ToString() + "%");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(spec.keyword) && columnfilter != null)
                        sql.Append(" or (").Append(columnfilter.ToString()).Append(")");
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
                        sql.Append(" order by ");
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
                sql.Append(" order by 1 "); // Add mandatory order by clause if it is not yet there.

            sql.AppendFormat(" OFFSET {0} ROWS ", spec.skip);

            if (spec.top != 0)
                sql.AppendFormat(" FETCH NEXT {0} ROWS ONLY ", spec.top);
            
        }

        public static SqlCommand AsJson(this SqlCommand cmd)
        {
            cmd.CommandText += " for json path";
            return cmd;
        }

        public static SqlCommand AsSingleJson(this SqlCommand cmd)
        {
            cmd.CommandText += " for json path, without_array_wrapper";
            return cmd;
        }
    }
}
