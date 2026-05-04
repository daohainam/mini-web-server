## Module 6: Debugging the Three Failure Modes

### Teaching Arc
- **Metaphor:** Three warning lights on a machine: one says “the input was unreadable,” one says “nobody claimed this job,” and one says “the worker crashed while doing it.”
- **Opening hook:** Web server bugs feel random until you realize most request failures in this codebase collapse into three big buckets: 400, 404, and 500.
- **Key insight:** The server uses a deliberate failure map: parse/read errors become 400, missing handlers become 404, and exceptions during request handling become 500.
- **"Why should I care?":** This gives the learner a calm debugging checklist and the language to tell AI exactly where to look first.

### Code Snippets (pre-extracted)

File: `MiniWebServer.Server\MiniWebClientConnection.cs` (lines 116-126)
```csharp
                    var readRequestResult = await protocolHandler.ReadRequestAsync(requestBuilder, cancellationToken);
                    if (!readRequestResult)
                    {
                        isKeepAlive = false; // we always close wrongly working connections

                        var response = new HttpResponse(HttpResponseCodes.BadRequest, config.ClientStream);

                        cancellationTokenSource.CancelAfter(config.SendResponseTimeout);
                        logger.LogDebug("[{cid}] - Sending back response...", ConnectionId); // send back Bad Request
                        await SendResponseAsync(response, protocolHandler, cancellationToken);
```

File: `MiniWebServer.Server\MiniWebClientConnection.cs` (lines 316-328)
```csharp
                try
                {
                    await action.InvokeAsync(context, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{cid}] - Error executing action handler", ConnectionId);
                    response.StatusCode = HttpResponseCodes.InternalServerError;
                }
            }
            else
            {
                StandardResponseBuilderHelpers.NotFound(response);
```

File: `MiniWebServer.Server\Http\Helpers\StandardResponseBuilderHelpers.cs` (lines 5-11)
```csharp
public class StandardResponseBuilderHelpers
{
    public static void NotFound(IHttpResponse response)
    {
        response.StatusCode = HttpResponseCodes.NotFound;
        response.Headers.ContentLength = 0;
    }
```

File: `Tests\Http11ProtocolTests\ProtocolHandlers\Http11\ReadRequestTests.cs` (lines 66-81)
```csharp
    private static async Task<ReadRequestTestResult> ReadRequestAsync(string requestContent)
    {
        var reader = PipeUtils.String2Reader(requestContent);
        var loggerFactory = new LoggerFactory();

        var httpParser = new ByteSequenceHttpParser(loggerFactory);
        var handler = new Http11ProtocolHandler(new ProtocolHandlerConfiguration(global::MiniWebServer.Abstractions.HttpVersions.Http11, 1024 * 1024 * 10), loggerFactory, httpParser, new DefaultCookieParser(), 
            new ProtocolHandlerContext() { 
                PipeReader = reader, Stream = new MemoryStream()
            });

        var requestBuilder = new HttpWebRequestBuilder();

        var result = await handler.ReadRequestAsync(requestBuilder, CancellationToken.None);
        return new ReadRequestTestResult(result, requestBuilder);
```

### Interactive Elements

- [x] **Code↔English translation** — translate the 400/404/500 snippets and the isolated parser test setup.
- [x] **Quiz** — 4 questions, style: debugging/scenario. Angles: “malformed headers vs missing route vs crashed route handler,” “which file to inspect first for each symptom,” “why is 404 more helpful than 500 for an unmatched route?”, “how can tests isolate parser behavior?”
- [ ] **Group chat animation** — not required.
- [ ] **Data flow animation** — optional; not required.
- [ ] **Drag-and-drop** — not required.
- [x] **Other** — a three-lane diagnostic diagram (`400 Read`, `404 Find`, `500 Execute`) plus a “spot the bug” challenge where the learner classifies a failure into the correct lane.

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Multiple-Choice Quizzes`, `"Spot the Bug" Challenge`, `Flow Diagrams`, `Callout Boxes`, `Glossary Tooltips`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** `Why This Server Is Built This Way` — the learner just saw the architectural reasoning; now they see the practical debugging payoff.
- **Next module:** None — this is the final module and should end with an empowering recap of the full request lifecycle.
- **Tone/style notes:** End on confidence, not fear. The takeaway is “you now know where to look,” not “servers are fragile and scary.”
