/* Examples:
aggregate(Amount with sum as Total)								=>	sum(Amount) as Total
groupby((Country,Name),	aggregate(Amount with sum as Total))	=>	sum(Amount) as Total group by Country,Name
WideWorldImporters database examples:
aggregate(PersonID with min as Minimum)
groupby((PhoneNumber),   aggregate(PersonID with sum as Total),	aggregate(PersonID with min as Minimum))
groupby((PhoneNumber,FaxNumber),   aggregate(PersonID with sum as Total),	aggregate(PersonID with min as Minimum))
groupby((FullName),		aggregate(PersonID with sum as Total))
*/
grammar ApplyTranslator;

@parser::members {

	public System.Collections.Generic.LinkedList<RestApi.OData.Aggregate> Aggregates =
        new System.Collections.Generic.LinkedList<RestApi.OData.Aggregate>();
	private void AddAggregateExpression(string Method, string Column, string Alias){
    var agg = new RestApi.OData.Aggregate() { AggregateMethod = Method, AggregateColumn = Column, AggregateColumnAlias = Alias };
		this.Aggregates.AddLast(agg);
	}
	public string GroupBy = null;
	public SqlServerRestApi.TableSpec tableSpec;
	public ApplyTranslatorParser(ITokenStream input,
							SqlServerRestApi.TableSpec tableSpec): this(input) 
	{
		this.tableSpec = tableSpec;
	}
}


@lexer::members {
	SqlServerRestApi.TableSpec tableSpec;
	SqlServerRestApi.QuerySpec querySpec;
	string odataHelperSqlSchema = "odata";
	int i = 0;
	public ApplyTranslatorLexer(ICharStream input,
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

orderby
	returns [string orderbyclause]:
		e=expression w=with_clause
		{ $orderbyclause = _localctx.w.fun==null?_localctx.e.expr:(_localctx.w.fun+"("+_localctx.e.expr+")"); };

with_clause
	returns [string fun]:
		('with' agg=('sum'|'avg'|'min'|'max'|'count')
		 { $fun = _localctx.agg.Text; } )
		 |
		 { $fun = null; }
		 ;

//// Main production for $apply parameter
apply : ('groupby(' columns ',' aggregates ')') 
		| aggregates;

columns: '(' list=ids { this.GroupBy = _localctx.list.ColumnNames; } ')' ;
ids 
returns [string ColumnNames]: 
	c1=column {$ColumnNames=_localctx.c1.Name;} ( ',' c2=column { $ColumnNames += ","+_localctx.c2.Name; } )* ;

aggregates: aggregate (',' aggregate)*;

aggregate: 'aggregate(' agg_exp = agg_function 'as' alias=IDENT ')'
			{
				this.AddAggregateExpression(_localctx.agg_exp.fun, _localctx.agg_exp.expr, _localctx.alias.Text);
			};

agg_function
	returns [string fun, string expr]
	: agg_exp=expression 'with' agg=('sum'|'avg'|'min'|'max'|'count')
			{
				$fun = _localctx.agg.Text; $expr = _localctx.agg_exp.expr;
			};
			
expression
	returns [string expr]
	:
		{ System.Text.StringBuilder sb = new System.Text.StringBuilder(); } 
		(
			(	operand1 = (LITERAL | STRING_LITERAL | DATETIME_LITERAL | NUMBER | PAR)
				{ sb.Append(_localctx.operand1.Text).Append(" "); }	)
			|
			(	f=FUNCTION e=expression ')'
				{ sb.Append(_localctx.f.Text).Append(" ").Append(_localctx.e.expr).Append(")"); }
			)
			|
			( c1=column { sb.Append(_localctx.c1.Name).Append(" "); } )
		)
		(
			op=OPERATOR
			{
				sb.Append(_localctx.op.Text).Append(" "); 
			}
			(
				(operand2=(LITERAL | STRING_LITERAL | DATETIME_LITERAL | NUMBER | PAR)	
				{
					sb.Append(_localctx.operand2.Text).Append(" "); 
				}
				)
				|
				(	f=FUNCTION e=expression ')'
					{ sb.Append(_localctx.f.Text).Append(" ").Append(_localctx.e.expr).Append(")"); }
				)
				|
				(c2=column
				{
					sb.Append(_localctx.c2.Name).Append(" ");
				})
			)
		)*
			{
				$expr = sb.ToString();
			}
		;
column 
	returns [string Name]: c=IDENT { this.tableSpec.HasColumn(_localctx.c.Text); $Name = _localctx.c.Text;};




//Translated tokens
OPERATOR :  'add' { Text = "+"; } | 'sub' { Text = "-"; } |
			'mul' { Text = "*"; } | 'div' { Text = "/"; } |
			'mod' { Text = "%"; };

IDENT : [_@#a-zA-Z][a-zA-Z0-9_@#]*;

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
//PROPERTY : [a-zA-Z][a-zA-Z0-9]* { this.tableSpec.HasColumn(Text);};
PAR : [()];