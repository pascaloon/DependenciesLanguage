grammar DependenciesGrammar;

/*
 * Parser Rules
 */

compileUnit
	: lineExpression	
	| EOF
	;

lineExpression
	: expression NEWLINE?
	;

expression
    : projectExpr ARROW projectExpr #DependencyExpression
    ;

projectExpr
	: nameExpr SPLIT pathExpr #ComposedName
	| nameExpr #OnlyProject
	;

pathExpr
	: NAME (SPLIT NAME)* #FullPath
	| WILDCARD #PathCatchAll
	;

nameExpr
	: NAME #FullName
	| WILDCARD #NameCatchAll
	;


/*
 * Lexer Rules
 */

INT: '0' .. '9';
WILDCARD: '*';
SPLIT: '/';

NAME
	: ('a' .. 'z' | 'A' .. 'Z'| '_' | '-' | '.' | WILDCARD | INT)+
	;
//PATH
//	: NAME (SPLIT NAME)*
//	;

ARROW: '-->';

NEWLINE: ('\r'?'\n' | '\r')+;

WS
	:	' ' -> channel(HIDDEN)
	;
