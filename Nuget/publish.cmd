SET PublishProfile=Debug
SET NugetSource=https://api.nuget.org/v3/index.json

nuget push ..\Middleware\Authentication\bin\%PublishProfile%\MiniWebServer.Authentication.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\Authorization\bin\%PublishProfile%\MiniWebServer.Authorization.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\HttpsRedirection\bin\%PublishProfile%\MiniWebServer.HttpsRedirection.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\Mvc\bin\%PublishProfile%\MiniWebServer.Mvc.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\OutputCaching\bin\%PublishProfile%\MiniWebServer.OutputCaching.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\Session\bin\%PublishProfile%\MiniWebServer.Session.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\Middleware\StaticFiles\bin\%PublishProfile%\MiniWebServer.StaticFiles.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Abstractions\bin\%PublishProfile%\MiniWebServer.Abstractions.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Configuration\bin\%PublishProfile%\MiniWebServer.Configuration.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.HttpParser\bin\%PublishProfile%\MiniWebServer.HttpParser.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.MimeMapping\bin\%PublishProfile%\MiniWebServer.MimeMapping.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Mvc.Abstraction\bin\%PublishProfile%\MiniWebServer.Mvc.Abstraction.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Mvc.MiniRazorEngine\bin\%PublishProfile%\MiniWebServer.Mvc.MiniRazorEngine.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Mvc.RazorLightTemplateParser\bin\%PublishProfile%\MiniWebServer.Mvc.RazorLightTemplateParser.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Server\bin\%PublishProfile%\MiniWebServer.Server.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate
nuget push ..\MiniWebServer.Server.Abstractions\bin\%PublishProfile%\MiniWebServer.Server.Abstractions.0.3.1.nupkg -Source %NugetSource% -SkipDuplicate