using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLibPacker
{
    public class ZeeParse
    {
        public static float ReadFloat(string input, float failureDefault)
        {
            float output;
            if (float.TryParse(input, out output) == true)
            {
                return output;
            }
            else { return failureDefault; }
        }

        public static int ReadInt(string input, int failureDefault)
        {
            int output;
            if (int.TryParse(input, out output) == true)
            {
                return output;
            }
            else { return failureDefault; }
        }

        public static uint ReadUInt(string input, uint failureDefault)
        {
            uint output;
            if (uint.TryParse(input, out output) == true)
            {
                return output;
            }
            else { return failureDefault; }
        }

        public static float ReadFloat(string input, float failureDefault, float min, float max)
        {
            float output;
            if (float.TryParse(input, out output))
            {
                return Clamp(output, min, max);
            }
            else
            {
                return failureDefault;
            }
        }

        public static int ReadInt(string input, int failureDefault, int min, int max)
        {
            int output;
            if (int.TryParse(input, out output))
            {
                return ClampInt(output, min, max);
            }
            else
            {
                return failureDefault;
            }
        }

        public static uint ReadUInt(string input, uint failureDefault, uint min, uint max)
        {
            uint output;
            if (uint.TryParse(input, out output))
            {
                return ClampUint(output, min, max);
            }
            else
            {
                return failureDefault;
            }
        }

        public static bool IsStringTruthy(string str)
        {
            if (str == null) { return false; }
            if (str == "t" || str == "T" || str == "true" || str == "True")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string CreateThreeDigitIndex(int i)
        {
            string str;
            if (i < 10)
            {
                str = "00" + i;
            }
            else if (i < 100)
            {
                str = "0" + i;
            }
            else
            {
                str = i.ToString();
            }
            return str;
        }

        public static string ClearNewLines(string str)
        {
            return str.Replace("\n", "");
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static int ClampInt(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static uint ClampUint(uint value, uint min, uint max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

    }
}
