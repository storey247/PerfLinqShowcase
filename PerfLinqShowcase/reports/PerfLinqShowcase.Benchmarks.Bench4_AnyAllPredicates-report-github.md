```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD


```
| Method                                                                 | Scale  | Mean         | StdDev       | Error        | Ratio | RatioSD | Rank | Gen0     | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------------------------------------- |------- |-------------:|-------------:|-------------:|------:|--------:|-----:|---------:|-------:|----------:|------------:|
| &#39;LINQ: Where → ToList → Where(Any(All(…))) — spurious materialisation&#39; | Large  |  69,022.7 ns |    756.87 ns |    809.13 ns |  1.00 |    0.01 |    1 |   3.7842 | 0.1221 |   32728 B |    1,022.75 |
| &#39;Foreach: nested loops with early exit (baseline)&#39;                     | Large  |  69,093.9 ns |    269.00 ns |    287.57 ns |  1.00 |    0.01 |    1 |        - |      - |      32 B |        1.00 |
| &#39;LINQ: Where(Any(All(…))) — nested predicate chain&#39;                    | Large  |  83,315.6 ns |    662.02 ns |    746.80 ns |  1.21 |    0.01 |    2 |   0.3662 |      - |    3856 B |      120.50 |
| &#39;LINQ: new HashSet per employee inside Where — micro-allocation trap&#39;  | Large  | 555,053.7 ns | 18,411.57 ns | 11,019.77 ns |  8.03 |    0.26 |    3 | 133.7891 |      - | 1122816 B |   35,088.00 |
|                                                                        |        |              |              |              |       |         |      |          |        |           |             |
| &#39;Foreach: nested loops with early exit (baseline)&#39;                     | Medium |   6,869.0 ns |     25.88 ns |     29.20 ns |  1.00 |    0.01 |    1 |   0.0076 |      - |      88 B |        1.00 |
| &#39;LINQ: Where → ToList → Where(Any(All(…))) — spurious materialisation&#39; | Medium |   9,249.3 ns |     41.63 ns |     44.51 ns |  1.35 |    0.01 |    2 |   0.7782 | 0.0153 |    6576 B |       74.73 |
| &#39;LINQ: Where(Any(All(…))) — nested predicate chain&#39;                    | Medium |  10,596.4 ns |     33.96 ns |     38.31 ns |  1.54 |    0.01 |    3 |   0.1678 |      - |    1504 B |       17.09 |
| &#39;LINQ: new HashSet per employee inside Where — micro-allocation trap&#39;  | Medium |  58,826.8 ns |    415.94 ns |    498.11 ns |  8.56 |    0.07 |    4 |  25.0244 |      - |  209504 B |    2,380.73 |
|                                                                        |        |              |              |              |       |         |      |          |        |           |             |
| &#39;Foreach: nested loops with early exit (baseline)&#39;                     | Small  |     819.3 ns |      5.84 ns |      6.25 ns |  1.00 |    0.01 |    1 |   0.0105 |      - |      88 B |        1.00 |
| &#39;LINQ: Where → ToList → Where(Any(All(…))) — spurious materialisation&#39; | Small  |   1,458.8 ns |      7.07 ns |      8.46 ns |  1.78 |    0.01 |    2 |   0.1869 |      - |    1568 B |       17.82 |
| &#39;LINQ: Where(Any(All(…))) — nested predicate chain&#39;                    | Small  |   1,514.7 ns |      4.10 ns |      4.39 ns |  1.85 |    0.01 |    3 |   0.0916 |      - |     776 B |        8.82 |
| &#39;LINQ: new HashSet per employee inside Where — micro-allocation trap&#39;  | Small  |   9,461.1 ns |     89.17 ns |    106.78 ns | 11.55 |    0.13 |    4 |   4.5166 |      - |   37856 B |      430.18 |
