using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Session
{
    internal class DefaultSession : ISession
    {
        private string sessionId = Guid.Empty.ToString();

        public string Id => sessionId;

        public bool IsAvaiable => false;

        public byte[]? Get(string key)
        {
            return null;
        }

        public void Remove(string key)
        {
        }

        public byte[] Set(string key, byte[] value)
        {
            return value;
        }

        public static readonly DefaultSession Instance = new();
    }
}
