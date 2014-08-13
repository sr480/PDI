using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Model
{
    [Serializable]
    public class ExperimentLog
    {
        public int Cycle { get; set; }
        public double Position { get; set; }
        public ExperimentLog()
        { }
        public ExperimentLog(int cycle, double position)
        {
            Cycle = cycle;
            Position = position;    
        }
    }
}
