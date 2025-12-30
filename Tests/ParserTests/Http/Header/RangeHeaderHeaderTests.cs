using MiniWebServer.Server.Http.Parsers;

namespace MiniWebServer.Abstractions.Http.Header.Tests;

[TestClass()]
public class RangeHeaderHeaderTests
{
    [TestMethod()]
    [DataRow("bytes=0-1023", true, RangeUnits.Bytes, 0, 1023)]
    [DataRow("bytes=1023-0", false, RangeUnits.Bytes, 1023, 0)]
    [DataRow("bytes=1023-", true, RangeUnits.Bytes, 1023, long.MaxValue)]
    public void Header_Range_TryParseTest_1Part(string input, bool result, RangeUnits rangeUnit, long start, long end)
    {
        var b = RangeHeaderParser.TryParse(input, out RangeHeader? header);

        Assert.AreEqual(result, b);

        if (result)
        {
            Assert.IsNotNull(header);
            Assert.AreEqual(rangeUnit, header.Unit);
            Assert.AreEqual(start, header.Parts[0].Start);
            Assert.AreEqual(end, header.Parts[0].End);
        }
    }

    [TestMethod()]
    [DataRow("bytes=0-1023, 10000-20000", true, RangeUnits.Bytes, 0, 1023, 10000, 20000)]
    [DataRow("bytes=0-1023, 10000-", true, RangeUnits.Bytes, 0, 1023, 10000, long.MaxValue)]
    public void Header_Range_TryParseTest_2Parts(string input, bool result, RangeUnits rangeUnit, long start1, long end1, long start2, long end2)
    {
        var b = RangeHeaderParser.TryParse(input, out RangeHeader? header);

        Assert.AreEqual(result, b);

        Assert.IsNotNull(header);

        Assert.AreEqual(rangeUnit, header.Unit);
        Assert.AreEqual(start1, header.Parts[0].Start);
        Assert.AreEqual(end1, header.Parts[0].End);

        Assert.AreEqual(start2, header.Parts[1].Start);
        Assert.AreEqual(end2, header.Parts[1].End);

    }
}
