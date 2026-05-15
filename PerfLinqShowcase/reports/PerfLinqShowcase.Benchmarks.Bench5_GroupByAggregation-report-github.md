```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD


```
| Method                                                       | Scale  | Mean         | StdDev     | Error      | Ratio | RatioSD | Rank | Gen0     | Gen1    | Allocated  | Alloc Ratio |
|------------------------------------------------------------- |------- |-------------:|-----------:|-----------:|------:|--------:|-----:|---------:|--------:|-----------:|------------:|
| &#39;Foreach: Dictionary accumulation + sort output (baseline)&#39;  | Large  | 1,353.152 μs | 11.8441 μs | 12.6620 μs |  1.00 |    0.01 |    1 |        - |       - |     5.7 KB |        1.00 |
| &#39;LINQ: SelectMany → Where → GroupBy → Select → OrderBy&#39;      | Large  | 5,151.011 μs | 97.8807 μs | 99.6613 μs |  3.81 |    0.08 |    2 | 187.5000 | 62.5000 | 1551.76 KB |      272.46 |
| &#39;LINQ: GroupBy + Count() before Select (triple enumeration)&#39; | Large  | 5,387.445 μs | 43.9774 μs | 49.6094 μs |  3.98 |    0.05 |    2 | 242.1875 | 78.1250 |  2030.2 KB |      356.47 |
|                                                              |        |              |            |            |       |         |      |          |         |            |             |
| &#39;Foreach: Dictionary accumulation + sort output (baseline)&#39;  | Medium |    58.668 μs |  0.4176 μs |  0.4465 μs |  1.00 |    0.01 |    1 |   0.3662 |       - |    3.15 KB |        1.00 |
| &#39;LINQ: GroupBy + Count() before Select (triple enumeration)&#39; | Medium |   419.282 μs |  1.9290 μs |  2.1760 μs |  7.15 |    0.06 |    2 |  36.6211 |  4.8828 |  300.58 KB |       95.47 |
| &#39;LINQ: SelectMany → Where → GroupBy → Select → OrderBy&#39;      | Medium |   463.701 μs |  2.8955 μs |  3.2663 μs |  7.90 |    0.07 |    3 |  26.8555 |  2.9297 |  219.74 KB |       69.79 |
|                                                              |        |              |            |            |       |         |      |          |         |            |             |
| &#39;Foreach: Dictionary accumulation + sort output (baseline)&#39;  | Small  |     4.167 μs |  0.0147 μs |  0.0157 μs |  1.00 |    0.00 |    1 |   0.2823 |       - |    2.35 KB |        1.00 |
| &#39;LINQ: SelectMany → Where → GroupBy → Select → OrderBy&#39;      | Small  |    22.711 μs |  0.0773 μs |  0.0873 μs |  5.45 |    0.03 |    2 |   3.3264 |  0.0610 |   27.28 KB |       11.60 |
| &#39;LINQ: GroupBy + Count() before Select (triple enumeration)&#39; | Small  |    35.164 μs |  0.0810 μs |  0.0970 μs |  8.44 |    0.03 |    3 |   4.7607 |  0.1221 |   39.27 KB |       16.70 |
