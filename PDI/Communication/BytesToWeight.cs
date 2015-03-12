using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Communication
{
    static class BytesToWeight
    {
        private const double A = 0.63393;
        private const double B = -21.507;

        public static double GetWeight(byte lo, byte hi)
        {
            return (lo + (hi << 8)) * A + B;
        }
        public static int GetBytes(int weight)
        {
            return (int)Math.Round((weight - B) / A);
        }
    }
}
