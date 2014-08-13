using PDI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Tools
{
    [Serializable]
    public class ExperimentSerializer
    {
        public static void Save(string path, ExperimentSerializer exp)
        {
            var xmls = new System.Xml.Serialization.XmlSerializer(typeof(ExperimentSerializer));
            try
            {

                using (System.IO.TextWriter tw = System.IO.File.CreateText(path))
                {
                    xmls.Serialize(tw, exp);
                    tw.Flush();
                }
            }
            catch (Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message, "Ошибка сохранения", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        public static ExperimentSerializer Open(string path)
        {
            var xmls = new System.Xml.Serialization.XmlSerializer(typeof(ExperimentSerializer));
            try
            {
                return (ExperimentSerializer)xmls.Deserialize(System.IO.File.OpenRead(path));
            }
            catch(Exception err)
            {
                System.Windows.Forms.MessageBox.Show(err.Message, "Ошибка открытия", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return null;
        }

        public ExperimentLog[] ExperimentValues { get; set; }
        public int Frequency { get; set; }
        public int Weight { get; set; }
        public int Temperature { get; set; }

        public ExperimentSerializer()
        { }

        public ExperimentSerializer(int freq, int weight, int temperature, List<ExperimentLog> experimentValues)
        {
            Frequency = freq;
            Weight = weight;
            Temperature = temperature;
            ExperimentValues = experimentValues.ToArray();
        }

    }
}
