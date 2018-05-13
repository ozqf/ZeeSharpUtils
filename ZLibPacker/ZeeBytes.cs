using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLibPacker
{
    public class ZeeBytes
    {
        public const int FLOAT_TO_INT_PRECISION = 1000;

        public static int Write(byte[] bytes, int position, byte val)
        {
            bytes[position] = val;
            position++;
            return position;
        }

        public static int Write(byte[] bytes, int position, short val)
        {
            bytes[position] = (byte)(val >> 0);
            bytes[position + 1] = (byte)(val >> 8);
            position += 2;
            return position;
        }

        public static int Write(byte[] bytes, int position, ushort val)
        {
            bytes[position] = (byte)(val >> 0);
            bytes[position + 1] = (byte)(val >> 8);
            position += 2;
            return position;
        }

        public static int Write(byte[] bytes, int position, int val)
        {
            bytes[position] = (byte)(val >> 0);
            bytes[position + 1] = (byte)(val >> 8);
            bytes[position + 2] = (byte)(val >> 16);
            bytes[position + 3] = (byte)(val >> 24);
            position += 4;
            return position;
        }

        public static int Write(byte[] bytes, int position, uint val)
        {
            bytes[position] = (byte)(val >> 0);
            bytes[position + 1] = (byte)(val >> 8);
            bytes[position + 2] = (byte)(val >> 16);
            bytes[position + 3] = (byte)(val >> 24);
            position += 4;
            return position;
        }

        public static int Write(byte[] bytes, int position, float val)
        {
            int integer = (int)(val * FLOAT_TO_INT_PRECISION);
            return Write(bytes, position, integer);
        }

        public static int Write(byte[] bytes, int position, char[] chars)
        {
            int len = chars.Length;
            position = Write(bytes, position, len);
            for (int i = 0; i < len; ++i)
            {
                position = Write(bytes, position, (ushort)chars[i]);
            }
            return position;
        }

        public static char[] Readchars(int position, byte[] bytes)
        {
            int len = ReadInt(bytes, ref position);
            char[] chars = new char[len];
            for (int i = 0; i < len; ++i)
            {
                chars[i] = (char)ReadUShort(bytes, ref position);
            }
            return chars;
        }

        public static byte ReadByte(byte[] bytes, ref int position)
        {
            byte result = bytes[position];
            position += 1;
            return result;
        }

        public static byte PeekByte(byte[] bytes, int position)
        {
            return bytes[position];
        }

        public static short ReadShort(byte[] bytes, ref int position)
        {
            short result = BitConverter.ToInt16(bytes, position);
            position += 2;
            return result;
        }

        public static short PeekShort(byte[] bytes, int position)
        {
            return BitConverter.ToInt16(bytes, position);
        }

        public static ushort ReadUShort(byte[] bytes, ref int position)
        {
            ushort result = BitConverter.ToUInt16(bytes, position);
            position += 2;
            return result;
        }

        public static ushort PeekUShort(byte[] bytes, int position)
        {
            return BitConverter.ToUInt16(bytes, position);
        }

        public static int ReadInt(byte[] bytes, ref int position)
        {
            int result = BitConverter.ToInt32(bytes, position);
            position += 4;
            return result;
        }

        public static int PeekInt(byte[] bytes, int position)
        {
            return BitConverter.ToInt32(bytes, position);
        }

        public static uint ReadUint(byte[] bytes, ref int position)
        {
            uint result = BitConverter.ToUInt32(bytes, position);
            position += 4;
            return result;
        }

        public static uint PeekUInt(byte[] bytes, int position)
        {
            return BitConverter.ToUInt32(bytes, position);
        }

        public static float ReadFloat(byte[] bytes, ref int position)
        {
            float result = (float)ReadInt(bytes, ref position) / FLOAT_TO_INT_PRECISION;
            return result;
        }

        public static string DebugDumpBytes(byte[] bytes, int readerPosition)
        {
            string str = string.Empty;
            int len = bytes.Length;
            for (int i = 0; i < len; ++i)
            {
                if (i == readerPosition)
                {
                    str += ">";
                }
                str += bytes[i].ToString("X2") + ", ";
            }
            return str;
        }
    }
}
