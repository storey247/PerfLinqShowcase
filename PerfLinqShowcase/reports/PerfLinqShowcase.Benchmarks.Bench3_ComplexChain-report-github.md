```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD


```
| Method                                                       | EmployeeCount | Mean         | StdDev     | Error      | Ratio | RatioSD | Rank | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|------------------------------------------------------------- |-------------- |-------------:|-----------:|-----------:|------:|--------:|-----:|---------:|---------:|-----------:|------------:|
| &#39;Foreach: single pass per employee (baseline)&#39;               | 100           |     55.71 μs |   0.224 μs |   0.253 μs |  1.00 |    0.01 |    1 |   0.7324 |        - |    6.34 KB |        1.00 |
| &#39;LINQ: 4 sub-queries per employee (re-enumerating Projects)&#39; | 100           |    212.54 μs |   1.423 μs |   1.704 μs |  3.81 |    0.03 |    2 |  10.2539 |        - |   84.08 KB |       13.27 |
| &#39;LINQ: Count() then Select() — double enumeration of Where&#39;  | 100           |    214.08 μs |   2.797 μs |   3.155 μs |  3.84 |    0.05 |    2 |  10.2539 |        - |   84.08 KB |       13.27 |
|                                                              |               |              |            |            |       |         |      |          |          |            |             |
| &#39;Foreach: single pass per employee (baseline)&#39;               | 1000          |  1,022.08 μs |  22.739 μs |  19.746 μs |  1.00 |    0.03 |    1 |   5.8594 |        - |   60.43 KB |        1.00 |
| &#39;LINQ: Count() then Select() — double enumeration of Where&#39;  | 1000          |  2,535.23 μs |  19.245 μs |  21.709 μs |  2.48 |    0.06 |    2 |  97.6563 |   7.8125 |  801.78 KB |       13.27 |
| &#39;LINQ: 4 sub-queries per employee (re-enumerating Projects)&#39; | 1000          |  2,552.28 μs |  19.489 μs |  21.985 μs |  2.50 |    0.06 |    2 |  97.6563 |   7.8125 |  801.79 KB |       13.27 |
|                                                              |               |              |            |            |       |         |      |          |          |            |             |
| &#39;Foreach: single pass per employee (baseline)&#39;               | 10000         | 12,979.05 μs |  91.041 μs |  97.328 μs |  1.00 |    0.01 |    1 |  62.5000 |  15.6250 |  609.37 KB |        1.00 |
| &#39;LINQ: 4 sub-queries per employee (re-enumerating Projects)&#39; | 10000         | 26,316.18 μs | 128.749 μs | 145.237 μs |  2.03 |    0.02 |    2 | 968.7500 | 156.2500 | 8100.85 KB |       13.29 |
| &#39;LINQ: Count() then Select() — double enumeration of Where&#39;  | 10000         | 26,321.99 μs |  94.625 μs | 101.159 μs |  2.03 |    0.02 |    2 | 968.7500 | 156.2500 | 8100.81 KB |       13.29 |
