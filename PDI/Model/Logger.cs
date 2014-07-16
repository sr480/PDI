using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDI.Model
{
    public class Logger : INotifyPropertyChanged
    {
        private static readonly Logger _logInstance = new Logger();

        private string _log = String.Empty;
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        public event PropertyChangedEventHandler PropertyChanged;

        public static Logger LogInstance
        {
            get { return _logInstance; }
        }

        public string Log
        {
            get { return _log; }
        }

        public void Write(string log)
        {
            //_log = log + _log;
            //OnPropertyChanged("Log");
        }
        public void WriteLine(string log)
        {
            //_log = log + "\n" + _log;
            //OnPropertyChanged("Log");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (SynchronizationContext.Current != _synchronizationContext)
                RaisePropertyChanged(propertyName);
            else
                _synchronizationContext.Post(RaisePropertyChanged, propertyName);
        }
        private void RaisePropertyChanged(object param)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs((string)param));
        }
    }
}
