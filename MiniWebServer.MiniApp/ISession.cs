using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface ISession
    {
        string Id { get; }
        // session is not always avaiable, we need to initialize it when server starts, in some scenarios such as IoT, API servers, we don't need session
        bool IsAvaiable { get; }
        // session is type-independent, so we support only byte array data type
        byte[]? Get(string key);
        byte[] Set(string key, byte[] value);
        void Remove(string key);
    }
}
