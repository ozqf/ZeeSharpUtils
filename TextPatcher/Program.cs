using System;
using System.IO;
using System.Collections.Generic;

namespace TextPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Text Patcher - please specify input and output file names, eg:");
                Console.WriteLine("TextPatcher.exe inputFile.txt outputFile.txt");
                return;
            }
            Console.WriteLine("Text Patcher: input file: " + args[0] + " output file: " + args[1]);
            List<string> results = new List<string>();
            StreamReader reader = null;
            try
            {
                reader = File.OpenText(args[0]);
                string line = null;
                int lineCount = 0;
                int duplicateCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCount++;
                    if (!results.Contains(line))
                    {
                        results.Add(line);
                    }
                    else
                    {
                        duplicateCount++;
                    }
                }
                reader.Close();
                Console.WriteLine("Read " + lineCount + " lines, with " + results.Count + " unique and " + duplicateCount + " duplicates");

                TextWriter writer = new StreamWriter(args[1]);
                foreach (string s in results)
                {
                    writer.WriteLine(s);
                }
                writer.Close();
                Console.WriteLine("Done");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Well something cocked up: ", ex.Message);
                return;
            }
        }
    }
}
