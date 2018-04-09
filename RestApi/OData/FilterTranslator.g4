// Example:
// ID op DIGIT op ID op (DIGIT op ID) op ID op STRING_LITERAL op (DIGIT op DIGIT )s
lexer grammar FilterTranslator;
import ExpressionTranslator;

@lexer::members {

	public FilterTranslator(ICharStream input,
							SqlServerRestApi.TableSpec tableSpec,
							SqlServerRestApi.QuerySpec querySpec,
							string odataHelperSqlSchema = "odata"): this(input) 
	{
		this.tableSpec = tableSpec;
		this.querySpec = querySpec;
		this.odataHelperSqlSchema = odataHelperSqlSchema;
		if(this.querySpec.parameters == null)
		{
			this.querySpec.parameters = new System.Collections.Generic.LinkedList<System.Data.SqlClient.SqlParameter>();
			i = 0;
		} else {
			i = this.querySpec.parameters.Count;
		}
	}
}

RELOP : 'eq' { Text = "="; } | 'ne' { Text = "<>"; } |
			'gt' { Text = ">"; } | 'ge' { Text = ">="; } |
			'lt' { Text = "<"; } | 'le' { Text = "<="; } | 
			'is' { Text = "IS"; };

// Operators that are not translated but they need to skip identifier check.
LOGOP:		'and' { Text = " AND "; } | 'or' { Text = " OR "; } |
			'not' { Text = " NOT "; };