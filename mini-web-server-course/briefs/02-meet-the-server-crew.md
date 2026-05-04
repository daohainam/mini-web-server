## Module 2: Meet the Server Crew

### Teaching Arc
- **Metaphor:** A film set where each crew member has a narrow job: director, camera, editor, lighting, sound. The final scene works because specialists coordinate.
- **Opening hook:** Once the learner sees a request move through the server, the next question is: who are these parts, exactly?
- **Key insight:** This codebase is intentionally split into small roles — app builder, middleware, server builder, protocol handler factory, and app host — so the server can evolve without one giant class absorbing everything.
- **"Why should I care?":** This is the vocabulary module: it teaches the learner how to tell AI “put this in middleware,” “this belongs in the protocol handler,” or “this should be configured at startup.”

### Code Snippets (pre-extracted)

File: `MiniWebServer\Program.cs` (lines 118-128)
```csharp
        appBuilder.UseAuthorization(options =>
        {
            options.Policies.Add("is-mini-web-server", policy => policy.RequireClaimValue(ClaimTypes.NameIdentifier, "mini-web-server"));
            options.Policies.Add("dev-required", policy => policy.RequireClaimValue(ClaimTypes.Role, "Developer"));

            options.Routes.Add("/helpcheck", "is-mini-web-server", "dev-required");
        });

        appBuilder.UseSession();
        appBuilder.UseStaticFiles("wwwroot", defaultMaxAge: 7 * 24 * 3600); // defaultMaxAge = 7 days
        appBuilder.UseMvc();
```

File: `MiniWebServer\Program.cs` (lines 52-54)
```csharp
        IMiniApp app = BuildApp(serverBuilder.Services, demoAppConfig, httpsPort); // or server.CreateAppBuilder(); ?
        app = MapRoutes(app);
        serverBuilder.AddHost(string.Empty, app);
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

- [x] **Code↔English translation** — translate the `BuildApp` middleware chain and the `ProtocolHandlerFactory` choice block.
- [x] **Quiz** — 3 questions, style: architecture/scenario. Angles: “where would a new caching feature live?”, “if you support a new HTTP version, where would that decision go?”, “what kind of thing belongs in startup wiring versus request-time code?”
- [ ] **Group chat animation** — not required in this module.
- [ ] **Data flow animation** — not required in this module.
- [ ] **Drag-and-drop** — not required.
- [x] **Other** — interactive architecture diagram with five actors: Startup/Program, MiniApp Builder, MiniWebServer, Protocol Handler Factory, Route/Middleware layer. Add feature cards for Auth, Session, Static Files, MVC, HTTP/1.1, HTTP/2.

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Multiple-Choice Quizzes`, `Interactive Architecture Diagram`, `Pattern/Feature Cards`, `Glossary Tooltips`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** `One Click, One Request` — the learner just saw the trip; now they learn the cast of characters behind it.
- **Next module:** `Reading Raw HTTP` — after naming the actors, zoom into the most mysterious one: the parser/protocol handler that turns bytes into a request object.
- **Tone/style notes:** Treat components like named characters. Avoid dumping too many class names at once; pair every class with a plain-English role.
