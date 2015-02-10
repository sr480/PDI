using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Communication
{
    static class BytesToWeight
    {
        private const double WEIGHT_CONV = 0.58;
        public static double GetWeight(byte lo, byte hi)
        {
            return (lo + (hi << 8)) * WEIGHT_CONV;
        }
        public static int GetBytes(int weight)
        {
            return (int)Math.Round(weight / WEIGHT_CONV);
        }
    }
}
