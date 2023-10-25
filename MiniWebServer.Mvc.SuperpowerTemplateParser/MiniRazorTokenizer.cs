using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.SuperpowerTemplateParser
{
	public class MiniRazorTokenizer: Tokenizer<MiniRazorToken>
	{
		private const string UnexpectedEndOfInput = "Unexpected end of input";

		protected override IEnumerable<Result<MiniRazorToken>> Tokenize(TextSpan input)
		{
			var next = SkipWhiteSpace(input);
			if (!next.HasValue)
				yield break;

			do
			{
				if (next.Value == '@')
				{
					var atSignStart = next.Location;
					next = next.Remainder.ConsumeChar();
					if (!next.HasValue)
					{
						yield return Result.Empty<MiniRazorToken>(atSignStart, UnexpectedEndOfInput);
						yield break;
					}

					if (next.Value == '@')
					{
						yield return Result.Value(MiniRazorToken.AtSign, atSignStart, next.Remainder);

						next = next.Remainder.ConsumeChar();
					}
					else if (next.Value == '(')
					{
						yield return Result.Value(MiniRazorToken.ExpressionBlockStart, atSignStart, next.Remainder);

						next = next.Remainder.ConsumeChar();
					}
					else if (next.Value == '{')
					{
						yield return Result.Value(MiniRazorToken.CodeBlockStart, atSignStart, next.Remainder);

						next = next.Remainder.ConsumeChar();
						var codeBlock = CodeBlock(next.Location);

						yield return Result.Value(MiniRazorToken.CodeBlockContent, codeBlock.Location, next.Remainder);

						next = next.Remainder.ConsumeChar();
						if (next.Value == '}')
						{
							yield return Result.Value(MiniRazorToken.CodeBlockStart, atSignStart, next.Remainder);
						}
						else
						{
							yield return Result.Empty<MiniRazorToken>(atSignStart, "missing '}'");
							yield break;
						}

						next = next.Remainder.ConsumeChar();
					}
					else if (next.Value == '*')
					{
						var comment = ParserHelpers.Comment(next.Remainder);
						yield return Result.Value(MiniRazorToken.CommentBlockStart, atSignStart, comment.Remainder);

						next = next.Remainder.ConsumeChar();
					}
					else if (char.IsLetter(next.Value))
					{
						var identifier = Identifier.CStyle(next.Location);

						if (identifier.HasValue)
						{
							if (identifier.Value.EqualsValue("model"))
							{
								yield return Result.Value(MiniRazorToken.ModelKeyword, next.Location, identifier.Remainder);

								next = identifier.Remainder.ConsumeChar();
							}
							else if (identifier.Value.EqualsValue("inject"))
							{
								yield return Result.Value(MiniRazorToken.InjectKeyword, next.Location, identifier.Remainder);

								next = identifier.Remainder.ConsumeChar();
							}
							else if (identifier.Value.EqualsValue("using"))
							{
								yield return Result.Value(MiniRazorToken.UsingKeyword, next.Location, identifier.Remainder);

								next = identifier.Remainder.ConsumeChar();
							}
							else
							{
								yield return Result.Empty<MiniRazorToken>(atSignStart, $"invalid keyword `@{identifier.Value}`");
								yield break;
							}
						}
						else
						{
							next = next.Remainder.ConsumeChar();
						}
					}
				}
				else if (char.IsLetter(next.Value))
				{
					var keywordStart = next.Location;
					do
					{
						next = next.Remainder.ConsumeChar();
					} while (next.HasValue && char.IsLetter(next.Value));

					yield return Result.Value(MiniRazorToken.CommentBlock, keywordStart, next.Location);
				}
				else
				{
					yield return Result.Empty<MiniRazorToken>(next.Location, $"unrecognized `{next.Value}`");
					next = next.Remainder.ConsumeChar(); // Skip the character anyway
				}

				next = SkipWhiteSpace(next.Location);
			} while (next.HasValue);
		}

		public static TextParser<TextSpan> CodeBlock { get; } = input =>
		{
			var next = input.ConsumeChar();
			var stack = new Stack<char>();

			if (!next.HasValue)
				return Result.Empty<TextSpan>(input, UnexpectedEndOfInput);

			TextSpan remainder;
			do
			{
				remainder = next.Remainder;
				next = remainder.ConsumeChar();

				if (next.HasValue && next.Value == '{')
				{
					stack.Push('{');
				}
				else if (next.HasValue && next.Value == '}')
				{
					if (stack.Count > 0)
						stack.Pop();
					else
					{
						next = remainder.ConsumeChar();
					}
				}
			} while (next.HasValue && next.Value != '}');

			return Result.Value(input.Until(remainder), input, remainder);
		};
		public static TextParser<TextSpan> CommentBlock { get; } = input =>
		{
			var next = input.ConsumeChar();

			if (!next.HasValue)
				return Result.Empty<TextSpan>(input, UnexpectedEndOfInput);

			var commentBlockStart = next.Location;

			TextSpan remainder;
			do
			{
				remainder = next.Remainder;

				if (next.Value == '*')
				{
					var starSignStart = next.Location;
					var afterNext = next.Remainder.ConsumeChar();

					if (afterNext.HasValue && afterNext.Value == '@')
					{
						return Result.Value(input.Until(remainder), input, afterNext.Remainder);
					}
				}

				next = remainder.ConsumeChar();

			} while (next.HasValue && next.Value == '}');

			return Result.Value(input.Until(remainder), input, remainder);
		};

		public IEnumerable<Result<MiniRazorToken>> TokenizePublic(TextSpan span) // for testing
		{
			return this.Tokenize(span);
		}
	}
}
