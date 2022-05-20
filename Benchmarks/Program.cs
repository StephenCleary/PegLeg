using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PegLeg.Runtime.Hashing;

BenchmarkRunner.Run(typeof(Benchmarks));

[MemoryDiagnoser]
public class Benchmarks
{
    private HashValue _hash1;
    private HashValue _hash2;

    public Benchmarks()
    {
        //var random = new Random();
        //Hasher.Shared.Append(random.Next());
        //_hash1 = Hasher.Shared.GetAndReset();
        //Hasher.Shared.Append(random.Next());
        //_hash2 = Hasher.Shared.GetAndReset();
    }

    [Benchmark]
    public bool AreEqual1()
    {
        return _hash1.Equals(_hash2);
    }

    //[Benchmark]
    //public bool AreEqual2()
    //{
    //    return _hash1.Equals2(_hash2);
    //}

    [Benchmark]
    public bool AreEqual1_WhenEqual()
    {
        return _hash1.Equals(_hash1);
    }

    //[Benchmark]
    //public bool AreEqual2_WhenEqual()
    //{
    //    return _hash1.Equals2(_hash1);
    //}
}