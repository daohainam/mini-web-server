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
        private DefaultSession() { } // no one else can create 

        private readonly string sessionId = Guid.Empty.ToString();

        public string Id => sessionId;

        public bool IsAvaiable => false;

        public byte[]? Get(string key)
        {
            return null;
        }

        public byte[] Set(string key, byte[] value)
        {
            return value;
        }

        public bool Clear()
        {
            return true;
        }

        bool ISession.Remove(string key)
        {
            return true;
        }

        Task<bool> ISession.LoadAsync()
        {
            return Task.FromResult(true);
        }

        Task<bool> ISession.SaveAsync()
        {
            return Task.FromResult(true);
        }

        public string? GetString(string key)
        {
            return null;
        }

        public string? SetString(string key, string value)
        {
            return value;
        }

        public static readonly DefaultSession Instance = new();
    }
}
