using System;
using ZqfInfluenceMap;
using System.Diagnostics;

namespace ZeeSharpUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test Influence map");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string result = InfluenceMap.Test();
            sw.Stop();
            Console.WriteLine(result);
            Console.WriteLine($"Took ${sw.ElapsedMilliseconds / 1000f} seconds\n");
        }
    }
}
