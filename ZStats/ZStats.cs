using System;
using System.Collections.Generic;

namespace ZStats
{
    public enum StatType
    {
        Float, Integer, Text, Ref
    }

    public class Stat
    {
        public StatType statType = StatType.Float;
        public float f;
        public int i;
        public string txt;
        // not sure how to do statset references yet!
        //public string statsPath;
    }

    public class StatSet
    {
        private Dictionary<string, Stat> _stats;


    }
}
