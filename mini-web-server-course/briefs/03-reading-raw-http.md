## Module 3: Reading Raw HTTP

### Teaching Arc
- **Metaphor:** Opening an envelope and reading the address label before deciding what to do with the package inside.
- **Opening hook:** The browser does not send “a C# object” over the wire; it sends text and bytes that must be decoded carefully.
- **Key insight:** HTTP/1.1 requests are read in stages: one line for the request line, several lines for headers, a blank line, and then an optional body.
- **"Why should I care?":** This is how a learner stops treating “400 Bad Request” like magic. They can ask AI to inspect the request line, headers, body length, and parser assumptions with precision.

### Code Snippets (pre-extracted)

File: `Tests\Http11ProtocolTests\ProtocolHandlers\Http11\ReadRequestTests.cs` (lines 18-29)
```csharp
        string requestContent =
            @"GET /index.html?id1=1&id2=2&t1=Mini%20Web%20Server HTTP/1.1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36
Host: localhost:8443
Connection: keep-alive
Cookie: CONSENT=PENDING+243; sw_version=1; JSESSION=10F4DF1D-C4BE-44BA-AF88-81A3AC132E6A
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
Accept-Encoding: gzip, deflate, br
Accept-Language: vi,en-US;q=0.9,en;q=0.8,nb;q=0.7
Cache-Control:no-cache

";
```

File: `MiniWebServer.Server\ProtocolHandlers\Http11\Http11ProtocolHandler.cs` (lines 50-70)
```csharp
            if (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                var requestLine = httpComponentParser.ParseRequestLine(line);

                if (requestLine != null)
                {
                    logger.LogDebug("Parsed request line: {requestLine}", requestLine);

                    // when implementing as a sequence of octets, if method length exceeds method buffer length, you should return 501 Not Implemented 
                    // if Url length exceeds Url buffer length, you should return 414 URI Too Long

                    // todo: parse the Url with percent-encoding (https://www.rfc-editor.org/rfc/rfc3986)

                    httpMethod = requestLine.Method;
                    requestBuilder
                        .SetMethod(requestLine.Method)
                        .SetUrl(requestLine.Url)
                        .SetParameters(requestLine.Parameters)
                        .SetQueryString(requestLine.QueryString)
                        .SetHash(requestLine.Hash)
                        .SetSegments(requestLine.Segments);
```

File: `MiniWebServer.HttpParser\Http11\ByteSequenceHttpParser.cs` (lines 119-137)
```csharp
            if (TryParseUrl(buffer.Slice(0, pos.Value), out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters) && !string.IsNullOrEmpty(url)) // url cannot be missing
            {
                buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));

                // remove last CR
                var ba = buffer.ToArray();
                if (HTTP1_1_CR_Bytes.SequenceEqual(ba) || HTTP1_1_Bytes.SequenceEqual(ba)) // todo: can we use memory pool instead of ToArray?
                {
                    HttpRequestLine requestLine = new(
                        httpMethod,
                        url,
                        hash ?? string.Empty,
                        queryString ?? string.Empty,
                        new HttpProtocolVersion("1", "1"),
                        segments ?? [],
                        parameters ?? []
                        );

                    return requestLine;
```

### Interactive Elements

- [x] **Code↔English translation** — use the raw test request and the `TryReadLine` + `ParseRequestLine` snippet together; optionally a second translation for `TryParseUrl`.
- [x] **Quiz** — 3 questions, style: tracing/debugging. Angles: “which part of the request line carries the path?”, “what kind of malformed input would lead to a 400?”, “why does the parser check the HTTP version string?”
- [ ] **Group chat animation** — not required.
- [x] **Data flow animation** — actors: Request Buffer, `TryReadLine`, Parser, Request Builder. Steps: read request line, split fields, validate version, add parsed parts to builder, continue to headers.
- [x] **Drag-and-drop** — match request parts (`GET`, `/index.html?...`, `HTTP/1.1`, `Host`, blank line, body) to their meanings.
- [x] **Other** — flow diagram of request-line anatomy and glossary tooltips for request line, header, query string, protocol, parser.

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Multiple-Choice Quizzes`, `Drag-and-Drop Matching`, `Message Flow / Data Flow Animation`, `Flow Diagrams`, `Glossary Tooltips`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** `Meet the Server Crew` — the learner just met the parser and protocol handler; this module opens them up.
- **Next module:** `The Middleware Conveyor Belt` — once a request object exists, the question becomes “who gets first crack at handling it?”
- **Tone/style notes:** Make HTTP feel friendly and concrete. Explicitly define request line, header, and body the first time each term appears in this module.
