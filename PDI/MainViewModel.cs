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
        private const int BAUD_RATE = 115200;
        private readonly Tools.ViewableCollection<string> _availablePorts;
        private string _SelectedPort;
        private readonly Tools.ViewableCollection<Model.CurrentValue> _currentValues;
        private Communication.Port _port;

        private System.Timers.Timer _dataRequestTimer;
        private string _td1 = "ТД1: н/д";
        private string _td2 = "ТД2: н/д";
        private string _td3 = "ТД3: н/д";
        private string _td4 = "ТД4: н/д";
        private string _position = "ПОЗ: н/д";

        public event PropertyChangedEventHandler PropertyChanged;
        //Collections
        public Tools.ViewableCollection<Model.CurrentValue> CurrentValues { get { return _currentValues; } }
        public Tools.ViewableCollection<string> AvailablePorts { get { return _availablePorts; } }
        //Commands
        public Tools.Command Connect_Disconnect { get; private set; }
        //Properties
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
        public string TD1
        {
            get { return _td1; }
            set
            {
                if (_td1 == value)
                    return;
                _td1 = value;
                RaisePropertyChanged("TD1");
            }
        }
        public string TD2
        {
            get { return _td2; }
            set
            {
                if (_td2 == value)
                    return;
                _td2 = value;
                RaisePropertyChanged("TD2");
            }
        }
        public string TD3
        {
            get { return _td3; }
            set
            {
                if (_td3 == value)
                    return;
                _td3 = value;
                RaisePropertyChanged("TD3");
            }
        }
        public string TD4
        {
            get { return _td4; }
            set
            {
                if (_td4 == value)
                    return;
                _td4 = value;
                RaisePropertyChanged("TD4");
            }
        }
        public string Position
        {
            get { return _position; }
            set
            {
                if (_position == value)
                    return;
                _position = value;
                RaisePropertyChanged("Position");
            }
        }
        
        public MainViewModel()
        {
            Connect_Disconnect = new Tools.Command(x => Connect_DisconnectAction(), x => !String.IsNullOrEmpty(SelectedPort));

            _availablePorts = new Tools.ViewableCollection<string>();
            _availablePorts.Fill(System.IO.Ports.SerialPort.GetPortNames());
            SelectedPort = AvailablePorts.FirstOrDefault();

            _currentValues = new Tools.ViewableCollection<Model.CurrentValue>();

            _dataRequestTimer = new System.Timers.Timer(1000);
            _dataRequestTimer.Elapsed += _dataRequestTimer_Elapsed;
            _dataRequestTimer.Start();


        }

        void _dataRequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_port == null)
                return;

            if (!_port.TransmitAvailable)
                return;

            var cmd = new Communication.RequestExperimentStateCommand();
            cmd.RespondRecieved += ExperimentRespondRecieved;
            _port.SendCommand(cmd);

            //_port = null;
        }

        void ExperimentRespondRecieved(object sender, Communication.ExperimentStateRecievedEventArgs e)
        {
            TD1 = string.Format("ТД1: {0} °C", e.TD1);
            TD2 = string.Format("ТД1: {0} °C", e.TD2);
            TD3 = string.Format("ТД1: {0} °C", e.TD3);
            TD4 = string.Format("ТД1: {0} °C", e.TD4);
            Position = string.Format("ПОЗ: {0} мм", e.Position);

            List<Model.CurrentValue> values = new List<Model.CurrentValue>();
            int div = 1;
            for (int i = 0; i < e.Tensos.Length; i += div)
                values.Add(new Model.CurrentValue(i * 1.25, e.Tensos[i]));

            CurrentValues.BulkFill(values);
        }

        private void Connect_DisconnectAction()
        {
            if (_port != null)
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
