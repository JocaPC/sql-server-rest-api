grammar ODataTranslator;

@header {
using MsSql.RestApi;
}
 
@parser::members {

	internal System.Collections.Generic.LinkedList<Aggregate> Aggregates =
        new System.Collections.Generic.LinkedList<Aggregate>();
	
	internal string GroupBy = null;
	internal Dictionary<string, QuerySpec> Relations = new Dictionary<string, QuerySpec>();
	
	internal TableSpec tableSpec;
	internal TableSpec currentTableScopeSpec; // Used to validate columns in the current scope.
	internal QuerySpec querySpec;
	internal ODataTranslatorParser(ITokenStream input,
							TableSpec tableSpec,
							QuerySpec querySpec): this(input) 
	{
		this.tableSpec = tableSpec;
		this.querySpec = querySpec;
		this.currentTableScopeSpec = tableSpec; // we are initially validating root entity scope.
		this.ErrorHandler = new BailErrorStrategy();
	}
}

@lexer::members {
	internal TableSpec tableSpec;
	internal QuerySpec querySpec;
	string odataHelperSqlSchema = "odata";
	int i = 0;
	internal ODataTranslatorLexer(ICharStream input,
							TableSpec tableSpec,
							QuerySpec querySpec,
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

// Order by expression with optional aggregation functions:
// $orderBy=PersonID				->	ORDER BY PersonID
// $orderBy=Price with sum			->	ORDER BY SUM(Price)
// $orderBy=Price with sum desc		->	ORDER BY SUM(Price) DESC
orderBy
	returns [string Expression, string Direction]:
		e=expression
		('with' agg=('sum'|'avg'|'min'|'max'|'count') )? 
		('asc'|('desc'{ $Direction = "desc"; }))?
		{ $Expression = (_localctx.agg == null ? _localctx.e.expr:(_localctx.agg.Text +"("+_localctx.e.expr+")")); }
;

//// Main production for $apply parameter - DO NOT MERGE ')' and ',' INTO '),' BECAUSE OTHER PROD RULES WILL FAIL
// $apply=Price with avg as avgPrice								->	AVG(Price) as avgPrice	
// $apply=Price with avg as avgPrice,Price with sum as totalPrice	->	AVG(Price) as avgPrice,SUM(Price) as totalPrice
// $apply=groupby((State,City),Price with avg as avgPrice)			->	AVG(Price) as avgPrice ... GROUP BY State,City

apply : ('groupby((' columns ')' ',' aggregates ')') 
		| aggregates;

columns: list=ids { this.GroupBy = _localctx.list.ColumnNames; } ;

ids 
	returns [string ColumnNames]: 
		c1=column {$ColumnNames=_localctx.c1.Name;} ( ',' c2=column { $ColumnNames += ","+_localctx.c2.Name; } )*
;

aggregates: aggregate (',' aggregate)*;

aggregate: 'aggregate(' agg_exp = agg_function 'as' alias=IDENT ')'
			{
				this.AddAggregateExpression(_localctx.agg_exp.fun, _localctx.agg_exp.expr, _localctx.alias.Text);
			}
;

agg_function
	returns [string fun, string expr]
	: agg_exp=expression 'with' agg=('sum'|'avg'|'min'|'max'|'count')
			{
				$fun = _localctx.agg.Text; $expr = _localctx.agg_exp.expr;
			};

// Expanded items
// $expand=SalesOrders
// $expand=SalesOrders,Invoices
// $expand=SalesOrders($top=20,$skip=10,$select=OrderNo,OrderDate,$filter=OrderDate gt '2017-08-70',$orderBy=OrderNo)
// $expand=SalesOrders($select=OrderNo,OrderDate,$filter=OrderDate gt '2017-08-70',$orderBy=OrderNo),Invoices($top=20,$skip=10)

expandItems:
	expandItem ( ',' expandItem )*;

expandItem :
	expand=IDENT 
	{
		this.SetValidationScope(_localctx.expand.Text);
	}
	'(' es = expandSpec ')'
	{ 
		this.ResetValidationScope();
		this.AddExpandRelation(_localctx.expand.Text, _localctx.es == null ? null : _localctx.es.props);
	}; 

expandSpec
	returns [Dictionary<string, string> props]:
		{ $props = new Dictionary<string, string>(); }
		e=expandSpecItem { $props.Add(_localctx.e.key, _localctx.e.value); } 
			( ',' e2=expandSpecItem { $props.Add(_localctx.e2.key, _localctx.e2.value); } )*
;

expandSpecItem
	returns [string key, string value]:
		'$select=' columnList=columns { $key = "select"; $value = _localctx.columnList.GetText(); }
		|
		'$filter=' where=logExpression { $key = "filter"; $value = _localctx.where.GetText(); }
		|
		'$top=' top=NUMBER  { $key = "top"; $value = _localctx.top.Text; }
		|
		'$skip=' skip=NUMBER  { $key = "skip"; $value = _localctx.skip.Text; }
		|
		'$orderBy=' obi=orderBy  { $key = "orderBy"; $value = _localctx.obi.Expression + " " + _localctx.obi.Direction; }
;

// Logical expression
// ( <logExpression> )
// <relExpression> LOGOP <relExpression>
// <relExpression> LOGOP <relExpression> LOGOP <relExpression>

logExpression
	returns [string expr]:
	'(' le=logExpression ')' { $expr = _localctx.le.GetText(); } 
	|
	{ System.Text.StringBuilder sb = new System.Text.StringBuilder(); } 
	exp1=relExpression
		{	sb.Append(_localctx.exp1.GetText()); }
		(
			loglop=LOGOP exp2=relExpression
			{
				sb.Append(_localctx.loglop.Text).Append(" ").Append(_localctx.exp2.GetText()); 
			}
		)+
	{ $expr = sb.ToString(); }		
;

// Relational expression
// ( <relExpression> )
// <expression> RELOP <expression>
// <expression> RELOP <expression> RELOP <expression>

relExpression
	returns [string expr]:
	'(' re=relExpression ')' { $expr = _localctx.re.GetText(); } 
	|
	{ System.Text.StringBuilder sb = new System.Text.StringBuilder(); } 
	exp1=expression
		{	sb.Append(_localctx.exp1.GetText()); }
		(
			relop=RELOP exp2=expression
			{
				sb.Append(_localctx.relop.Text).Append(" ").Append(_localctx.exp2.GetText()); 
			}
		)+
	{ $expr = sb.ToString(); } 
;

// Expression
// OPERAND
// ( <expression> )
// <expression> OPERATOR <expression>
// <expression> OPERATOR <expression> OPERATOR <expression>

expression
	returns [string expr]:
		opd=operand { $expr = _localctx.opd.GetText(); }
		|
		'(' e=expression ')' { $expr = _localctx.e.GetText(); } 
		|
		operand1=expression
		op=OPERATOR
		operand2=expression
		{	$expr = _localctx.operand1.GetText() + " " + _localctx.op.Text + " " + _localctx.operand2.GetText();	}
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
NUMBER : [0-9]+ {
		var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.Int);
		p.Value = System.Convert.ToInt32(Text);
		this.querySpec.parameters.AddFirst(p);
		Text = "@p"+(i++); 
};

PAR : [()];