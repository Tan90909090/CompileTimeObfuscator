using System;
using Xunit;

namespace CompileTimeObfuscator.Tests.TestUtils;

public static class MatrixTheoryData
{
    public static MatrixTheoryData<T1, T2> Create<T1, T2>(T1[] data1, T2[] data2) => new(data1, data2);
}

public class MatrixTheoryData<T1, T2> : TheoryData<T1, T2>
{
    public MatrixTheoryData(ReadOnlySpan<T1> data1, ReadOnlySpan<T2> data2)
    {
        foreach (var v1 in data1)
        {
            foreach (var v2 in data2)
            {
                this.Add(v1, v2);
            }
        }
    }
}
