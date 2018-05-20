using System;
using System.Collections.Generic;
using System.IO;

namespace ZLibPacker
{
    class DiskArchiveHeader
    {
        public const int SIZE_IN_BYTES = 12;


        private static byte[] MAGIC = new byte[4] { (byte)'P', (byte)'A', (byte)'C', (byte)'K' };
        public byte[] magic { get { return MAGIC; } }

        public uint fileListOffset; // distance into byte stream of file list
        public uint numFiles;       // number of entries in file list
        
        public long Write(BinaryWriter writer)
        {
            writer.Write(magic[0]);
            writer.Write(magic[1]);
            writer.Write(magic[2]);
            writer.Write(magic[3]);

            writer.Write(fileListOffset);
            writer.Write(numFiles);
            return writer.BaseStream.Position;
        }
    }

    class DiskArchiveFile
    {
        public const int SIZE_IN_BYTES = 64;
        public const int NAME_CHAR_ARRAY_SIZE = 52; // max 51 + 0 to terminate
        public const int NAME_CHAR_ARRAY_MAX_INPUT_SIZE = 51;


        public string fullFilePath = string.Empty;  // for internal use only. not saved into archive

        // length limited name saved into archive
        char[] _fileNameArray = new char[NAME_CHAR_ARRAY_SIZE];
        public string fileNameArray
        {
            get
            {
                return new string(_fileNameArray);
            }
        }

        // distance into byte stream of file
        public uint offset = 0;
        public uint numBytes = 0;
        public byte[] info = new byte[4];

        public void SetFileName(string str)
        {
            fullFilePath = str;
            if (fullFilePath.Length > NAME_CHAR_ARRAY_MAX_INPUT_SIZE) { throw new Exception("File Name" + str + " is too long. It must be under " + NAME_CHAR_ARRAY_MAX_INPUT_SIZE + " characters"); }

            int numCharsWritten = 0;
            // skip root directory name
            int readIndex = fullFilePath.IndexOf("\\");
            int nameCharsToWrite = fullFilePath.Length - readIndex;

            while (numCharsWritten < NAME_CHAR_ARRAY_SIZE)
            {
                if (nameCharsToWrite > 0)
                {
                    _fileNameArray[numCharsWritten] = fullFilePath[readIndex];
                    nameCharsToWrite--;
                    readIndex++;
                }
                else
                {
                    _fileNameArray[numCharsWritten] = (char)0;
                }
                numCharsWritten++;
            }
        }

        public uint Write(BinaryWriter writer)
        {
            info[0] = (byte)ZLibPacker.GetFileType(fullFilePath);


            Console.WriteLine("'" + fileNameArray + "' type: " + info[0] + " offset: " + offset + " size: " + numBytes);
            for (int i = 0; i < _fileNameArray.Length; ++i)
            {
                //byte val = (byte)_fileNameArray[i];
                //Console.Write()
                writer.Write((byte)_fileNameArray[i]);
            }
            writer.Write(offset);
            writer.Write(numBytes);

            writer.Write(info[0]);
            writer.Write(info[1]);
            writer.Write(info[2]);
            writer.Write(info[3]);

            return (uint)writer.BaseStream.Position;
        }

        //public int Write(byte[] bytes, int position)
        //{
        //    int charsWritten = 0;
        //    int fileNameLength = fullFilePath.Length;
        //    for (int i = 0; i < fileNameLength; ++i)
        //    {
        //        position = ZeeBytes.Write(bytes, position, (byte)fullFilePath[i]);
        //        charsWritten++;
        //    }

        //    // Pad with NULL terminator
        //    while (charsWritten <= NAME_CHAR_ARRAY_SIZE)
        //    {
        //        position = ZeeBytes.Write(bytes, position, 0);
        //        charsWritten++;

        //    }

        //    position = ZeeBytes.Write(bytes, position, offset);
        //    position = ZeeBytes.Write(bytes, position, numBytes);

        //    return position;
        //}
    }

    enum ZPackerFileType : byte
    {
        Unknown = 0,
        BMP = 1,
        txt = 2,
        wav = 3
    };

    class ZLibPacker
    {
        public const int MAX_ARCHIVE_FILE_NAME_SIZE = 51;      // Maximum file size to keep headers under 64 bytes

        public static long GetFileSize(string path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.Length;
        }

        public static ZPackerFileType GetFileType(string path)
        {
            string extension = Path.GetExtension(path);
            switch (extension.ToLower())
            {
                case ".wav":
                    return ZPackerFileType.wav;
                case ".bmp":
                    return ZPackerFileType.BMP;
                case ".txt":
                    return ZPackerFileType.txt;
                default:
                    return ZPackerFileType.Unknown;
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("*** ZLib Packer ***");
            
            if (args.Length == 0)
            {
                Console.WriteLine("Specify input directory, eg ZLibPacker folderName\n");
                Console.WriteLine("\nDone");
                Console.ReadKey();
                return;
            }

            string dir = args[0];
            
            string outputFile = dir + ".dat";
            try
            {
                List<string> list = new List<string>(100);
                GetRecursiveFileList(dir, list);

                if (list.Count == 0)
                {
                    throw new Exception("No files found in directory '" + dir + "'");
                }

                int memSizeOfFileList = DiskArchiveFile.SIZE_IN_BYTES * list.Count;
                Console.WriteLine("Size of file table: " + memSizeOfFileList + " bytes, or 64 * " + list.Count + " files");

                //////////////////////////////////////////////////////////////////////
                // Build manifest
                // headers are stored in memory until write is finished, then appended to the end.
                //////////////////////////////////////////////////////////////////////
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
                    foreach (char c in output)
                    {
                        if (c > 255)
                        {
                            throw new Exception("File path " + output + " contains non-ascii characters");
                        }
                    }

                    DiskArchiveFile file = new DiskArchiveFile();
                    file.SetFileName(path);
                    file.numBytes = (uint)GetFileSize(path);
                    file.offset = 0;
                    fileHeaders.Add(file);
                }


                //////////////////////////////////////////////////////////////////////
                // Write files
                //////////////////////////////////////////////////////////////////////
                // open streams
                FileStream stream = new FileStream(outputFile, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(stream);

                DiskArchiveHeader header = new DiskArchiveHeader();
                header.numFiles = (uint)fileHeaders.Count;
                writer.Seek(DiskArchiveHeader.SIZE_IN_BYTES, SeekOrigin.Begin);

                Console.WriteLine("Writing files:");
                // Build the actual file, updating the manifest
                foreach (DiskArchiveFile file in fileHeaders)
                {
                    Console.WriteLine("Writing " + file.fullFilePath + " at " + writer.BaseStream.Position);
                    file.offset = (uint)writer.BaseStream.Position;
                    file.numBytes = (uint)GetFileSize(file.fullFilePath);
                    writer.Write(File.ReadAllBytes(file.fullFilePath));
                }

                Console.WriteLine("Writing manifest:");

                // Note the position of the file table in the header
                header.fileListOffset = (uint)writer.BaseStream.Position;

                // Write the manifest table
                foreach (DiskArchiveFile file in fileHeaders)
                {
                    file.Write(writer);
                }

                // Write header now that manifest is completed
                writer.Seek(0, SeekOrigin.Begin);
                header.Write(writer);

                // cleanup
                writer.Close();
                stream.Close();

                long outputSize = GetFileSize(outputFile);
                string sizeStr;
                if (outputSize >= (1024 * 1024))
                {
                    sizeStr = (outputSize / (1024 * 1024)).ToString() + " MB (" + outputSize + " bytes)";
                }
                else if (outputSize > 1024)
                {
                    sizeStr = (outputSize / 1024).ToString() + " KB (" + outputSize + " bytes)";
                }
                else
                {
                    sizeStr = outputSize.ToString() + " B";
                }
                Console.Write("Wrote \"" + outputFile + "\": " + sizeStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Well, something cocked up...");
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
