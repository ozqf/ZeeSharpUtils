using System;
using System.Linq;
using ZqfInfluenceMap;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace ZeeSharpUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test Influence map");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            IInfluenceMap map = InfluenceMap.Test();
            string result = map.DebugPrint();
            sw.Stop();
            Console.WriteLine(result);
            Console.WriteLine($"Took {sw.ElapsedMilliseconds / 1000f} seconds\n");

            float[] mapValues = map.GetValuesCopy();
            int len = map.width * map.height;

            // find highest and lowest values and normalise
            float min = mapValues.Min();
            float max = mapValues.Max();
            for (int i = 0; i < len; ++i)
            {
                // mapValues[i] = ...?;
            }
            
            using (Bitmap b = new Bitmap(map.width, map.height))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    for (int y = 0; y < map.height; ++y)
                    {
                        for (int x = 0; x < map.width; ++x)
                        {
                            int i = GridUtilities.GridPosToIndex(x, y, map.width);
                            float val = mapValues[i];
                            if (val > 1) { val = 1; }
                            if (val < 0) { val = 0; }
                            Color c = new Color();
                            // convert float in min/max range to 0-255
                            // Color.FromArgb(255, 0, 0);
                            byte r = (byte)(255f * val);
                            Console.Write($"{r}, ");
                            Color.FromArgb(255, r, 0, 0);
                            
                            // this doesn't work..? need to use Graphics class.. somehow?
                            b.SetPixel(x, y, c);
                        }
                    }
                    Console.Write($"\n");

                }
                b.Save(@"D:\test\influence_map.png", ImageFormat.Png);
            }
            Console.WriteLine($"Map is {map.width} by {map.height}");
            Console.ReadKey();
        }
    }
}
