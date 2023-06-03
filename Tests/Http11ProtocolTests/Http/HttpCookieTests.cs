using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http.Tests
{
    [TestClass()]
    public class HttpCookieTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            var cookie = new HttpCookie("id", "a3fWa",
                domain: "www.sample.com",
                expires: new DateTimeOffset(2023, 12, 31, 23, 15, 30, TimeSpan.Zero),
                secure: true,
                httpOnly: true,
                maxAge: 123456789,
                path: "/p1/p2/p3/p4"
                );
            Assert.AreEqual("id=a3fWa; Domain=www.sample.com; Expires=Sun, 31 Dec 2023 23:15:30 GMT; HttpOnly; Max-Age=123456789; Path=/p1/p2/p3/p4; Secure",
                cookie.ToString());

            cookie = new HttpCookie("id", "52gg3252fw332ww35",
        expires: new DateTimeOffset(2030, 12, 31, 23, 15, 30, TimeSpan.FromHours(3)),
        secure: true
        );
            Assert.AreEqual("id=52gg3252fw332ww35; Expires=Tue, 31 Dec 2030 20:15:30 GMT; Secure",
                cookie.ToString());
        }
    }
}