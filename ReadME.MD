
# GitHubActions

This repository contains shared code and actions used to build and release different software maintained by Kareadita.

## ReleaseTools

In house «GitHub actions» tailored towards our stack. Can be used to release C#/Angular applications, or publish C# libraries.

Consumer can use the base [action](./.github/workflows/release-tools.base.yml) to build upon. The action should run only on `pull_request`.

A useage can be found for the tool itself, [here](./.github/workflows/release-kavita-release-tools.yml)!

Alike GitHub actions, the tool runs stage by stage. Each stage has it's own configuration options on top of `Disabled` and `ReleaseTypes`.
Use `Disabled` to disable a stage while keeping the config, and use `ReleaseTypes` to restrict usage to specific releases. The types are: `Stable`, `Nightly`, and `Canary`.

### Stages

**ParseReleaseTypesStage**

Uses the branch being merged (e.g. `bugfix/scanner`) to decide which release types are active. By default the following rules are active

`release/` => `Stable`

`canary/` => `Canary`

Everything else => `Nightly`

**VersionBumpStage**

Bump the `AssemblyVersion` in a `.csproj` file. Optionally create a commit

**RunScriptStage**

Run a custom script

**GenerateOpenApiStage**

Generate openapi with swagger. Optionally create a commit

**FlushGitChangesStage**

Commit changes, and push upstream. Requires you to specify file paths.

**BuildFrontendStage**

Builds the frontend (runs a npm script) and optionally copies the output elsewhere (wwwroot)

**BuildServerStage**

Builds a C# Web application for a set of Runtime Identifiers. With options to copy, delete extra things afterwarss

**BuildLibraryStage**

Builds & packages a C# Class Library into nupkg files

**PublishToNuGetStage**

Publish the nupkg files to NuGet

**DockerStage**

Creates the docker container. While any Dockerfile can be used, it's expected the Docker file does not rebuild the app
and instead uses the tar artficats from the `BuildServerStage`. 

Tags & Images create a matrix for which ones to use. Use {Version} for the {Version} currently being published



**NotifyDiscordStage**

Send an embed to discord created from the PR body & title
