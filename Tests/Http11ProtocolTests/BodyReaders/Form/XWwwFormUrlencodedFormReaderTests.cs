using Http11ProtocolTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Abstractions.Http.Form;
using MiniWebServer.Server.BodyReaders.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form.Tests
{
    [TestClass()]
    public class XWwwFormUrlencodedFormReaderTests
    {
        [TestMethod()]
        [DataRow("field1=value1&field2=value2", 2)]
        [DataRow("field1=value1&field2=value2&field2=value2&field2=value2&field2=value2&field2=value2&field2=value2&field2=value2&field2=value2", 2)]
        public async Task XWwwFormUrlencodedFormReaderTestAsync(string data, int formCount)
        {
            var pipeReader = PipeUtils.String2Reader(data);

            var formReader = new XWwwFormUrlencodedFormReader(data.Length);
            var form = await formReader.ReadAsync(pipeReader);

            Assert.IsNotNull(form);
            Assert.AreEqual(formCount, form.Count());
        }
}
}