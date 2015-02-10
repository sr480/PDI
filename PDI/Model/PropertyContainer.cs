using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Model
{
    [Serializable]
    public class PropertyContainer
    {
        public string Name { get; set; }
        public int ExperimentFrequency { get; set; }
        public int ExperimentWeight { get; set; }
        public bool UseTermo { get; set; }
        public int ExperimentTemperature { get; set; }
        public int ExperimentDuration { get; set; }
        public PropertyContainer()
        {
            Name = "Новый набор";
            ExperimentFrequency = 10;
            ExperimentWeight = 80;
            UseTermo = false;
            ExperimentTemperature = 25;
            ExperimentDuration = 0;
        }
    }
}
