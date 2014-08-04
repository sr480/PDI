using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public class BytesToTemperature
    {
        public static double GetTemperature(byte hiBt, byte loBt)
        {
            return ((hiBt << 8) + loBt) * 0.0625;
        }
    }
}
