SET PublishProfile=Debug
SET MiniWebServerVersion=0.5.0
SET NugetSource=https://api.nuget.org/v3/index.json

dotnet nuget push ..\Middleware\Authentication\bin\%PublishProfile%\MiniWebServer.Authentication.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\Authorization\bin\%PublishProfile%\MiniWebServer.Authorization.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\HttpsRedirection\bin\%PublishProfile%\MiniWebServer.HttpsRedirection.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\Mvc\bin\%PublishProfile%\MiniWebServer.Mvc.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\OutputCaching\bin\%PublishProfile%\MiniWebServer.OutputCaching.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\Session\bin\%PublishProfile%\MiniWebServer.Session.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\Middleware\StaticFiles\bin\%PublishProfile%\MiniWebServer.StaticFiles.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Abstractions\bin\%PublishProfile%\MiniWebServer.Abstractions.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Configuration\bin\%PublishProfile%\MiniWebServer.Configuration.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Helpers\bin\%PublishProfile%\MiniWebServer.Helpers.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.HttpParser\bin\%PublishProfile%\MiniWebServer.HttpParser.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.MimeMapping\bin\%PublishProfile%\MiniWebServer.MimeMapping.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.MiniApp\bin\%PublishProfile%\MiniWebServer.MiniApp.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Mvc.Abstraction\bin\%PublishProfile%\MiniWebServer.Mvc.Abstraction.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Mvc.MiniRazorEngine\bin\%PublishProfile%\MiniWebServer.Mvc.MiniRazorEngine.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Mvc.RazorLightTemplateParser\bin\%PublishProfile%\MiniWebServer.Mvc.RazorLightTemplateParser.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Server\bin\%PublishProfile%\MiniWebServer.Server.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate
dotnet nuget push ..\MiniWebServer.Server.Abstractions\bin\%PublishProfile%\MiniWebServer.Server.Abstractions.%MiniWebServerVersion%.nupkg --source %NugetSource% --skip-duplicate