// Example:
// ID op DIGIT op ID op (DIGIT op ID) op ID op STRING_LITERAL op (DIGIT op DIGIT )s
lexer grammar ExpressionTranslator;

@lexer::members {
    SqlServerRestApi.TableSpec tableSpec;
	SqlServerRestApi.QuerySpec querySpec;
	string odataHelperSqlSchema = "odata";
	int i = 0;
}

//Translated tokens
OPERATOR :  'add' { Text = "+"; } | 'sub' { Text = "-"; } |
			'mul' { Text = "*"; } | 'div' { Text = "/"; } |
			'mod' { Text = "%"; };

LITERAL : 'true' { Text = "1"; } | 'false' { Text = "0"; } | 'null' { Text = "NULL"; };

FUNCTION :	'contains(' { Text = this.odataHelperSqlSchema+".contains("; } |
			'endswith(' { Text = this.odataHelperSqlSchema+".endswith("; } |
			'indexof(' { Text = this.odataHelperSqlSchema+".indexof("; } |
			'substringof(' { Text = this.odataHelperSqlSchema+".substringof("; } |
			'length(' { Text = "len("; } |
			'startswith(' { Text = this.odataHelperSqlSchema+".startswith("; } |
			'tolower(' { Text = "lower("; } |
			'touper(' { Text = "lower("; } |
			'trim(' { Text = "TRIM( CHAR(20) FROM "; } |
			'year(' { Text = "datepart(year,"; } |			
			'month(' { Text = "datepart(month,"; } |
			'day(' { Text = "datepart(day,"; } |
			'hour(' { Text = "datepart(hour,"; } |
			'minute(' { Text = "datepart(minute,"; } |
			'second(' { Text = "datepart(second,"; }
// Non standard functions
			| 'json_value(' { Text = "json_value("; }
			| 'json_query(' { Text = "json_query("; }
			| 'json_modify(' { Text = "json_modify("; }
			| 'isjson(' { Text = "isjson("; }
			| 'json_cast(' { Text = "json_query("; }
			;

UNSUPPORTEDFUNCTION: '[_a-zA-Z][_a-zA-Z0-9"."]*(' {throw new System.ArgumentException("Unsupported function: " + Text);};
DATETIME_LITERAL: 'datetime'STRING_LITERAL { 

		var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.DateTimeOffset);
		p.Value = System.DateTime.Parse(Text.Substring(9,Text.Length-10));
		this.querySpec.parameters.AddFirst(p);
		Text = "@p"+(i++);

};

WS : [ \n\u000D\t]+ -> skip;
STRING_LITERAL : ['].*?['] { 
		var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.NVarChar, 4000);
		p.Value = Text.Substring(1,Text.Length-2);
		this.querySpec.parameters.AddFirst(p);
		Text = "@p"+(i++);
};
NUMBER : [1-9][0-9]* {
		var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.Int);
		p.Value = System.Convert.ToInt32(Text);
		this.querySpec.parameters.AddFirst(p);
		Text = "@p"+(i++); 
};
PROPERTY : [_@#a-zA-Z][a-zA-Z0-9_@#]* { this.tableSpec.HasColumn(Text);};
PAR : [()];