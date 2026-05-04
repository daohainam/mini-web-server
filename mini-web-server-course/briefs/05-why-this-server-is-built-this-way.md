## Module 5: Why This Server Is Built This Way

### Teaching Arc
- **Metaphor:** A warehouse that does not unload the whole truck up front — it only pulls the boxes someone actually needs, and it has separate specialist teams for different package formats.
- **Opening hook:** By now the learner has seen the mechanics. This module explains the design choices hiding behind those mechanics.
- **Key insight:** The server deliberately separates concerns — protocol selection, parsing, middleware, lazy body reading, builder objects, and abstraction layers — to balance extensibility, testability, and performance.
- **"Why should I care?":** This is the “steer AI better” module: it teaches patterns the learner can explicitly request, such as factories, builders, lazy reads, and protocol-specific handlers.

### Code Snippets (pre-extracted)

File: `MiniWebServer.Server\MiniWebClientConnection.cs` (lines 152-160)
```csharp
                                // now we continue reading body part
                                using CancellationTokenSource readBodyCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                                Task readBodyTask = protocolHandler.ReadBodyAsync(requestPipeReader, request, readBodyCancellationTokenSource.Token);
                                Task callMethodTask = CallByMethod(connectionContext, app, request, response, cancellationToken);

                                readBodyCancellationTokenSource.Cancel();

                                // todo: here we need to find a proper way to stop reading body after calling to middlewares and endpoints finished
                                Task.WaitAll([readBodyTask, callMethodTask], cancellationToken);
```

File: `MiniWebServer.Server\ProtocolHandlers\Http11\Http11ProtocolHandler.cs` (lines 176-188)
```csharp
    public async Task ReadBodyAsync(PipeReader reader, IHttpRequest request, CancellationToken cancellationToken)
    {
        // we don't read body part in ReadRequestAsync, because:
        // 1. a body can be very large, and we want to read/process it only when an app requests
        // 2. we want to make it responsive, we can discard a connection right away without reading it's body, there is no reasons to waste our resouces to process an invalid request

        // read body part, we read only contentLength bytes
        var contentLength = request.ContentLength;
        if (contentLength == 0)
        {
            // nothing to read
            await request.BodyPipeline.Writer.CompleteAsync();
```

File: `MiniWebServer.Server\ProtocolHandlerFactory.cs` (lines 21-38)
```csharp
        if (httpVersion == HttpVersions.Http11)
        {
            // in reality we often use default parsers

            return new Http11ProtocolHandler(config, loggerFactory,
                services.GetService<IHttpComponentParser>() ?? new ByteSequenceHttpParser(loggerFactory),
                services.GetService<ICookieValueParser>() ?? new DefaultCookieParser(),
                protocolHandlerContext
                );
        }
        else if (httpVersion == HttpVersions.Http20)
        {
            return new Http2ProtocolHandler(loggerFactory,
                services.GetService<IHttpComponentParser>() ?? new ByteSequenceHttpParser(loggerFactory),
                services.GetService<ICookieValueParser>() ?? new DefaultCookieParser(),
                protocolHandlerContext
                );
        }
```

### Interactive Elements

- [x] **Code↔English translation** — use the lazy-body-reading snippets and the protocol factory block.
- [x] **Quiz** — 3 questions, style: architecture decisions. Angles: “why not read the whole body immediately?”, “why separate HTTP/1.1 and HTTP/2 handlers?”, “what benefit do builders/abstractions buy when AI-generated code needs to change later?”
- [ ] **Group chat animation** — not required.
- [ ] **Data flow animation** — optional; not required.
- [ ] **Drag-and-drop** — not required.
- [x] **Other** — pattern cards for Builder Pattern, Abstraction Layer, Lazy Body Read, Protocol-Specific Handler, and maybe a simple layer-toggle comparing “all-in-one server” vs “modular server.”

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Multiple-Choice Quizzes`, `Pattern/Feature Cards`, `Layer Toggle Demo`, `Callout Boxes`, `Glossary Tooltips`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** `The Middleware Conveyor Belt` — the learner just saw runtime ordering; this module explains why the system is modular in the first place.
- **Next module:** `Debugging the Three Failure Modes` — after understanding the design decisions, close by showing how those decisions make debugging more systematic.
- **Tone/style notes:** Pull in the README ideas about builder pattern, abstraction layers, and performance-sensitive parsing, but keep the prose plain-English and visual-first.
