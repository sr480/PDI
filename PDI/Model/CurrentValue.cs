using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Model
{
    class CurrentValue
    {
        public double TimeSpan { get; private set; }
        public double Weight { get; private set; }
        public CurrentValue(double timeSpan, double weight)
        {
            TimeSpan = timeSpan;
            Weight = weight;
        }
    }
}
