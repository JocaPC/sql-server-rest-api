/* Examples:
aggregate(Amount with sum as Total)								=>	sum(Amount) as Total
groupby((Country,Name),	aggregate(Amount with sum as Total))	=>	sum(Amount) as Total group by Country,Name
WideWorldImporters database examples:
aggregate(PersonID with min as Minimum)
groupBy((PhoneNumber),   aggregate(PersonID with sum as Total),	aggregate(PersonID with min as Minimum))
groupBy((PhoneNumber,FaxNumber),   aggregate(PersonID with sum as Total),	aggregate(PersonID with min as Minimum))
groupBy((FullName),		aggregate(PersonID with sum as Total))
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
	
}

apply : 'groupBy' '(' columns ',' aggregates ')' 
		| aggregates;

columns: '(' list=ids { this.GroupBy = _localctx.list.GetText(); } ')' ;
ids: ID (',' ID )* ;

aggregates: aggregate (',' aggregate)*;

aggregate: 'aggregate(' column=ID 'with' agg=OP 'as' alias=ID ')'
	{
		this.AddAggregateExpression(_localctx.agg.Text, _localctx.column.Text, _localctx.alias.Text);	
	};

OP: 'sum'|'avg'|'min'|'max'|'count';
ID : [_@#a-zA-Z][a-zA-Z0-9_@#]*;

WS : [ \n\r\t]+ -> skip;


