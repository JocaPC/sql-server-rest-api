// Example:
// ID op DIGIT op ID op (DIGIT op ID) op ID op STRING_LITERAL op (DIGIT op DIGIT )s
lexer grammar FilterTranslator;

@lexer::members {
    SqlServerRestApi.SQL.TableSpec tableSpec;
	SqlServerRestApi.SQL.QuerySpec querySpec;
	int i = 0;
	public FilterTranslator(ICharStream input,
							SqlServerRestApi.SQL.TableSpec tableSpec,
							SqlServerRestApi.SQL.QuerySpec querySpec): base(input) 
	{
		this.tableSpec = tableSpec;
		this.querySpec = querySpec;
		this.querySpec.columnFilter = new System.Collections.Hashtable();
		_interp = new LexerATNSimulator(this,_ATN);
	}
}

//Translated tokens
OPERATOR : 'eq' { Text = "="; } | 'ne' { Text = "<>"; } |
			'gt' { Text = ">"; } | 'ge' { Text = ">="; } |
			'lt' { Text = "<"; } | 'le' { Text = "<="; } |
			'add' { Text = "+"; } | 'sub' { Text = "-"; } |
			'mul' { Text = "*"; } | 'div' { Text = "/"; } |
			'mod' { Text = "%"; };
FUNCTION :	'contains(' { Text = "odata.contains("; } |
			'endswith' { Text = "odata.endswith("; } |
			'indexof' { Text = "odata.indexof("; } |
			'length(' { Text = "len("; } |
			'startswith(' { Text = "odata.startswith("; } |
			'tolower(' { Text = "lower("; } |
			'touper(' { Text = "lower("; } |
			'year(' { Text = "datepart(year,"; } |			
			'month(' { Text = "datepart(month,"; } |
			'day(' { Text = "datepart(day,"; } |
			'hour(' { Text = "datepart(hour,"; } |
			'minute(' { Text = "datepart(minute,"; } |
			'second(' { Text = "datepart(second,"; };

UNSUPPORTEDFUNCTION: '[a-zA-Z][a-zA-Z0-9"."]*(' {throw new System.ArgumentException("Unsupported function: " + Text);};

WS : [ \n\u000D\r\t]+ -> skip;
STRING_LITERAL : ['].*?['] { this.querySpec.columnFilter.Add("@p"+i, Text.Substring(1,Text.Length-2)); Text = "@p"+(i++); };
NUMBER : [1-9][0-9]*;
PROPERTY : [a-zA-Z][a-zA-Z0-9]* { this.tableSpec.HasColumn(Text);};
TEXT : [".""("")"]+;