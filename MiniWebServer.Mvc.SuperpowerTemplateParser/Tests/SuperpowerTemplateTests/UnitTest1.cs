using MiniWebServer.Mvc.SuperpowerTemplateParser;

namespace SuperpowerTemplateTests
{
    [TestClass]
    public class UnitTest1
    {
        //      [TestMethod]
        //      public void Simple1()
        //      {
        //          var razorContent = LoadRazorFileContent("simple-1.cshtml");

        //          var tokenizer = new MiniRazorTokenizer();
        //          var tokens = tokenizer.TokenizePublic(new Superpower.Model.TextSpan(razorContent));

        //          Assert.IsNotNull(tokens);

        //	foreach (var token in tokens)
        //	{
        //		Console.WriteLine(token);
        //	}
        //}

        [TestMethod]
        public void CodeBlock1()
        {
            var razorContent = LoadRazorFileContent("code-block-1.cshtml");

            var tokenizer = new MiniRazorTokenizer();
            var tokens = tokenizer.TokenizePublic(new Superpower.Model.TextSpan(razorContent));

            Assert.IsNotNull(tokens);

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }


        private static string LoadRazorFileContent(string filePath)
        {
            return File.ReadAllText(Path.Combine("RazorFiles", filePath));
        }
    }
}