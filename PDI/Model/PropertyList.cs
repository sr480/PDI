using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PDI.Model
{
    [Serializable]
    public class PropertyList
    {
        private const string FILEPATH = "properties.xml";
        public ObservableCollection<PropertyContainer> Properties { get; set; }

        public PropertyList()
        {
            Properties = new ObservableCollection<PropertyContainer>();
        }

        public static PropertyList OpenList()
        {
            var xmls = new System.Xml.Serialization.XmlSerializer(typeof(PropertyList));
            try
            {
                return (PropertyList)xmls.Deserialize(System.IO.File.OpenRead(FILEPATH));
            }
            catch (Exception err)
            {
                return new PropertyList();
            }
        }
        public void SaveList()
        {
            var xmls = new System.Xml.Serialization.XmlSerializer(typeof(PropertyList));
            try
            {

                using (System.IO.TextWriter tw = System.IO.File.CreateText(FILEPATH))
                {
                    xmls.Serialize(tw, this);
                    tw.Flush();
                }
            }
            catch (Exception err)
            {
                ;//System.Windows.Forms.MessageBox.Show(err.Message, "Ошибка сохранения", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
