using PDI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI.Tools
{
    public class ExperimentExporter
    {
        string _path;
        IEnumerable<ExperimentLog> _log;

        public ExperimentExporter(string path, IEnumerable<ExperimentLog> log)
        {
            _path = path;
            _log = log;
        }

        public void Export()
        {
            try
            {
                using (System.IO.StreamWriter wr = System.IO.File.CreateText(_path))
                {
                    wr.WriteLine("Cycle; Position(mm)");
                    foreach (var item in _log)
                        wr.WriteLine(string.Format("{0}; {1}", item.Cycle, item.Position));
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Ошибка экспорта", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
