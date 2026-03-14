# Coding Agent Instructions

This file provides guidance to coding agents when working with code in this repository.

## Build, test, and lint commands

- From the repository root, use `./build.ps1` as the authoritative full validation command. It bootstraps the required .NET SDK from `global.json` if needed, publishes `src\EurovisionHue`, and runs the full test suite unless `-SkipTests` is passed.
- Run the full test project directly with `dotnet test .\tests\EurovisionHue.Tests\EurovisionHue.Tests.csproj --configuration Release`.
- Run a single test with a filter and disable coverage collection, otherwise the test project-wide 80% coverage threshold will fail targeted runs: `dotnet test .\tests\EurovisionHue.Tests\EurovisionHue.Tests.csproj --configuration Release --filter "FullyQualifiedName~MartinCostello.EurovisionHue.AppTests.Application_Runs_Successfully" /p:CollectCoverage=false`
- There is no single repo-local lint script. The CI lint workflow is the source of truth:
  - PowerShell scripts: `Invoke-ScriptAnalyzer -Path $PWD -Recurse -ReportSummary -Settings @{ IncludeDefaultRules = $true; Severity = @('Error', 'Warning') }`
  - Markdown: `markdownlint-cli2 "**/*.md"` with `.markdownlint.json`
  - GitHub Actions: `actionlint` and `zizmor`

## High-level architecture

- This repository contains a .NET console app in `src\EurovisionHue` and a single xUnit v3 test project in `tests\EurovisionHue.Tests`.
- `Program.cs` is intentionally thin: it handles the special `--install-deps` Playwright bootstrap path, wires Ctrl+C to a cancellation token, and then calls `App.RunAsync()`.
- `App.RunAsync()` is the orchestration entry point. It builds configuration from `appsettings.json`, a persisted user settings file under `%LOCALAPPDATA%\$USER\EurovisionHue\usersettings.json`, environment variables, and user secrets, then builds the DI container via `ServiceCollectionExtensions.AddEurovisionHue()`.
- The runtime flow is:
  1. `LightsClientFactory` discovers Hue bridges, prompts for bridge authorization if no token is stored, and persists `HueToken` plus selected `LightIds`.
  2. `EurovisionFeed` launches Playwright Chromium, loads the configured live feed, polls it on the configured interval, and yields participants as an `IAsyncEnumerable<Participant>`.
  3. `Participants.TryFind()` maps feed text to a Eurovision participant using the precomputed name lookup in `Participants.cs`.
  4. `Participant.Colors()` loads embedded flag PNG resources from `Flags\*.png` and extracts dominant colors with ImageSharp.
  5. `LightsClient.ChangeAsync()` converts RGB colors to Hue XY values and sends updates through `HueApi`.
- Feed configuration is deliberately layered. `GitHubGistAppOptions` fills in missing `FeedUrl` and `ArticleSelector` values from a GitHub Gist, and `App` sets a reload timer so configuration can refresh while the app is running.
- `ServiceCollectionExtensions` is the DI composition root. It registers the app services, configures the shared `HttpClient`, enables the standard resilience handler, adds the app version as the user agent, and accepts the Hue bridge's self-signed certificate.
- The tests are not just unit tests. `BrowserFixture` installs Playwright Chromium and uses the repository's `demo.html` and `invalid.html` files as local feeds, while `AppFixture` uses `Spectre.Console.Testing` plus `JustEat.HttpClientInterception` to fake Hue bridge and GitHub Gist traffic. `AppTests` exercise the full application flow by overriding DI registrations rather than by spinning up separate infrastructure.

## Key conventions

- Respect the repo formatting rules from `.editorconfig`: 4 spaces for code, 2 spaces for `json`, `csproj`, `props`, `targets`, and `yml`, and UTF-8 BOM for `*.cs` files.
- New C# files are expected to keep the existing copyright header and use file-scoped namespaces.
- Package versions are centrally managed in `Directory.Packages.props`, and builds write into the shared `artifacts` layout enabled in `Directory.Build.props`.
- Keep DI changes centralized in `ServiceCollectionExtensions.AddEurovisionHue()` unless a test is intentionally overriding registrations.
- Persist only user-specific bridge state through `AppOptions.SaveAsync()`. That file stores `HueToken` and `LightIds`; feed settings come from app settings, environment variables, user secrets, or the remote gist layer.
- When changing feed detection behavior, keep the optimized participant lookup approach in `Participants.cs` and remember that alternate country names are part of the supported matching behavior.
- When changing tests, remember that the test project enables coverage by default with an 80% threshold. Targeted `dotnet test` runs need `/p:CollectCoverage=false` unless the selected subset still satisfies coverage.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
