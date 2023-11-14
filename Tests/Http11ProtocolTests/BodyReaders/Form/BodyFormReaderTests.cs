using Http11ProtocolTests;
using MiniWebServer.Abstractions.Http.Form;

namespace MiniWebServer.Server.BodyReaders.Form.Tests
{
    [TestClass()]
    public class BodyFormReaderTests
    {
        [TestMethod()]
        public async Task ReadForm_With_ParametersAsync()
        {
            var pipeReader = PipeUtils.String2Reader("-----------------------------41952539122868\r\nContent-Disposition: form-data; name=\"username\"\r\n\r\nexample\r\n-----------------------------41952539122868\r\nContent-Disposition: form-data; name=\"email\"\r\n\r\nexample@data.com\r\n-----------------------------41952539122868\r\nContent-Disposition: form-data; name=\"files[]\"; filename=\"photo1.jpg\"\r\nContent-Type: image/jpeg\r\n\r\nExampleBinaryData012031203\r\n-----------------------------41952539122868--");
            var boundary = "---------------------------41952539122868";

            MultipartFormDataFormReader formReader = new(boundary, null);
            var form = await formReader.ReadAsync(pipeReader);

            //
            //var formReader = new MultipartReader(pipeReader,);

            Assert.IsNotNull(form);
            //Assert.AreEqual(4, form.Count);

            //Assert.Fail();
        }
    }
}