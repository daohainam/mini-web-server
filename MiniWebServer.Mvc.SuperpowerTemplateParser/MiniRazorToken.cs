using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.SuperpowerTemplateParser
{
	public enum MiniRazorToken
	{
		None = 0,
		TextBlock,
		AtSign, // @@
		ExpressionBlockStart, // @(
		ExpressionBlockContent,
		ExpressionBlockEnd,
		CodeBlockStart, // @{
		CodeBlockContent,
		CodeBlockEnd,
		CommentBlockStart, // @*
		CommentBlock,
		CommentBlockEnd,
		UsingKeyword, // @using
		InjectKeyword, // @inject
		ModelKeyword, // @model
		KeywordParameter, // anything following a Keyword, and only on the same line, separated by spaces
	}
}
