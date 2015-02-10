using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDI
{
    class ExperimentPropertiesViewModel 
    {
        private readonly ExperimentProperties _View;
        public Tools.Command Ok { get; private set; }
        public Tools.Command Cancel { get; private set; }

        public Model.PropertyContainer Properties { get; private set; }

        private bool _dlgResult = false;


        public ExperimentPropertiesViewModel()
        {
            Properties = new Model.PropertyContainer();
            Ok = new Tools.Command((x) => OkAction(), (x) => true);
            Cancel = new Tools.Command((x) => CancelAction(), (x) => true);

            _View = new ExperimentProperties();
            _View.DataContext = this;
        }
        public bool Show()
        {
            _View.ShowDialog();
            return _dlgResult;
        }

        private void OkAction()
        {
            _dlgResult = true;
            _View.Close();
        }
        private void CancelAction()
        {
            _View.Close();
        }
    }
}
