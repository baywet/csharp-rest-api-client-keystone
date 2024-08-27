# JSON Deserialization

## Guidelines

- MUST NOT use intermediate strings during deserialization.
- MUST NOT use newtonsoft JSON, favor System.Text.Json.
- SHOULD provide type info.

## Results

| Method                                          | Mean     | Error      | StdDev    | Gen0   | Gen1   | Allocated |
|------------------------------------------------ |---------:|-----------:|----------:|-------:|-------:|----------:|
| DeserializeAsClassWithNewtonSoftAsync           | 2.722 us |  1.7064 us | 0.0935 us | 1.2131 | 0.0153 |    7624 B |
| DeserializeAsStructWithNewtonSoftAsync          | 2.906 us |  8.6042 us | 0.4716 us | 1.2321 | 0.0153 |    7736 B |
| DeserializeAsClassWithStringAndReflectionAsync  | 3.780 us | 12.8128 us | 0.7023 us | 0.8202 | 0.0114 |    5152 B |
| DeserializeAsStructWithStringAndReflectionAsync | 3.270 us |  7.2542 us | 0.3976 us | 0.8163 | 0.0076 |    5136 B |
| DeserializeAsClassWithReflectionAsync           | 1.378 us |  2.7192 us | 0.1490 us | 0.1411 |      - |     888 B |
| DeserializeAsStructWithReflectionAsync          | 1.160 us |  0.5263 us | 0.0288 us | 0.1373 |      - |     872 B |
| DeserializeAsClassWithTypeInfoAsync             | 1.353 us |  2.7396 us | 0.1502 us | 0.1411 |      - |     888 B |
| DeserializeAsStructWithTypeInfoAsync            | 1.344 us |  1.0818 us | 0.0593 us | 0.1373 |      - |     872 B |

## Analysis

Using intermediate strings to deserialize results in unnecessary allocations, which in turns leads to additional garbage collection. Garbage collection has been optimized between dotnet framework and dotnet core. If the cost remain high with net8, it will be much higher with netfx.

Passing type info does not appear to significantly improve deserialization, it however enables trimming through removing the use of reflection. Which itself results in smaller overall storage/memory footprint, shorter startup times, reduced memory consumption, etc...

## Rules

No pre-existing work, will require implementation:

- DJSON001 - Use System.Text.Json.
- DJSON002 - Use Deserialize Stream overloads.
- DJSON003 - Use Deserialize with a type info overload.
