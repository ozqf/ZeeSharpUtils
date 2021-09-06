using System.Text;
using System;

namespace ZqfInfluenceMap
{
    public class InfluenceMap : IInfluenceMap
    {
        #region static
        public static IInfluenceMap Create(int width, int height)
        {
            var result = new InfluenceMap(width, height);
            return result;
        }
		
		public static string Test()
		{
            int testWidth = 16;
            int testHeight = 24;
            int agentCount = 1;
			IInfluenceMap map = InfluenceMap.Create(testWidth, testHeight);

            Random rand = new Random(0);
            Rect r;
            for (int i = 0; i < agentCount; ++i)
            {
                r.centreX = (int)rand.RandomRangef(0, testWidth);
                r.centreY = (int)rand.RandomRangef(0, testHeight);
                r.halfWidth = (int)rand.RandomRangef(2, 6);
                r.halfHeight = (int)rand.RandomRangef(2, 6);
                map.QueueBlit(r, 1);
            }
            
            map.QueueQueryForBestZero(10, 8);
            map.QueueSetPixel(1, 1, 9.99f);

            return map.DebugPrint();
		}

        #endregion

        #region members

        private int _width;
        private int _height;
        private float[] _map;

        #endregion

        private InfluenceMap(int width, int height)
        {
            _width = width;
            _height = height;
            _map = new float[width * height];
        }

        private void BlitSolidRect(Rect r, float val)
        {
            Point min = new Point { x = r.centreX - r.halfWidth, y = r.centreY - r.halfHeight };
            Point max = new Point { x = r.centreX + r.halfWidth, y = r.centreY + r.halfHeight };
            
            for (int y = min.y; y < max.y; ++y)
            {
                for (int x = min.x; x < max.x; ++x)
                {
                    if (!GridUtilities.IsGridPosSafe(x, y, _width, _height)) { continue; }
                    _map[GridUtilities.GridPosToIndex(x, y, _width)] += val;
                }
            }
        }

        private void BlitFadedCircle(Rect r, float val)
        {
            r.halfHeight = r.halfWidth;
            Point min = new Point { x = r.centreX - r.halfWidth, y = r.centreY - r.halfHeight };
            Point max = new Point { x = r.centreX + r.halfWidth, y = r.centreY + r.halfHeight };
            float maxDistSqr = r.halfWidth * r.halfWidth;
            for (int y = min.y; y < max.y; ++y)
            {
                for (int x = min.x; x < max.x; ++x)
                {
                    if (!GridUtilities.IsGridPosSafe(x, y, _width, _height)) { continue; }
                    float distSqr = GridUtilities.DistanceSqr(r.centreX, r.centreY, x, y);
                    float scale = distSqr / maxDistSqr;
                    float addition = val * scale;
                    _map[GridUtilities.GridPosToIndex(x, y, _width)] += addition;
                }
            }
        }

        #region IInfluenceMap

        public int QueueBlit(Rect r, float val)
        {
            //BlitSolidRect(r, val);
            BlitFadedCircle(r, val);
            return 0;
        }

        public int QueueSetPixel(int x, int y, float val)
        {
            if (!GridUtilities.IsGridPosSafe(x, y, _width, _height)) { return 0; }
            _map[GridUtilities.GridPosToIndex(x, y, _width)] = val;
            return 0;
        }

        public int QueueQueryForBestZero(int originX, int originY)
        {
            Point best = new Point { x = originX, y = originY };
            float bestDistSqr = 9999;

            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    if (x == originX && y == originY) { continue; }
                    float queryDistSqr = GridUtilities.DistanceSqr(originX, originY, x, y);
                    if (queryDistSqr < bestDistSqr)
                    {
                        best.x = x;
                        best.y = y;
                        bestDistSqr = queryDistSqr;
                    }
                }
            }
            QueueSetPixel(best.x, best.y, 5.55f);
            return 0;
        }

        public void Process()
        {
            throw new System.NotImplementedException();
        }

        public string DebugPrint()
        {
            StringBuilder sb = new StringBuilder(2048);
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    int i = GridUtilities.GridPosToIndex(x, y, _width);
                    float val = _map[i];
                    if (val != 0)
                    {
                        sb.Append(val.ToString("0.00"));
                    }
                    else
                    {
                        sb.Append("    ");
                    }
                    sb.Append(", ");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        #endregion
    }
}
