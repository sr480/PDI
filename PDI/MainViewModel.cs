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
        private int _Acceleration = 0;
        private int _Speed = 6250;
        private int _ExperimentDuration = 0;
        private int _ExperimentTemperature = 25;
        private int _ExperimentWeight = 200;
        private int _ExperimentFrequency = 25;
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        
        private string _State;
        private const int BAUD_RATE = 1000000;
        private readonly Tools.ViewableCollection<string> _availablePorts;
        private string _SelectedPort;
        private readonly Tools.ViewableCollection<Model.CurrentValue> _currentValues;
        private Communication.Port _port;
        private bool _isExperimentMode = false;

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
        public Tools.Command StartExperiment { get; private set; }
        public Tools.Command ApplyTuning { get; private set; }
        //Properties
        public Model.Logger Logger
        {
            get { return Model.Logger.LogInstance; }
        }

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


        public bool IsExperimentMode
        {
            get
            {
                return _isExperimentMode;
            }
            private set
            {
                if (_isExperimentMode == value)
                    return;
                _isExperimentMode = value;                
                RaisePropertyChanged("IsExperimentMode");
            }
        }
        public int ExperimentFrequency
        {
            get
            {
                return _ExperimentFrequency;
            }
            set
            {
                if (_ExperimentFrequency == value)
                    return;
                if (value < 5 | value > 25)
                    throw new Exception("Частота вне заданных пределов");
                
                _ExperimentFrequency = value;
                RaisePropertyChanged("ExperimentFrequency");
            }
        }
        public int ExperimentWeight
        {
            get
            {
                return _ExperimentWeight;
            }
            set
            {
                if (_ExperimentWeight == value)
                    return;
                if (value < 80 | value > 600)
                    throw new Exception("Вес вне заданных пределов");
                
                _ExperimentWeight = value;
                
                RaisePropertyChanged("ExperimentWeight");
            }
        }
        public int ExperimentTemperature
        {
            get
            {
                return _ExperimentTemperature;
            }
            set
            {
                if (_ExperimentTemperature == value)
                    return;
                if (value < 10 | value > 70)
                    throw new Exception("Температура вне заданных пределов");

                _ExperimentTemperature = value;
                RaisePropertyChanged("ExperimentTemperature");
            }
        }
        public int ExperimentDuration
        {
            get
            {
                return _ExperimentDuration;
            }
            set
            {
                if (_ExperimentDuration == value)
                    return;
                if (value < 0 | value > 16777215)
                    throw new Exception("Длительность эксперимента вне заданных пределов");
                
                _ExperimentDuration = value;
                RaisePropertyChanged("ExperimentDuration");
            }
        }

        //Tuning properties
        public int Speed
        {
            get { return _Speed; }
            set
            {
                if (_Speed == value)
                    return;
                if (value > 51000)
                    throw new Exception("Скорость не может быть больше 51000 мкс");
                _Speed = value;
                RaisePropertyChanged("Speed");
            }
        }

        public int Acceleration
        {
            get { return _Acceleration; }
            set
            {
                if (_Acceleration == value)
                    return;
                if (value > 51000)
                    throw new Exception("Ускорение не может быть больше 51000 мкс");
                _Acceleration = value;
                RaisePropertyChanged("Acceleration");
            }
        }
        

        public MainViewModel()
        {
            Connect_Disconnect = new Tools.Command(x => Connect_DisconnectAction(), x => !String.IsNullOrEmpty(SelectedPort));
            StartExperiment = new Tools.Command(x => StartExperimentAction(), x => _port != null);
            ApplyTuning = new Tools.Command(x => ApplyTunungAction(), x => _port != null);

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

            if (IsExperimentMode)
            {
                var cmd = new Communication.RequestExperimentStateCommand();
                cmd.RespondRecieved += ExperimentRespondRecieved;
                _port.SendCommand(cmd);
            }
            else
            {
                //var cmd = new Communication.RequestTemperatureCommand();
                //cmd.RespondRecieved += TemperatureRespondRecieved;
                //_port.SendCommand(cmd);
            }
        }

        void TemperatureRespondRecieved(object sender, Communication.TemperatureRecievedEventArgs e)
        {
            TD1 = string.Format("ТД1: {0} °C", e.TD1);
            TD2 = string.Format("ТД2: {0} °C", e.TD2);
            TD3 = string.Format("ТД3: {0} °C", e.TD3);
            TD4 = string.Format("ТД4: {0} °C", e.TD4);
        }

        void ExperimentRespondRecieved(object sender, Communication.ExperimentStateRecievedEventArgs e)
        {
            TD1 = string.Format("ТД1: {0} °C", e.TD1);
            TD2 = string.Format("ТД2: {0} °C", e.TD2);
            TD3 = string.Format("ТД3: {0} °C", e.TD3);
            TD4 = string.Format("ТД4: {0} °C", e.TD4);
            Position = string.Format("ПОЗ: {0} мм", e.Position);

            List<Model.CurrentValue> values = new List<Model.CurrentValue>(800);
            int div = 1;
            for (int i = 0; i < e.Tensos.Length; i += div)
                values.Add(new Model.CurrentValue(i * 1.25, e.Tensos[i]));

            CurrentValues.BulkFill(values);
        }

        private void StartExperimentAction()
        {
            IsExperimentMode = !IsExperimentMode;
            while (!_port.TransmitAvailable) ;
            _port.SendCommand(new Communication.StartExperimentRequest(ExperimentFrequency, ExperimentDuration, ExperimentWeight, ExperimentDuration));
        }
        private void ApplyTunungAction()
        {
            while (!_port.TransmitAvailable) ;
            _port.SendCommand(new Communication.TuningCommand(Speed, Acceleration));
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
            StartExperiment.RaiseCanExecuteChanged();
            ApplyTuning.RaiseCanExecuteChanged();
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
