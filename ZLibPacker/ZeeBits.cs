using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLibPacker
{
    /// <summary>
    /// Simple Bit Mask operations
    /// </summary>
    public class ZeeBits
    {
        public static bool IncludesFlag(uint flags, uint query)
        {
            if ((flags & query) == query)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IncludesFlag(int flags, int query)
        {
            if ((flags & query) == query)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static uint AddFlag(uint flags, uint flagToAdd)
        {
            uint f = flags | flagToAdd;
            return f;
        }

        public static int AddFlag(int flags, int flagToAdd)
        {
            int f = flags | flagToAdd;
            return f;
        }

        public static uint RemoveFlag(uint flags, uint flagToRemove)
        {
            uint f = flags & ~flagToRemove;
            return f;
        }

        public static int RemoveFlag(int flags, int flagToRemove)
        {
            int f = flags & ~flagToRemove;
            return f;
        }

        public static bool Match(uint mask, uint query)
        {
            return (mask & query) != 0;
        }

        public static bool Match(int mask, int query)
        {
            return (mask & query) != 0;
        }
    }
}
