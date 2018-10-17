grammar ODataTranslator;

@parser::members {

	public System.Collections.Generic.LinkedList<RestApi.OData.Aggregate> Aggregates =
        new System.Collections.Generic.LinkedList<RestApi.OData.Aggregate>();
	
	public string GroupBy = null;
	public Dictionary<string, QuerySpec> Relations = new Dictionary<string, QuerySpec>();
	

	internal SqlServerRestApi.TableSpec tableSpec;
	internal SqlServerRestApi.TableSpec currentTableScopeSpec; // Used to validate columns in the current scope.
	internal SqlServerRestApi.QuerySpec querySpec;
	public ODataTranslatorParser(ITokenStream input,
							SqlServerRestApi.TableSpec tableSpec,
							SqlServerRestApi.QuerySpec querySpec): this(input) 
	{
		this.tableSpec = tableSpec;
		this.querySpec = querySpec;
		this.currentTableScopeSpec = tableSpec; // we are initially validating root entity scope.
	}
}


@lexer::members {
	internal SqlServerRestApi.TableSpec tableSpec;
	internal SqlServerRestApi.QuerySpec querySpec;
	string odataHelperSqlSchema = "odata";
	int i = 0;
	public ODataTranslatorLexer(ICharStream input,
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

orderByItem
	returns [string orderByExpression, string orderByDirection]:
		{ $orderByDirection = "asc"; /* default */ }
		orderByExpr=orderByItemExpr { $orderByExpression = _localctx.orderByExpr.value; }
			('asc'|('desc'{ $orderByDirection = "desc"; }))?
		;

// Order by expression with optional aggergaiton functions, without trailing asc/desc, such as "PersonID" or "Price with sum"
orderByItemExpr
	returns [string value]:
		e=expression w=with_clause
		{ $value = _localctx.w.fun==null?_localctx.e.expr:(_localctx.w.fun+"("+_localctx.e.expr+")"); };

with_clause
	returns [string fun]:
		('with' agg=('sum'|'avg'|'min'|'max'|'count')
		 { $fun = _localctx.agg.Text; } )
		 |
		 { $fun = null; }
		 ;

//// Main production for $apply parameter
apply : ('groupby((' columns '),' aggregates ')') 
		| aggregates;

columns: list=ids { this.GroupBy = _localctx.list.ColumnNames; } ;

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
			
expandItems 
	returns [Dictionary<string, QuerySpec> relations]:
	expandItem (',' expandItem)*
			{
				$relations = this.Relations;
			};

expandItem :
	expand=IDENT 
	{
		this.SetValidationScope(_localctx.expand.Text);
	}
			( '(' es = expandSpec ')' )?
	{ 
		this.ResetValidationScope();
		this.AddExpandRelation(_localctx.expand.Text, _localctx.es == null ? null : _localctx.es.props);
	}; 

expandSpec
	returns [Dictionary<string, string> props]:
		{ $props = new Dictionary<string, string>(); }
		e=expandSpecItem { $props.Add(_localctx.e.key, _localctx.e.value); } 
			( ',' e2=expandSpecItem { $props.Add(_localctx.e2.key, _localctx.e2.value); } )*;

expandSpecItem
	returns [string key, string value]:
		'$select=' columnList=columns { $key = "select"; $value = _localctx.columnList.GetText(); }
		|
		'$filter=' where=expression { $key = "filter"; $value = _localctx.where.GetText(); }
		|
		'$top=' top=NUMBER  { $key = "top"; $value = _localctx.top.Text; }
		|
		'$skip=' skip=NUMBER  { $key = "skip"; $value = _localctx.skip.Text; }
		|
		'$orderBy=' obi=orderByItem  { $key = "orderBy"; $value = _localctx.obi.orderByExpression + " " + _localctx.obi.orderByDirection; }
;

expression
	returns [string expr]
	:
		{ System.Text.StringBuilder sb = new System.Text.StringBuilder(); } 
		(
			operand1=operand
			{
				sb.Append(_localctx.operand1.GetText()); 
			}
		)
		(
			op=OPERATOR
			operand2 = operand
			{
				sb.Append(_localctx.op.Text).Append(" ").Append(_localctx.operand2.GetText()); 
			}
		)*
			{
				$expr = sb.ToString();
			}
		;

operand
	returns [string expr]:
	(	operand1 = (LITERAL | STRING_LITERAL | DATETIME_LITERAL | NUMBER | PAR)
				{ $expr = _localctx.operand1.Text + " "; }	)
			|
			(	f=FUNCTION e=expression ')'
				{ $expr = _localctx.f.Text + " " + _localctx.e.expr + ")"; }
			)
			|
			( c1=column { $expr = _localctx.c1.Name + " "; } )
;

column 
	returns [string Name]: c=IDENT { this.ValidateColumn(_localctx.c.Text); $Name = _localctx.c.Text;};




//Translated tokens
OPERATOR :  'add' { Text = "+"; } | 'sub' { Text = "-"; } |
			'mul' { Text = "*"; } | 'div' { Text = "/"; } |
			'mod' { Text = "%"; };

RELOP : 'eq' { Text = "="; } | 'ne' { Text = "<>"; } |
			'gt' { Text = ">"; } | 'ge' { Text = ">="; } |
			'lt' { Text = "<"; } | 'le' { Text = "<="; } | 
			'is' { Text = "IS"; };

// Operators that are not translated but they need to skip identifier check.
LOGOP:		'and' { Text = " AND "; } | 'or' { Text = " OR "; } |
			'not' { Text = " NOT "; };

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
//PROPERTY : [a-zA-Z][a-zA-Z0-9]* { this.ValidateColumn(Text);};
PAR : [()];