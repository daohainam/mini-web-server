using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace MiniWebServer.Mvc.SuperpowerTemplateParser;

public static class ParserHelpers
{
    public static TextParser<TextSpan> Comment
    {
        get
        {
            var beginComment = Span.EqualTo("@*");
            var endComment = Span.EqualTo("*@");
            return i =>
            {
                var begin = beginComment(i);
                if (!begin.HasValue)
                    return begin;

                var content = begin.Remainder;
                while (!content.IsAtEnd)
                {
                    var end = endComment(content);
                    if (end.HasValue)
                        return Result.Value(i.Until(end.Remainder), i, end.Remainder);

                    content = content.ConsumeChar().Remainder;
                }

                return endComment(content); // Will fail, because we're at the end-of-input.
            };

        }
    }
}
