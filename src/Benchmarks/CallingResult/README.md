# Task Management

## Guidelines

- MUST NOT call .Result on a Task.
- MUST NOT call .GetAwaiter().GetResult on a Task.
- A method calling an async method MUST be async itself and MUST return Task, Task of T or ValueTask equivalents.
- When a service or a library MUST pass the cancellation token from the caller to the callee.
- MUST call ConfigureAwait when awaiting a Task.

## Results

| Method                            | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------------------------------- |---------:|----------:|----------:|-------:|-------:|----------:|
| CallingResult                     | 3.392 us | 6.4680 us | 0.3545 us | 0.1221 | 0.0458 |     768 B |
| CallingGetResult                  | 3.219 us | 6.5425 us | 0.3586 us | 0.1221 | 0.0420 |     768 B |
| CallingAsync                      | 1.924 us | 4.6868 us | 0.2569 us | 0.1335 | 0.0458 |     840 B |
| CallingWithCancellationTokenAsync | 2.393 us | 0.5324 us | 0.0292 us | 0.1335 | 0.0458 |     856 B |

## Rules

- [CA1849](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1849)
- [CA2016](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2016)
- [CA2007](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007)
- [VSTHRD002](https://github.com/Microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD002.md)
- [VSTHRD100](https://github.com/microsoft/vs-threading/blob/main/doc/analyzers/VSTHRD100.md)
