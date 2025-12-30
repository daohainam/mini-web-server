using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Server.ProtocolHandlers.Http2;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2.Tests;

[TestClass()]
public class HPACKIntegerHeaderTests
{
    [TestMethod()]
    public void ReadIntTest()
    {
        var bytes = new byte[] { 0b_0001_1111, 0b_10011010, 0b_00001010 };
        var byteSequence = new ReadOnlySequence<byte>(bytes);
        var r = HPACKInteger.ReadInt(ref byteSequence, 5);

        Assert.AreEqual(1337, r);
    }
}
