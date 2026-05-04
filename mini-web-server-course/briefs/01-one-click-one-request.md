## Module 1: One Click, One Request

### Teaching Arc
- **Metaphor:** A tracked parcel moving through a delivery network: one package enters, several stations touch it, and a finished parcel comes back.
- **Opening hook:** Imagine you visit `/string-api/toupper?text=hello` and see the server answer with uppercased text — this module follows that exact trip.
- **Key insight:** A web server is not “one magic function”; it is a chain of handoffs that turns raw network bytes into a real response.
- **"Why should I care?":** This gives the learner the basic map they need before asking AI to “add a route,” “trace the request,” or “show where the response is produced.”

### Code Snippets (pre-extracted)

File: `MiniWebServer\Program.cs` (lines 175-182)
```csharp
        app.MapGet("/string-api/toupper", (context, cancellationToken) =>
        {
            string p = context.Request.QueryParameters["text"].Value ?? string.Empty;

            context.Response.Content = new MiniApp.Content.StringContent(p + " ===> " + p.ToUpper());

            return Task.CompletedTask;
        });
```

File: `MiniWebServer.Server\MiniWebServer.cs` (lines 160-166)
```csharp
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);

                var connectionId = Interlocked.Increment(ref nextClientId); // this function can be called concurrently (or not?) so we cannot use ++

                logger.LogDebug("New client connected! ClientID = {cid}", connectionId);

                Task t = HandleNewClientConnectionAsync(connectionId, binding, client);
```

File: `MiniWebServer.Server\MiniWebClientConnection.cs` (lines 77-90)
```csharp
            if (httpVersion == HttpVersions.Http20) // skip MAGIC string
            {
                requestPipeReader.AdvanceTo(buffer.GetPosition(HTTP2_MAGIC.Length));
            }

            var protocolConfig = new ProtocolHandlerConfiguration(httpVersion, config.MaxRequestBodySize);
            var protocolHandlerContext = new ProtocolHandlerContext()
            {
                PipeReader = requestPipeReader,
                Stream = config.ClientStream
            };

            var protocolHandler = protocolHandlerFactory.Create(httpVersion, protocolConfig, protocolHandlerContext);
```

### Interactive Elements

- [x] **Code↔English translation** — use the route snippet and the `MiniWebClientConnection` protocol-selection snippet.
- [x] **Quiz** — 3 questions, style: tracing/scenario. Angles: “where would you add a new endpoint?”, “what part notices a brand-new client?”, “which layer chooses how to interpret incoming bytes?”
- [x] **Group chat animation** — actors: Browser, Listener, Connection Handler, Protocol Handler, Route Handler. Message flow summary: browser says “GET /string-api/toupper”; listener says “new client arrived”; connection handler says “which HTTP version is this?”; protocol handler says “I know how to read it”; route handler says “I built the answer.”
- [x] **Data flow animation** — actors: Browser, `MiniWebServer`, `MiniWebClientConnection`, `Http11ProtocolHandler`, route handler. Steps: request leaves browser, listener accepts TCP client, connection object reads bytes, protocol handler interprets request, route returns content, response goes back.
- [ ] **Drag-and-drop** — not needed.
- [x] **Other** — numbered step cards summarizing the six-hop journey; small visual file tree highlighting `Program.cs`, `MiniWebServer.cs`, and `MiniWebClientConnection.cs`.

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Group Chat Animation`, `Message Flow / Data Flow Animation`, `Scenario Quiz`, `Glossary Tooltips`, `Visual File Tree`, `Numbered Step Cards`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** None — this is the opening module and should establish the “one request” narrative for the whole course.
- **Next module:** `Meet the Server Crew` — zoom out from one request and name the reusable components that keep appearing in the flow.
- **Tone/style notes:** Use the phrase “request journey” consistently. Keep the route example concrete: `/string-api/toupper?text=hello`.
