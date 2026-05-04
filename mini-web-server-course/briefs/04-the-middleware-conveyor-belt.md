## Module 4: The Middleware Conveyor Belt

### Teaching Arc
- **Metaphor:** An airport security conveyor belt where the order of stations matters: ID check before gate access, scanner before boarding, and any station can stop the bag from going further.
- **Opening hook:** The request has been parsed. Now the real question is: which feature gets to handle it first?
- **Key insight:** In this server, middleware is wrapped in last-in-first-out order, so the order in startup code directly changes runtime behavior.
- **"Why should I care?":** This is the module that lets the learner catch AI mistakes like “auth runs after authorization” or “MVC grabbed the route before static files.”

### Code Snippets (pre-extracted)

File: `MiniWebServer.MiniApp\Builders\MiniAppBuilder.cs` (lines 60-63)
```csharp
        if (typeof(IMiddleware).IsAssignableFrom(middlewareType))
        {
            middlewareTypes.Insert(0, middlewareType); // LIFO: last added middleware will be called first
        }
```

File: `MiniWebServer.MiniApp\BaseMiniApp.cs` (lines 43-50)
```csharp
        foreach (var middleware in middlewareChain)
        {
            var callWrapper = new MiddlewareWrapper(middleware, callable);

            callable = callWrapper;
        }

        return callable;
```

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

### Interactive Elements

- [x] **Code↔English translation** — translate `Insert(0, middlewareType)` and the wrapper-building loop; optionally a second translation for the startup middleware chain.
- [x] **Quiz** — 3 questions, style: debugging/architecture. Angles: “what happens if authorization runs before authentication?”, “how could `UseStaticFiles` and `UseMvc` compete?”, “which middleware order would you pick for a protected route plus static assets?”
- [x] **Group chat animation** — actors: Authentication, Authorization, Session, Static Files, MVC, Final 404 Handler. Message flow summary: each actor decides whether to pass the request onward or finish the job; show one path for `/helpcheck` and one for a static file.
- [ ] **Data flow animation** — optional, not required here.
- [x] **Drag-and-drop** — reorder middleware cards into a sensible chain for a sample app.
- [x] **Other** — callout box about “last added, first called,” plus permission/config badges for protected routes and file-serving behavior.

### Reference Files to Read

- `references/interactive-elements.md` → `Code ↔ English Translation Blocks`, `Multiple-Choice Quizzes`, `Drag-and-Drop Matching`, `Group Chat Animation`, `Callout Boxes`, `Permission/Config Badges`, `Glossary Tooltips`
- `references/design-system.md` → `Module Structure`, `Code Block Globals`
- `references/content-philosophy.md` → all
- `references/gotchas.md` → all

### Connections

- **Previous module:** `Reading Raw HTTP` — the request is now parsed into a structured object.
- **Next module:** `Why This Server Is Built This Way` — after seeing the chain in motion, step back and ask why the author split the system into these pieces.
- **Tone/style notes:** Emphasize consequences, not just mechanics. The learner should leave with a strong “order matters” instinct.
