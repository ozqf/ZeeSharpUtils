using System;
using System.Runtime.CompilerServices;

namespace ZqfInfluenceMap
{
    public struct Point
    {
        public int x;
        public int y;
    }

    public struct Rect
    {
        public int centreX;
        public int centreY;
        public int halfWidth;
        public int halfHeight;
    }

    public static class GridUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GridPosToIndex(int x, int y, int width)
        {
            return x + (y * width);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGridPosSafe(int x, int y, int width, int height)
        {
            if (x < 0 || x >= width) { return false; }
            if (y < 0 || y >= height) { return false; }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(int x0, int y0, int x1, int y1)
        {
            float vx = x1 - x0;
            float vy = y1 - y0;
            return (vx * vx) + (vy * vy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomRangef(float seed, float min, float max)
        {
            return seed * (max - min) + min;
        }

        public static float RandomRangef(this Random rand, float min, float max)
        {
            return (float)rand.NextDouble() * (max - min) + min;
        }
    }
}
