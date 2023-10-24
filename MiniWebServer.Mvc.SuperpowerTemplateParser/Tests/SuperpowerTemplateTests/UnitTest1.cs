namespace SuperpowerTemplateTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        private static string LoadRazorFileContent(string filePath)
        {
            return File.ReadAllText(Path.Combine("RazorFiles", filePath));
        }
    }
}