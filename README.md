# C# REST API Client Keystone

## Introduction

Base on the [How to be miserable: 40 strategies you already use book](https://www.randypaterson.com/books/how-to-be-miserable.html) approach, this repository aims to demonstrate with benchmarks how you can make any REST API dotnet client as slow and unreliable as possible.

## Structure

Each anti-pattern is demonstrated in a benchmark test to be compared against the preferred implementation pattern. The results of all the anti-patterns when compared with their respective preferred implementations are published in an associated readme file. In these readme files you'll also find implementation guidelines as well as recommended analyzers to avoid the anti-patterns at the first place.

## Patterns

- [Task Calling Result](./src/Benchmarks/CallingResult/)
- [Deserialization with reflection or strings](./src/Benchmarks/Deserialization/)

## Future work

Where analyzers cannot be recommended (no existing work), the plan is to start building some so API consumers can start cleaning the anti-patterns in their code.

## Getting started

### Running the benchmarks or contributing to the analyzers

Required tools:

- .NET SDK 8 `winget install Microsoft.DotNet.SDK.8`.
- Visual Studio Code `winget install Microsoft.VisualStudioCode` or Visual Studio 2022 `Microsoft.VisualStudio.2022.Community`.
- Git `winget install Git.Git`.
- GitHub CLI `winget install GitHub.CLI`.

Running the benchmarks:

```shell
gh repo clone baywet/csharp-rest-api-client-keystone`
cd csharp-rest-api-client-keystone
dotnet run --project src\Benchmarks\Benchmarks.csproj
```

Running unit tests

```shell
dotnet test
```

### Using the JSON code analyzers

> NOTE: at the moment the library is NOT published as a nuget package, you'll need a direct reference to the project

```shell
gh repo clone baywet/csharp-rest-api-client-keystone`
# in the target project
dotnet add <pathToTargetProject> reference $PWD/src/CSharpRestApiClientKeystone.Analyzers/CSharpRestApiClientKeystone.Analyzers.csproj
dotnet build
```
