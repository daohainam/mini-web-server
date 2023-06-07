using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session
{
    public interface ISessionStore
    {
        Task<ISession> FindOrCreateAsync(string sessionId, CancellationToken cancellationToken);
    }
}
