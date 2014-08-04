using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Model
{
    public class ExperimentLog
    {
        public int Cycle { get; private set; }
        public double Position { get; private set; }
        public ExperimentLog(int cycle, double position)
        {
            Cycle = cycle;
            Position = position;    
        }
    }
}
