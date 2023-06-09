﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniApp: ICallableService, ICallable
    {
        void Map(string route, ICallable action, params Abstractions.Http.HttpMethod[] methods);
    }
}
