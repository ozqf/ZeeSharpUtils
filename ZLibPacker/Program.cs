﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ZLibPacker
{
    class Program
    {
        class DiskArchiveHeader
        {
            public const int SIZE_IN_BYTES = 12;


            private static byte[] MAGIC = new byte[4] { (byte)'P', (byte)'A', (byte)'C', (byte)'K' };
            public byte[] magic { get { return MAGIC; } }

            public uint fileListOffset; // distance into byte stream of file list
            public uint numFiles;       // number of entries in file list
            public short version;
            public short flags;
        }

        class DiskArchiveFile
        {
            public const int SIZE_IN_BYTES = 64;


            public string fileName = string.Empty;      // max 51 + 0 to terminate
            public uint offset = 0;                     // distance into byte stream of file
            public uint numBytes = 0;

            public int Write(byte[] bytes, int position)
            {
                int charsWritten = 0;
                int fileNameLength = fileName.Length;
                for (int i = 0; i < fileNameLength; ++i)
                {
                    position = ZeeBytes.Write(bytes, position, (byte)fileName[i]);
                    charsWritten++;
                }
                
                // Pad with NULL terminator
                while (charsWritten < 52)
                {
                    position = ZeeBytes.Write(bytes, position, 0);
                    charsWritten++;

                }

                position = ZeeBytes.Write(bytes, position, offset);
                position = ZeeBytes.Write(bytes, position, numBytes);

                return position;
            }
        }

        static void Main(string[] args)
        {
            const int MAX_ARCHIVE_FILE_NAME_SIZE = 51;      // Maximum file size to keep headers under 64 bytes

            Console.WriteLine("*** ZLib Packer ***");
            string dir = "testInput";
            try
            {
                List<string> list = new List<string>(100);
                GetRecursiveFileList(dir, list);

                if (list.Count == 0)
                {
                    throw new Exception("No files found in directory '" + dir + "'");
                }

                int memSizeOfFileList = DiskArchiveFile.SIZE_IN_BYTES * list.Count;
                Console.WriteLine("Size of file table: " + memSizeOfFileList + " bytes, or " + list.Count + " * 64");

                // headers are stored in memory until write is finished, then appended to the end.
                List<DiskArchiveFile> fileHeaders = new List<DiskArchiveFile>(list.Count);
                foreach (string path in list)
                {
                    int properStart = path.IndexOf('\\');
                    string output;
                    if (properStart >= 0)
                    {
                        output = path.Remove(0, properStart);
                    }
                    else
                    {
                        output = path;
                    }

                    // validate filename
                    if (output.Length > MAX_ARCHIVE_FILE_NAME_SIZE)
                    {
                        throw new Exception("File path " + output + " is too long. Must be 51 chars or lower");
                    }
                    foreach(char c in output)
                    {
                        if (c > 255)
                        {
                            throw new Exception("File path " + output + " contains non-ascii characters");
                        }
                    }

                    DiskArchiveFile file = new DiskArchiveFile();
                    file.fileName = output;
                    file.numBytes = 0;
                    file.offset = 0;


                    
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Well something cocked up.");
                Console.WriteLine(ex.Message);
            }
            

            Console.WriteLine("\nDone");
            Console.ReadKey();
        }
        
        static void GetRecursiveFileList(string rootPath, List<string> results)
        {
            if (results == null) { throw new ArgumentNullException("GetRecursiveFileList results list is null"); }

            string[] files = Directory.GetFiles(rootPath, "*");
            int numFiles = files.Length;
            foreach (string filePath in files)
            {
                results.Add(filePath);
            }

            string[] directories = Directory.GetDirectories(rootPath);
            foreach (string dir in directories)
            {
                GetRecursiveFileList(dir, results);
            }
        }
    }
}