```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD


```
| Method                                                              | EmployeeCount | Mean       | StdDev     | Error      | Median     | Ratio | RatioSD | Rank | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|-------------------------------------------------------------------- |-------------- |-----------:|-----------:|-----------:|-----------:|------:|--------:|-----:|--------:|--------:|--------:|----------:|------------:|
| &#39;Foreach: single-pass sum (baseline)&#39;                               | 500           |   1.329 μs |  0.0031 μs |  0.0033 μs |   1.329 μs |  1.00 |    0.00 |    1 |       - |       - |       - |         - |          NA |
| &#39;LINQ: Where → Select → Sum (3 operators)&#39;                          | 500           |   2.543 μs |  0.0076 μs |  0.0081 μs |   2.543 μs |  1.91 |    0.01 |    2 |  0.0191 |       - |       - |     160 B |          NA |
| &#39;LINQ: Where → Count + Where → Sum (double enumeration)&#39;            | 500           |   3.973 μs |  0.0219 μs |  0.0234 μs |   3.971 μs |  2.99 |    0.02 |    3 |  0.0076 |       - |       - |      72 B |          NA |
| &#39;LINQ: Where → ToList → Select → Sum (unnecessary materialisation)&#39; | 500           |   4.477 μs |  0.0183 μs |  0.0195 μs |   4.473 μs |  3.37 |    0.02 |    4 |  0.3586 |       - |       - |    3008 B |          NA |
|                                                                     |               |            |            |            |            |       |         |      |         |         |         |           |             |
| &#39;Foreach: single-pass sum (baseline)&#39;                               | 5000          |  15.370 μs |  0.0427 μs |  0.0546 μs |  15.379 μs |  1.00 |    0.00 |    1 |       - |       - |       - |         - |          NA |
| &#39;LINQ: Where → Select → Sum (3 operators)&#39;                          | 5000          |  34.833 μs |  0.1896 μs |  0.2271 μs |  34.849 μs |  2.27 |    0.01 |    2 |       - |       - |       - |     160 B |          NA |
| &#39;LINQ: Where → Count + Where → Sum (double enumeration)&#39;            | 5000          |  50.347 μs |  0.2078 μs |  0.2344 μs |  50.322 μs |  3.28 |    0.02 |    3 |       - |       - |       - |      72 B |          NA |
| &#39;LINQ: Where → ToList → Select → Sum (unnecessary materialisation)&#39; | 5000          |  57.489 μs |  0.3705 μs |  0.4179 μs |  57.365 μs |  3.74 |    0.03 |    4 |  3.4790 |  0.3052 |       - |   29416 B |          NA |
|                                                                     |               |            |            |            |            |       |         |      |         |         |         |           |             |
| &#39;Foreach: single-pass sum (baseline)&#39;                               | 50000         | 212.399 μs |  5.2972 μs |  4.0739 μs | 211.708 μs |  1.00 |    0.03 |    1 |       - |       - |       - |         - |          NA |
| &#39;LINQ: Where → Select → Sum (3 operators)&#39;                          | 50000         | 533.747 μs | 52.9255 μs | 18.0459 μs | 509.277 μs |  2.51 |    0.26 |    2 |       - |       - |       - |     160 B |          NA |
| &#39;LINQ: Where → Count + Where → Sum (double enumeration)&#39;            | 50000         | 787.324 μs | 10.0011 μs | 11.9768 μs | 788.304 μs |  3.71 |    0.10 |    3 |       - |       - |       - |      72 B |          NA |
| &#39;LINQ: Where → ToList → Select → Sum (unnecessary materialisation)&#39; | 50000         | 788.703 μs | 34.0342 μs | 15.2167 μs | 781.415 μs |  3.72 |    0.18 |    3 | 27.3438 | 27.3438 | 27.3438 |  294491 B |          NA |
