using System.Collections;

namespace SqlServerRestApi.SQL
{
    public class QuerySpec
    {
        public string select = "*";
        public int top;
        public int skip;
        public Hashtable columnFilter;
        public string predicate;
        public Hashtable order;
        internal string keyword;
    }
}
