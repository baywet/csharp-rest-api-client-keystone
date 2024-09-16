# C# REST API Client Keystone

## Introduction

Base on the [How to be miserable: 40 strategies you already use book](https://www.randypaterson.com/books/how-to-be-miserable.html) approach, this repository aims to demonstrate with benchmarks how you can make any REST API dotnet client as slow and unreliable as possible.

## Patterns

- [Task Calling Result](./src/Benchmarks/CallingResult/)
- [Deserialization with reflection or strings](./src/Benchmarks/Deserialization/)

## Future work

Where analyzers cannot be recommended (no existing work), the plan is to start building some so API consumers can start cleaning the anti-patterns in their code.
