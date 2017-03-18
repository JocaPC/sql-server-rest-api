//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from FilterTranslator.g4 by ANTLR 4.5.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.3")]
[System.CLSCompliant(false)]
public partial class FilterTranslator : Lexer {
	public const int
		OPERATOR=1, FUNCTION=2, UNSUPPORTEDFUNCTION=3, WS=4, STRING_LITERAL=5, 
		NUMBER=6, PROPERTY=7, TEXT=8;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"OPERATOR", "FUNCTION", "UNSUPPORTEDFUNCTION", "WS", "STRING_LITERAL", 
		"NUMBER", "PROPERTY", "TEXT"
	};


	    SqlServerRestApi.SQL.TableSpec tableSpec;
		SqlServerRestApi.SQL.QuerySpec querySpec;
		int i = 0;
		public FilterTranslator(ICharStream input,
								SqlServerRestApi.SQL.TableSpec tableSpec,
								SqlServerRestApi.SQL.QuerySpec querySpec): base(input) 
		{
			this.tableSpec = tableSpec;
			this.querySpec = querySpec;
			this.querySpec.parameters = new System.Collections.Generic.LinkedList<System.Data.SqlClient.SqlParameter>();
			_interp = new LexerATNSimulator(this,_ATN);
		}


	public FilterTranslator(ICharStream input)
		: base(input)
	{
		_interp = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, "'[_a-zA-Z][_a-zA-Z0-9\".\"]*('"
	};
	private static readonly string[] _SymbolicNames = {
		null, "OPERATOR", "FUNCTION", "UNSUPPORTEDFUNCTION", "WS", "STRING_LITERAL", 
		"NUMBER", "PROPERTY", "TEXT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[System.Obsolete("Use Vocabulary instead.")]
	public static readonly string[] tokenNames = GenerateTokenNames(DefaultVocabulary, _SymbolicNames.Length);

	private static string[] GenerateTokenNames(IVocabulary vocabulary, int length) {
		string[] tokenNames = new string[length];
		for (int i = 0; i < tokenNames.Length; i++) {
			tokenNames[i] = vocabulary.GetLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = vocabulary.GetSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}

		return tokenNames;
	}

	[System.Obsolete]
	public override string[] TokenNames
	{
		get
		{
			return tokenNames;
		}
	}

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "FilterTranslator.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public override void Action(RuleContext _localctx, int ruleIndex, int actionIndex) {
		switch (ruleIndex) {
		case 0 : OPERATOR_action(_localctx, actionIndex); break;

		case 1 : FUNCTION_action(_localctx, actionIndex); break;

		case 2 : UNSUPPORTEDFUNCTION_action(_localctx, actionIndex); break;

		case 4 : STRING_LITERAL_action(_localctx, actionIndex); break;

		case 5 : NUMBER_action(_localctx, actionIndex); break;

		case 6 : PROPERTY_action(_localctx, actionIndex); break;
		}
	}
	private void OPERATOR_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:  Text = "=";  break;

		case 1:  Text = "<>";  break;

		case 2:  Text = ">";  break;

		case 3:  Text = ">=";  break;

		case 4:  Text = "<";  break;

		case 5:  Text = "<=";  break;

		case 6:  Text = "+";  break;

		case 7:  Text = "-";  break;

		case 8:  Text = "*";  break;

		case 9:  Text = "/";  break;

		case 10:  Text = "%";  break;

		case 11:  Text = " AND ";  break;

		case 12:  Text = " OR ";  break;

		case 13:  Text = " NOT ";  break;
		}
	}
	private void FUNCTION_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 14:  Text = "odata.contains(";  break;

		case 15:  Text = "odata.endswith(";  break;

		case 16:  Text = "odata.indexof(";  break;

		case 17:  Text = "len(";  break;

		case 18:  Text = "odata.startswith(";  break;

		case 19:  Text = "lower(";  break;

		case 20:  Text = "lower(";  break;

		case 21:  Text = "datepart(year,";  break;

		case 22:  Text = "datepart(month,";  break;

		case 23:  Text = "datepart(day,";  break;

		case 24:  Text = "datepart(hour,";  break;

		case 25:  Text = "datepart(minute,";  break;

		case 26:  Text = "datepart(second,";  break;

		case 27:  Text = "json_value(,";  break;

		case 28:  Text = "json_query(,";  break;

		case 29:  Text = "isjson(,";  break;
		}
	}
	private void UNSUPPORTEDFUNCTION_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 30: throw new System.ArgumentException("Unsupported function: " + Text); break;
		}
	}
	private void STRING_LITERAL_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 31:  
				var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.NVarChar, 4000);
				p.Value = Text.Substring(1,Text.Length-2);
				this.querySpec.parameters.AddFirst(p);
				Text = "@p"+(i++);
		 break;
		}
	}
	private void NUMBER_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 32: 
				var p = new System.Data.SqlClient.SqlParameter("@p"+i, System.Data.SqlDbType.Int);
				p.Value = System.Convert.ToInt32(Text);
				this.querySpec.parameters.AddFirst(p);
				Text = "@p"+(i++); 
		 break;
		}
	}
	private void PROPERTY_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 33:  this.tableSpec.HasColumn(Text); break;
		}
	}

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x2\n\x135\b\x1\x4"+
		"\x2\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b"+
		"\x4\t\t\t\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2"+
		"\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3"+
		"\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2"+
		"\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3"+
		"\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2"+
		"\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x3\x2\x5\x2S\n\x2\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3"+
		"\x3\x3\x3\x3\x5\x3\xEF\n\x3\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4"+
		"\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3"+
		"\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x4\x3\x5"+
		"\x6\x5\x10F\n\x5\r\x5\xE\x5\x110\x3\x5\x3\x5\x3\x6\x3\x6\a\x6\x117\n\x6"+
		"\f\x6\xE\x6\x11A\v\x6\x3\x6\x3\x6\x3\x6\x3\a\x3\a\a\a\x121\n\a\f\a\xE"+
		"\a\x124\v\a\x3\a\x3\a\x3\b\x3\b\a\b\x12A\n\b\f\b\xE\b\x12D\v\b\x3\b\x3"+
		"\b\x3\t\x6\t\x132\n\t\r\t\xE\t\x133\x3\x118\x2\x2\n\x3\x2\x3\x5\x2\x4"+
		"\a\x2\x5\t\x2\x6\v\x2\a\r\x2\b\xF\x2\t\x11\x2\n\x3\x2\t\x5\x2\v\f\xF\xF"+
		"\"\"\x3\x2))\x3\x2\x33;\x3\x2\x32;\x6\x2%%\x42\\\x61\x61\x63|\a\x2%%\x32"+
		";\x42\\\x61\x61\x63|\x5\x2$$*+\x30\x30\x155\x2\x3\x3\x2\x2\x2\x2\x5\x3"+
		"\x2\x2\x2\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2"+
		"\x2\x2\x2\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x3R\x3\x2\x2\x2\x5\xEE\x3"+
		"\x2\x2\x2\a\xF0\x3\x2\x2\x2\t\x10E\x3\x2\x2\x2\v\x114\x3\x2\x2\x2\r\x11E"+
		"\x3\x2\x2\x2\xF\x127\x3\x2\x2\x2\x11\x131\x3\x2\x2\x2\x13\x14\ag\x2\x2"+
		"\x14\x15\as\x2\x2\x15\x16\x3\x2\x2\x2\x16S\b\x2\x2\x2\x17\x18\ap\x2\x2"+
		"\x18\x19\ag\x2\x2\x19\x1A\x3\x2\x2\x2\x1AS\b\x2\x3\x2\x1B\x1C\ai\x2\x2"+
		"\x1C\x1D\av\x2\x2\x1D\x1E\x3\x2\x2\x2\x1ES\b\x2\x4\x2\x1F \ai\x2\x2 !"+
		"\ag\x2\x2!\"\x3\x2\x2\x2\"S\b\x2\x5\x2#$\an\x2\x2$%\av\x2\x2%&\x3\x2\x2"+
		"\x2&S\b\x2\x6\x2\'(\an\x2\x2()\ag\x2\x2)*\x3\x2\x2\x2*S\b\x2\a\x2+,\a"+
		"\x63\x2\x2,-\a\x66\x2\x2-.\a\x66\x2\x2./\x3\x2\x2\x2/S\b\x2\b\x2\x30\x31"+
		"\au\x2\x2\x31\x32\aw\x2\x2\x32\x33\a\x64\x2\x2\x33\x34\x3\x2\x2\x2\x34"+
		"S\b\x2\t\x2\x35\x36\ao\x2\x2\x36\x37\aw\x2\x2\x37\x38\an\x2\x2\x38\x39"+
		"\x3\x2\x2\x2\x39S\b\x2\n\x2:;\a\x66\x2\x2;<\ak\x2\x2<=\ax\x2\x2=>\x3\x2"+
		"\x2\x2>S\b\x2\v\x2?@\ao\x2\x2@\x41\aq\x2\x2\x41\x42\a\x66\x2\x2\x42\x43"+
		"\x3\x2\x2\x2\x43S\b\x2\f\x2\x44\x45\a\x63\x2\x2\x45\x46\ap\x2\x2\x46G"+
		"\a\x66\x2\x2GH\x3\x2\x2\x2HS\b\x2\r\x2IJ\aq\x2\x2JK\at\x2\x2KL\x3\x2\x2"+
		"\x2LS\b\x2\xE\x2MN\ap\x2\x2NO\aq\x2\x2OP\av\x2\x2PQ\x3\x2\x2\x2QS\b\x2"+
		"\xF\x2R\x13\x3\x2\x2\x2R\x17\x3\x2\x2\x2R\x1B\x3\x2\x2\x2R\x1F\x3\x2\x2"+
		"\x2R#\x3\x2\x2\x2R\'\x3\x2\x2\x2R+\x3\x2\x2\x2R\x30\x3\x2\x2\x2R\x35\x3"+
		"\x2\x2\x2R:\x3\x2\x2\x2R?\x3\x2\x2\x2R\x44\x3\x2\x2\x2RI\x3\x2\x2\x2R"+
		"M\x3\x2\x2\x2S\x4\x3\x2\x2\x2TU\a\x65\x2\x2UV\aq\x2\x2VW\ap\x2\x2WX\a"+
		"v\x2\x2XY\a\x63\x2\x2YZ\ak\x2\x2Z[\ap\x2\x2[\\\au\x2\x2\\]\a*\x2\x2]^"+
		"\x3\x2\x2\x2^\xEF\b\x3\x10\x2_`\ag\x2\x2`\x61\ap\x2\x2\x61\x62\a\x66\x2"+
		"\x2\x62\x63\au\x2\x2\x63\x64\ay\x2\x2\x64\x65\ak\x2\x2\x65\x66\av\x2\x2"+
		"\x66g\aj\x2\x2gh\a*\x2\x2hi\x3\x2\x2\x2i\xEF\b\x3\x11\x2jk\ak\x2\x2kl"+
		"\ap\x2\x2lm\a\x66\x2\x2mn\ag\x2\x2no\az\x2\x2op\aq\x2\x2pq\ah\x2\x2qr"+
		"\a*\x2\x2rs\x3\x2\x2\x2s\xEF\b\x3\x12\x2tu\an\x2\x2uv\ag\x2\x2vw\ap\x2"+
		"\x2wx\ai\x2\x2xy\av\x2\x2yz\aj\x2\x2z{\a*\x2\x2{|\x3\x2\x2\x2|\xEF\b\x3"+
		"\x13\x2}~\au\x2\x2~\x7F\av\x2\x2\x7F\x80\a\x63\x2\x2\x80\x81\at\x2\x2"+
		"\x81\x82\av\x2\x2\x82\x83\au\x2\x2\x83\x84\ay\x2\x2\x84\x85\ak\x2\x2\x85"+
		"\x86\av\x2\x2\x86\x87\aj\x2\x2\x87\x88\a*\x2\x2\x88\x89\x3\x2\x2\x2\x89"+
		"\xEF\b\x3\x14\x2\x8A\x8B\av\x2\x2\x8B\x8C\aq\x2\x2\x8C\x8D\an\x2\x2\x8D"+
		"\x8E\aq\x2\x2\x8E\x8F\ay\x2\x2\x8F\x90\ag\x2\x2\x90\x91\at\x2\x2\x91\x92"+
		"\a*\x2\x2\x92\x93\x3\x2\x2\x2\x93\xEF\b\x3\x15\x2\x94\x95\av\x2\x2\x95"+
		"\x96\aq\x2\x2\x96\x97\aw\x2\x2\x97\x98\ar\x2\x2\x98\x99\ag\x2\x2\x99\x9A"+
		"\at\x2\x2\x9A\x9B\a*\x2\x2\x9B\x9C\x3\x2\x2\x2\x9C\xEF\b\x3\x16\x2\x9D"+
		"\x9E\a{\x2\x2\x9E\x9F\ag\x2\x2\x9F\xA0\a\x63\x2\x2\xA0\xA1\at\x2\x2\xA1"+
		"\xA2\a*\x2\x2\xA2\xA3\x3\x2\x2\x2\xA3\xEF\b\x3\x17\x2\xA4\xA5\ao\x2\x2"+
		"\xA5\xA6\aq\x2\x2\xA6\xA7\ap\x2\x2\xA7\xA8\av\x2\x2\xA8\xA9\aj\x2\x2\xA9"+
		"\xAA\a*\x2\x2\xAA\xAB\x3\x2\x2\x2\xAB\xEF\b\x3\x18\x2\xAC\xAD\a\x66\x2"+
		"\x2\xAD\xAE\a\x63\x2\x2\xAE\xAF\a{\x2\x2\xAF\xB0\a*\x2\x2\xB0\xB1\x3\x2"+
		"\x2\x2\xB1\xEF\b\x3\x19\x2\xB2\xB3\aj\x2\x2\xB3\xB4\aq\x2\x2\xB4\xB5\a"+
		"w\x2\x2\xB5\xB6\at\x2\x2\xB6\xB7\a*\x2\x2\xB7\xB8\x3\x2\x2\x2\xB8\xEF"+
		"\b\x3\x1A\x2\xB9\xBA\ao\x2\x2\xBA\xBB\ak\x2\x2\xBB\xBC\ap\x2\x2\xBC\xBD"+
		"\aw\x2\x2\xBD\xBE\av\x2\x2\xBE\xBF\ag\x2\x2\xBF\xC0\a*\x2\x2\xC0\xC1\x3"+
		"\x2\x2\x2\xC1\xEF\b\x3\x1B\x2\xC2\xC3\au\x2\x2\xC3\xC4\ag\x2\x2\xC4\xC5"+
		"\a\x65\x2\x2\xC5\xC6\aq\x2\x2\xC6\xC7\ap\x2\x2\xC7\xC8\a\x66\x2\x2\xC8"+
		"\xC9\a*\x2\x2\xC9\xCA\x3\x2\x2\x2\xCA\xEF\b\x3\x1C\x2\xCB\xCC\al\x2\x2"+
		"\xCC\xCD\au\x2\x2\xCD\xCE\aq\x2\x2\xCE\xCF\ap\x2\x2\xCF\xD0\a\x61\x2\x2"+
		"\xD0\xD1\ax\x2\x2\xD1\xD2\a\x63\x2\x2\xD2\xD3\an\x2\x2\xD3\xD4\aw\x2\x2"+
		"\xD4\xD5\ag\x2\x2\xD5\xD6\a*\x2\x2\xD6\xD7\x3\x2\x2\x2\xD7\xEF\b\x3\x1D"+
		"\x2\xD8\xD9\al\x2\x2\xD9\xDA\au\x2\x2\xDA\xDB\aq\x2\x2\xDB\xDC\ap\x2\x2"+
		"\xDC\xDD\a\x61\x2\x2\xDD\xDE\as\x2\x2\xDE\xDF\aw\x2\x2\xDF\xE0\ag\x2\x2"+
		"\xE0\xE1\at\x2\x2\xE1\xE2\a{\x2\x2\xE2\xE3\a*\x2\x2\xE3\xE4\x3\x2\x2\x2"+
		"\xE4\xEF\b\x3\x1E\x2\xE5\xE6\ak\x2\x2\xE6\xE7\au\x2\x2\xE7\xE8\al\x2\x2"+
		"\xE8\xE9\au\x2\x2\xE9\xEA\aq\x2\x2\xEA\xEB\ap\x2\x2\xEB\xEC\a*\x2\x2\xEC"+
		"\xED\x3\x2\x2\x2\xED\xEF\b\x3\x1F\x2\xEET\x3\x2\x2\x2\xEE_\x3\x2\x2\x2"+
		"\xEEj\x3\x2\x2\x2\xEEt\x3\x2\x2\x2\xEE}\x3\x2\x2\x2\xEE\x8A\x3\x2\x2\x2"+
		"\xEE\x94\x3\x2\x2\x2\xEE\x9D\x3\x2\x2\x2\xEE\xA4\x3\x2\x2\x2\xEE\xAC\x3"+
		"\x2\x2\x2\xEE\xB2\x3\x2\x2\x2\xEE\xB9\x3\x2\x2\x2\xEE\xC2\x3\x2\x2\x2"+
		"\xEE\xCB\x3\x2\x2\x2\xEE\xD8\x3\x2\x2\x2\xEE\xE5\x3\x2\x2\x2\xEF\x6\x3"+
		"\x2\x2\x2\xF0\xF1\a]\x2\x2\xF1\xF2\a\x61\x2\x2\xF2\xF3\a\x63\x2\x2\xF3"+
		"\xF4\a/\x2\x2\xF4\xF5\a|\x2\x2\xF5\xF6\a\x43\x2\x2\xF6\xF7\a/\x2\x2\xF7"+
		"\xF8\a\\\x2\x2\xF8\xF9\a_\x2\x2\xF9\xFA\a]\x2\x2\xFA\xFB\a\x61\x2\x2\xFB"+
		"\xFC\a\x63\x2\x2\xFC\xFD\a/\x2\x2\xFD\xFE\a|\x2\x2\xFE\xFF\a\x43\x2\x2"+
		"\xFF\x100\a/\x2\x2\x100\x101\a\\\x2\x2\x101\x102\a\x32\x2\x2\x102\x103"+
		"\a/\x2\x2\x103\x104\a;\x2\x2\x104\x105\a$\x2\x2\x105\x106\a\x30\x2\x2"+
		"\x106\x107\a$\x2\x2\x107\x108\a_\x2\x2\x108\x109\a,\x2\x2\x109\x10A\a"+
		"*\x2\x2\x10A\x10B\x3\x2\x2\x2\x10B\x10C\b\x4 \x2\x10C\b\x3\x2\x2\x2\x10D"+
		"\x10F\t\x2\x2\x2\x10E\x10D\x3\x2\x2\x2\x10F\x110\x3\x2\x2\x2\x110\x10E"+
		"\x3\x2\x2\x2\x110\x111\x3\x2\x2\x2\x111\x112\x3\x2\x2\x2\x112\x113\b\x5"+
		"!\x2\x113\n\x3\x2\x2\x2\x114\x118\t\x3\x2\x2\x115\x117\v\x2\x2\x2\x116"+
		"\x115\x3\x2\x2\x2\x117\x11A\x3\x2\x2\x2\x118\x119\x3\x2\x2\x2\x118\x116"+
		"\x3\x2\x2\x2\x119\x11B\x3\x2\x2\x2\x11A\x118\x3\x2\x2\x2\x11B\x11C\t\x3"+
		"\x2\x2\x11C\x11D\b\x6\"\x2\x11D\f\x3\x2\x2\x2\x11E\x122\t\x4\x2\x2\x11F"+
		"\x121\t\x5\x2\x2\x120\x11F\x3\x2\x2\x2\x121\x124\x3\x2\x2\x2\x122\x120"+
		"\x3\x2\x2\x2\x122\x123\x3\x2\x2\x2\x123\x125\x3\x2\x2\x2\x124\x122\x3"+
		"\x2\x2\x2\x125\x126\b\a#\x2\x126\xE\x3\x2\x2\x2\x127\x12B\t\x6\x2\x2\x128"+
		"\x12A\t\a\x2\x2\x129\x128\x3\x2\x2\x2\x12A\x12D\x3\x2\x2\x2\x12B\x129"+
		"\x3\x2\x2\x2\x12B\x12C\x3\x2\x2\x2\x12C\x12E\x3\x2\x2\x2\x12D\x12B\x3"+
		"\x2\x2\x2\x12E\x12F\b\b$\x2\x12F\x10\x3\x2\x2\x2\x130\x132\t\b\x2\x2\x131"+
		"\x130\x3\x2\x2\x2\x132\x133\x3\x2\x2\x2\x133\x131\x3\x2\x2\x2\x133\x134"+
		"\x3\x2\x2\x2\x134\x12\x3\x2\x2\x2\n\x2R\xEE\x110\x118\x122\x12B\x133%"+
		"\x3\x2\x2\x3\x2\x3\x3\x2\x4\x3\x2\x5\x3\x2\x6\x3\x2\a\x3\x2\b\x3\x2\t"+
		"\x3\x2\n\x3\x2\v\x3\x2\f\x3\x2\r\x3\x2\xE\x3\x2\xF\x3\x3\x10\x3\x3\x11"+
		"\x3\x3\x12\x3\x3\x13\x3\x3\x14\x3\x3\x15\x3\x3\x16\x3\x3\x17\x3\x3\x18"+
		"\x3\x3\x19\x3\x3\x1A\x3\x3\x1B\x3\x3\x1C\x3\x3\x1D\x3\x3\x1E\x3\x3\x1F"+
		"\x3\x4 \b\x2\x2\x3\x6!\x3\a\"\x3\b#";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
