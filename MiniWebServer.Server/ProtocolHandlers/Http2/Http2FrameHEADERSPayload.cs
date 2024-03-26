﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2FrameHEADERSPayload
    {
        public byte PadLength { get; set; }
        public bool Exclusive { get; set; }
        public uint StreamDependency { get; set; }
        public byte Weight { get; set; }    
        public object FieldBlockFragment { get; set; }

        // there will be a Padding in a HEADERS frame, but we don't need to process it
    }
}