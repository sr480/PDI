using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDI
{
    class MainViewModel : INotifyPropertyChanged
    {
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        private string _State;
        private const int BAUD_RATE = 9600;
        private readonly Tools.ViewableCollection<string> _availablePorts;
        private string _SelectedPort;
        private readonly Tools.ViewableCollection<Model.CurrentValue> _currentValues;
        private Communication.Port _port;

        private System.Timers.Timer _dataRequestTimer;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public Tools.ViewableCollection<Model.CurrentValue> CurrentValues { get { return _currentValues; } }
        public Tools.ViewableCollection<string> AvailablePorts { get { return _availablePorts; } }
        public Tools.Command Connect_Disconnect { get; private set; }
        public string SelectedPort
        {
            get
            {
                return _SelectedPort;
            }
            set
            {
                if (_SelectedPort == value)
                    return;
                _SelectedPort = value;
                OnPropertyChanged("SelectedPort");
                Connect_Disconnect.RaiseCanExecuteChanged();
            }
        }
        public string State
        {
            get
            {
                return _State;
            }
            private set
            {
                if (_State == value)
                    return;
                _State = value;
                OnPropertyChanged("State");
            }
        }
        
        public MainViewModel()
        {
            _availablePorts = new Tools.ViewableCollection<string>();
            _availablePorts.Fill(System.IO.Ports.SerialPort.GetPortNames());
            SelectedPort = AvailablePorts.FirstOrDefault();
            
            _currentValues = new Tools.ViewableCollection<Model.CurrentValue>();
            
            _dataRequestTimer = new System.Timers.Timer(1000);
            _dataRequestTimer.Elapsed += _dataRequestTimer_Elapsed;
            _dataRequestTimer.Start();
            
            Connect_Disconnect = new Tools.Command(x => Connect_DisconnectAction(), x => !String.IsNullOrEmpty(SelectedPort));
        }

        void _dataRequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_port == null)
                return;
        }

        private void Connect_DisconnectAction()
        {
            if(_port != null)
            {
                _port.Dispose();
                _port = null;
                State = "нет соединения";
            }
            else
            {
                _port = new Communication.Port(SelectedPort, BAUD_RATE);
                State = "подключение установлено";
            }
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
