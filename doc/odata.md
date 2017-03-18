# Sql Server REST API - OData Service

Sql Server REST API library enables you to easily create OData services. In this page you can find out how to create
OData service and what features from OData standard are supported.

# Supported query parameters

Sql Server REST API library enables you to create OData REST services that support the following operations:
 - $select that enables the caller to choose what fields should be returned in response,
 - $filter that can filter entities using entity properties, and expressions with:
   - Arithmetical operators 'add', 'sub', 'mul', 'div' , 'mod'
   - Relational operators 'eq', 'ne', 'gt', 'ge', 'lt', 'le'
   - Logical operators 'and', 'or', 'not', 	
   - Build-in functions: 'length', 'tolower', 'toupper', 'year', 'month', 'day', 'hour', 'minute', 'second', 'json_value', 'json_query', and 'isjson'
 - $orderby that can sort entities by some column(s)
 - $top and $skip that can be used for pagination,
 - $count that enables you to get the total number of entities,
 - $search that search for entities by a keyword

OData services implemented using Sql Server REST API library provide minimal interface that web clients can use to
query data without additional overhead introduced by advanced OData operators (e.g. $extend, all/any), or verbose response format.

> The goal of this project is not to support all standard OData features. Library provides the most important features, and
> excludes features that cannot provide . The most important benefits that this library provides are simplicity and speed. 
> If you need full compatibility with official OData spec, you can chose other implementations.

# Metadata information

OData services implemented using Sql Server REST API library return minimal response format that is compliant to the
OData spec (aka. metadata-none format).
In this library is supported only minimal output format that do not include any metadata information in the REST API body.

