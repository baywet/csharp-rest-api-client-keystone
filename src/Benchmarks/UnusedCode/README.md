# Unused code

## Guidelines

- MUST NOT call methods returning a value if they do not mutate the state. (effectively causing unnecessary compute)
- MUST NOT assign variables that are not read after the assignment.
- MUST NOT declare unused private members.
- MUST NOT declare unused parameters unless it's to implement an interface.
- MUST NOT have unused usings as it slows down compilation on large repositories.

## Analysis

This set of guidelines is complementary to the other patterns. Effectively the code resolution for other patterns might result in unused code paths, or there might already be unused code in the code base. The most performant code is the one that does not exist.

## Rules

Pre-existing work:

- [ISE0005](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0005)
- [IDE0051](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0051)
- [IDE0052](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0052)
- [IDE0058](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0058)
- [IDE0059](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0059)
- [IDE0060](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0060)