```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD


```
| Method                                                   | Scale  | Mean            | StdDev        | Error         | Ratio | RatioSD | Rank | Gen0      | Gen1      | Gen2     | Allocated   | Alloc Ratio |
|--------------------------------------------------------- |------- |----------------:|--------------:|--------------:|------:|--------:|-----:|----------:|----------:|---------:|------------:|------------:|
| &#39;Foreach: nested loops with filters (baseline filtered)&#39; | Large  |    346,157.0 ns |   3,974.25 ns |   4,483.21 ns |  0.40 |    0.01 |    1 |   34.1797 |   18.5547 |  18.5547 |   512.47 KB |        0.13 |
| &#39;LINQ: 4× SelectMany → ToList&#39;                           | Large  |    469,372.0 ns |  17,032.53 ns |   8,840.44 ns |  0.55 |    0.02 |    2 |   41.5039 |   31.7383 |  31.7383 |  1330.88 KB |        0.32 |
| &#39;Foreach: nested loops → List (baseline)&#39;                | Large  |    856,997.9 ns |   7,645.81 ns |   8,624.97 ns |  1.00 |    0.01 |    3 |   76.1719 |   60.5469 |  60.5469 |  4096.77 KB |        1.00 |
| &#39;LINQ: 4× SelectMany + Where filters → ToList&#39;           | Large  |    871,016.6 ns |   9,838.30 ns |  10,517.74 ns |  1.02 |    0.01 |    3 |   31.2500 |   12.6953 |  12.6953 |   405.33 KB |        0.10 |
| &#39;LINQ: SelectMany with projection → anonymous type list&#39; | Large  | 14,723,341.3 ns | 224,996.89 ns | 269,442.74 ns | 17.18 |    0.29 |    4 | 1796.8750 | 1046.8750 | 718.7500 | 13876.43 KB |        3.39 |
|                                                          |        |                 |               |               |       |         |      |           |           |          |             |             |
| &#39;Foreach: nested loops with filters (baseline filtered)&#39; | Medium |     23,187.3 ns |     118.27 ns |     133.41 ns |  0.27 |    0.01 |    1 |    7.8430 |    0.9460 |        - |    64.26 KB |        0.25 |
| &#39;LINQ: 4× SelectMany → ToList&#39;                           | Medium |     62,904.8 ns |     393.89 ns |     421.09 ns |  0.73 |    0.01 |    2 |   30.2734 |   30.2734 |  30.2734 |   110.73 KB |        0.43 |
| &#39;LINQ: 4× SelectMany + Where filters → ToList&#39;           | Medium |     72,891.9 ns |     250.54 ns |     300.03 ns |  0.85 |    0.02 |    3 |    5.9814 |         - |        - |    49.09 KB |        0.19 |
| &#39;Foreach: nested loops → List (baseline)&#39;                | Medium |     86,254.5 ns |   1,651.95 ns |   1,608.64 ns |  1.00 |    0.03 |    4 |   41.6260 |   41.6260 |  41.6260 |   256.33 KB |        1.00 |
| &#39;LINQ: SelectMany with projection → anonymous type list&#39; | Medium |    826,353.4 ns |  14,170.21 ns |  15,984.91 ns |  9.58 |    0.25 |    5 |  149.4141 |   83.9844 |  41.0156 |  1151.09 KB |        4.49 |
|                                                          |        |                 |               |               |       |         |      |           |           |          |             |             |
| &#39;Foreach: nested loops with filters (baseline filtered)&#39; | Small  |        923.5 ns |       1.75 ns |       2.10 ns |  0.34 |    0.00 |    1 |    0.2613 |    0.0010 |        - |     2.14 KB |        0.13 |
| &#39;Foreach: nested loops → List (baseline)&#39;                | Small  |      2,720.8 ns |      12.69 ns |      15.20 ns |  1.00 |    0.01 |    2 |    1.9836 |    0.0839 |        - |    16.21 KB |        1.00 |
| &#39;LINQ: 4× SelectMany → ToList&#39;                           | Small  |      3,353.0 ns |      11.95 ns |      13.48 ns |  1.23 |    0.01 |    3 |    0.9384 |    0.0153 |        - |     7.69 KB |        0.47 |
| &#39;LINQ: 4× SelectMany + Where filters → ToList&#39;           | Small  |      6,084.3 ns |      24.18 ns |      25.85 ns |  2.24 |    0.01 |    4 |    0.6561 |         - |        - |      5.4 KB |        0.33 |
| &#39;LINQ: SelectMany with projection → anonymous type list&#39; | Small  |     27,066.8 ns |     150.58 ns |     160.98 ns |  9.95 |    0.07 |    5 |    8.3923 |    0.9460 |        - |    68.59 KB |        4.23 |
