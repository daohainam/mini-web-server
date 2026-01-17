# GitHub Actions Workflow for Mini Web Server

This directory contains the GitHub Actions workflow configuration for automatically building, testing, and deploying Mini Web Server NuGet packages.

## Workflows

### build-test-deploy.yml

This workflow handles the complete CI/CD pipeline:

#### Triggers
- **Push to main/master**: Runs build and tests only
- **Pull Requests**: Runs build and tests only
- **Releases**: Runs build, tests, and deploys to NuGet.org
- **Manual trigger**: Can be triggered manually via workflow_dispatch

#### Jobs

**1. Build and Test**
- Checks out the code
- Sets up .NET 10.0
- Restores dependencies
- Builds the solution in Release configuration
- Runs all tests
- Uploads generated NuGet packages as artifacts

**2. Deploy to NuGet.org**
- Only runs on release events
- Downloads the NuGet packages from the build job
- Pushes all packages to NuGet.org
- Uses `--skip-duplicate` to avoid errors on already published versions

## Setup Requirements

### Required Secrets

To enable automatic deployment to NuGet.org, you need to configure the following secret in your GitHub repository:

1. **NUGET_API_KEY**: Your NuGet.org API key
   - Go to https://www.nuget.org/account/apikeys
   - Create a new API key with "Push" permission
   - Add it as a repository secret:
     - Go to your repository → Settings → Secrets and variables → Actions
     - Click "New repository secret"
     - Name: `NUGET_API_KEY`
     - Value: Your API key from NuGet.org

### .NET Version

The workflow is configured to use .NET 10.0. If you need to change this:
- Update the `DOTNET_VERSION` environment variable in the workflow file
- Ensure all project files use compatible target frameworks

## Usage

### Automatic Deployment

To deploy packages to NuGet.org:

1. Ensure all package versions are updated in the `.csproj` files
2. Create and publish a GitHub release:
   ```bash
   git tag v0.5.0
   git push origin v0.5.0
   ```
3. Go to GitHub → Releases → Draft a new release
4. Select the tag and publish the release
5. The workflow will automatically build, test, and deploy all packages

### Manual Testing

You can test the build and test jobs without deploying by:
- Pushing to main/master branch
- Opening a pull request
- Using the "Run workflow" button in the Actions tab

## Package List

The workflow will generate and deploy the following packages:
- MiniWebServer.Abstractions
- MiniWebServer.Authentication
- MiniWebServer.Authorization
- MiniWebServer.CgiMiddleware
- MiniWebServer.Configuration
- MiniWebServer.Helpers
- MiniWebServer.HttpParser
- MiniWebServer.HttpsRedirection
- MiniWebServer.HstsMiddleware
- MiniWebServer.MimeMapping
- MiniWebServer.MiniApp
- MiniWebServer.Mvc
- MiniWebServer.Mvc.Abstraction
- MiniWebServer.Mvc.MiniRazorEngine
- MiniWebServer.Mvc.RazorLightTemplateParser
- MiniWebServer.OutputCaching
- MiniWebServer.Server
- MiniWebServer.Server.Abstractions
- MiniWebServer.Session
- MiniWebServer.StaticFiles
- MiniWebServer.WebSocket

## Notes

- The workflow uses the `GeneratePackageOnBuild` property in project files to create NuGet packages during build
- Failed tests will prevent deployment
- The `--skip-duplicate` flag prevents errors when a version already exists on NuGet.org
- Artifacts are retained for 7 days for debugging purposes
