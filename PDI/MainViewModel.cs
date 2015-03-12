using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PDI
{
    class MainViewModel : INotifyPropertyChanged
    {
        private Model.PropertyContainer _CurrentPropertyContainer;
        private bool _UseTermo;
        private const string TEMPORAL_FILENAME = "temp.expml";

        private static readonly Model.StateText _READY = new Model.StateText() { Text = "Готовность", Color = System.Windows.Media.Brushes.LimeGreen };
        private static readonly Model.StateText _PREPARING = new Model.StateText() { Text = "Подготовка", Color = System.Windows.Media.Brushes.Blue };
        private static readonly Model.StateText _RUNNING = new Model.StateText() { Text = "Идет испытание", Color = System.Windows.Media.Brushes.Orange };
        private static readonly Model.StateText _SERVOERROR = new Model.StateText() { Text = "Ошибка серводвигателя", Color = System.Windows.Media.Brushes.Red };
        private static readonly Model.StateText _STEPPERERROR = new Model.StateText() { Text = "Ошибка ШД", Color = System.Windows.Media.Brushes.Red };
        private static readonly Model.StateText _NOCONNECTION = new Model.StateText() { Text = "Нет подключения", Color = System.Windows.Media.Brushes.Gray };

        private object locker = new object();

        private int _LastCycles = 0;

        private Model.StateText _CurrentState = _NOCONNECTION;

        private bool _StopUpdate;
        private int _ExperimentDuration = 0;
        private int _ExperimentTemperature = 25;
        private int _ExperimentWeight = 200;
        private int _ExperimentFrequency = 15;
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        private string _State;
        private const int BAUD_RATE = 1000000;
        private readonly Tools.ViewableCollection<string> _availablePorts;
        private string _SelectedPort;
        private readonly Tools.ViewableCollection<Model.CurrentValue> _currentValues;

        private readonly List<Model.ExperimentLog> _experimentActualLog;
        private readonly Tools.ViewableCollection<Model.ExperimentLog> _experimentValues;


        private Communication.Port _port;
        private bool _isExperimentMode = false;

        private System.Timers.Timer _dataRequestTimer;
        private string _td1 = "ТД1: н/д";
        private string _td2 = "ТД2: н/д";
        private string _td3 = "ТД3: н/д";
        private string _delta = "ДЕФ: н/д";
        private string _position = "ПОЗ: н/д";
        private string _Cycles = "ЦКЛ: н/д";
        private string _Elapses = "ОСТ: н/д";

        public event PropertyChangedEventHandler PropertyChanged;
        //Collections
        public Tools.ViewableCollection<Model.CurrentValue> CurrentValues { get { return _currentValues; } }
        public Tools.ViewableCollection<Model.ExperimentLog> ExperimentValues { get { return _experimentValues; } }
        public Tools.ViewableCollection<string> AvailablePorts { get { return _availablePorts; } }
        //Commands
        public Tools.Command Connect_Disconnect { get; private set; }
        public Tools.Command StartExperiment { get; private set; }
        public Tools.Command SaveExperiment { get; private set; }
        public Tools.Command OpenExperiment { get; private set; }
        public Tools.Command Export { get; private set; }
        public Tools.Command AddPropertyContainer { get; private set; }
        public Tools.Command DeletePropertyContainer { get; private set; }

        //Properties
        public Model.PropertyList Properties { get; private set; }
        public Model.PropertyContainer CurrentPropertyContainer
        {
            get
            {
                return _CurrentPropertyContainer;
            }
            set
            {
                if (_CurrentPropertyContainer == value)
                    return;
                _CurrentPropertyContainer = value;
                OnPropertyChanged("CurrentPropertyContainer");
                ReadProperties();
            }
        }

        public Model.Logger Logger
        {
            get { return Model.Logger.LogInstance; }
        }
        public bool StopUpdate
        {
            get
            {
                return _StopUpdate;
            }
            set
            {
                if (_StopUpdate == value)
                    return;
                _StopUpdate = value;
                RaisePropertyChanged("StopUpdate");
            }
        }
        public Model.StateText CurrentState
        {
            get
            {
                return _CurrentState;
            }
            set
            {
                if (_CurrentState == value)
                    return;
                _CurrentState = value;
                RaisePropertyChanged("CurrentState");
            }
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
        public string Delta
        {
            get { return _delta; }
            set
            {
                if (_delta == value)
                    return;
                _delta = value;
                RaisePropertyChanged("Delta");
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
        public string Cycles
        {
            get
            {
                return _Cycles;
            }
            set
            {
                if (_Cycles == value)
                    return;
                _Cycles = value;
                RaisePropertyChanged("Cycles");
            }
        }
        public string Elapsed
        {
            get
            {
                return _Elapses;
            }
            set
            {
                if (_Elapses == value)
                    return;
                _Elapses = value;
                RaisePropertyChanged("Elapsed");
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
                if (_isExperimentMode)
                    CurrentState = _RUNNING;
                else
                    CurrentState = _READY;
                RaisePropertyChanged("IsExperimentMode");
                OpenExperiment.RaiseCanExecuteChanged();
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
        public bool UseTermo
        {
            get
            {
                return _UseTermo;
            }
            set
            {
                if (_UseTermo == value)
                    return;
                _UseTermo = value;
                RaisePropertyChanged("UseTermo");
            }
        }

        public MainViewModel()
        {
            Connect_Disconnect = new Tools.Command(x => Connect_DisconnectAction(), x => !String.IsNullOrEmpty(SelectedPort));
            StartExperiment = new Tools.Command(x => StartExperimentAction(), x => _port != null);
            Export = new Tools.Command(x => ExportExcel(), x => true);//_experimentActualLog.Count > 0);
            SaveExperiment = new Tools.Command(x => SaveExperimentToFile(), x => true);
            OpenExperiment = new Tools.Command(x => OpenExperimentFile(), x => !IsExperimentMode);
            AddPropertyContainer = new Tools.Command(x => AddPropertyContainerAction(), x => true);
            DeletePropertyContainer = new Tools.Command(x => DeletePropertyContainerAction(), x => true);

            Properties = Model.PropertyList.OpenList();

            _availablePorts = new Tools.ViewableCollection<string>();
            _availablePorts.Fill(System.IO.Ports.SerialPort.GetPortNames());
            SelectedPort = AvailablePorts.FirstOrDefault();

            _experimentActualLog = new List<Model.ExperimentLog>();
            _currentValues = new Tools.ViewableCollection<Model.CurrentValue>();
            _experimentValues = new Tools.ViewableCollection<Model.ExperimentLog>();
            _StopUpdate = false;

            _dataRequestTimer = new System.Timers.Timer(1000);
            _dataRequestTimer.Elapsed += _dataRequestTimer_Elapsed;
            _dataRequestTimer.Start();
            
            //GenerateTestData();
        }

        void GenerateTestData()
        {
            var temp = new List<Model.CurrentValue>();
            for (int i = 0; i < 800; i++)
                temp.Add(new Model.CurrentValue(i * 1.25, Math.Sin(i / 30.0 + DateTime.Now.Second) * 80.0 + 140.0));
            //_currentValues.BulkFill(temp);

            _experimentActualLog.Clear();
            for (int i = 1; i < 11500; i += 30)
                _experimentActualLog.Add(new Model.ExperimentLog(i, DateTime.Now.Second * i ^ 2 + 8 * i + 110));

            RedrawExperiment();
        }

        void _dataRequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //GenerateTestData();

            lock (locker)
            {
                if (_port == null)
                    return;
            }
            if (!_port.TransmitAvailable)
                return;

            var cmd = new Communication.RequestExperimentStateCommand();
            cmd.RespondRecieved += ExperimentRespondRecieved;
            _port.SendCommand(cmd);
        }

        void ExperimentRespondRecieved(object sender, Communication.ExperimentStateRecievedEventArgs e)
        {
            TD1 = string.Format("СРЕД: {0}", e.AverageTenso);
            //TD1 = string.Format("ТД1: {0} °C", Math.Round(e.TD1, 1));
            TD2 = string.Format("ТД2: {0} °C", Math.Round(e.TD2, 1));
            TD3 = string.Format("ТД3: {0} °C", Math.Round(e.TD3, 1));
            Position = string.Format("ПОЗ: {0} мм", Math.Round(e.Position, 2));
            Cycles = string.Format("ЦКЛ: {0}", e.Cycles);
            SetStatus((Communication.State)e.State);
            if (_experimentActualLog.Count > 0)
                Delta = string.Format("ДЕФ: {0} мм", Math.Round(e.Position - _experimentActualLog[0].Position, 2));
            else
                Delta = "ДЕФ: н/д";
            //Elapsed = string.Format("ОСТ: {0}", e.AverageTenso);
            if (_ExperimentDuration != 0)
            {
                double secs = (double)_ExperimentDuration / (double)_ExperimentFrequency - (double)e.Cycles / (double)_ExperimentFrequency;
                var ts = TimeSpan.FromSeconds(secs);
                Elapsed = string.Format("ОСТ: {0}", ts);
            }
            else
                Elapsed = "ОСТ: н/д";

            if (!_StopUpdate)
                ReadTensos(e.Tensos);

            CyclesRecieved(e.Cycles, e.Position);
            RedrawExperiment();

            if(e.Cycles > 0 )
                SaveTempExperiment();
        }
        private void SetStatus(Communication.State state)
        {
            if (state == Communication.State.Ready)
                CurrentState = _READY;
            else if (state == Communication.State.GetReady)
                CurrentState = _PREPARING;
            else if (state == Communication.State.Operating)
                CurrentState = _RUNNING;
            else if (state == Communication.State.ServoError)
                CurrentState = _SERVOERROR;
            else if (state == Communication.State.StepperError)
                CurrentState = _STEPPERERROR;
        }
        private void CyclesRecieved(int cycles, double position)
        {
            if (cycles <= 0)
                return;

            _LastCycles = cycles;
            if (_experimentActualLog.Count >= 2 &&
                _experimentActualLog[_experimentActualLog.Count - 1].Position == position &&
                _experimentActualLog[_experimentActualLog.Count - 2].Position == position)
                _experimentActualLog.RemoveAt(_experimentActualLog.Count - 1);  //Забываем последнее значение, если оно равно предпоследнему

            _experimentActualLog.Add(new Model.ExperimentLog(cycles, position));

        }
        private void ReadTensos(double[] tensos)
        {
            List<Model.CurrentValue> values = new List<Model.CurrentValue>(800);
            int div = 1;

            for (int i = 0; i < tensos.Length; i += div)
                values.Add(new Model.CurrentValue(i * 1.25, tensos[i]));

            CurrentValues.BulkFill(values);
        }
        private void RedrawExperiment()
        {
            int proxyCount = 800;

            double startPosition = 0;
            if (_experimentActualLog.Count > 0)
                startPosition = _experimentActualLog[0].Position;

            if (_experimentActualLog.Count > proxyCount * 2)
            {
                int step = _experimentActualLog.Count / proxyCount;
                ExperimentValues.BulkFill(_experimentActualLog.Where((x, i) => i % step == 0).
                    Select(x => new Model.ExperimentLog(x.Cycle, x.Position - startPosition)).ToArray());
            }
            else
                ExperimentValues.BulkFill(_experimentActualLog.
                    Select(x => new Model.ExperimentLog(x.Cycle, x.Position - startPosition)));
        }

        private void SaveExperimentToFile()
        {
            var exp = new Tools.ExperimentSerializer(ExperimentFrequency, ExperimentWeight, ExperimentTemperature, _experimentActualLog);
            Microsoft.Win32.SaveFileDialog sd = new Microsoft.Win32.SaveFileDialog();
            sd.DefaultExt = ".expml";
            sd.Filter = "Результаты эксперимента (.expml)|*.expml";
            bool? result = sd.ShowDialog();

            if (result == true)
                Tools.ExperimentSerializer.Save(sd.FileName, exp);
        }
        private void SaveTempExperiment()
        {
            var exp = new Tools.ExperimentSerializer(ExperimentFrequency, ExperimentWeight, ExperimentTemperature, _experimentActualLog);
            Tools.ExperimentSerializer.Save(TEMPORAL_FILENAME, exp);
        }
        private void OpenExperimentFile()
        {
            Microsoft.Win32.OpenFileDialog od = new Microsoft.Win32.OpenFileDialog();
            od.DefaultExt = ".expml";
            od.Filter = "Результаты эксперимента (.expml)|*.expml";
            bool? result = od.ShowDialog();

            if (result == true)
            {
                var exp = Tools.ExperimentSerializer.Open(od.FileName);
                ExperimentFrequency = exp.Frequency;
                ExperimentWeight = exp.Weight;
                ExperimentTemperature = exp.Temperature;
                _experimentActualLog.Clear();
                _experimentActualLog.AddRange(exp.ExperimentValues);
                RedrawExperiment();
            }
        }
        private void ExportExcel()
        {
            Microsoft.Win32.SaveFileDialog sd = new Microsoft.Win32.SaveFileDialog();
            sd.DefaultExt = ".csv";
            sd.Filter = "Электронная таблица (.csv)|*.csv";
            bool? result = sd.ShowDialog();

            if (result == true)
                new Tools.ExperimentExporter(sd.FileName, _experimentActualLog).Export();
        }

        private void StartExperimentAction()
        {
            while (!_port.TransmitAvailable) ;

            if (!IsExperimentMode)
            {
                _experimentActualLog.Clear();
                RedrawExperiment();

                while (!_port.TransmitAvailable) ;
                _port.SendCommand(new Communication.StartExperimentRequest(
                    ExperimentFrequency, 
                    UseTermo?ExperimentTemperature:0, 
                    ExperimentWeight, 
                    ExperimentDuration));

            }
            else
            {
                while (!_port.TransmitAvailable) ;
                _port.SendCommand(new Communication.StopExperimentRequest());
            }
            IsExperimentMode = !IsExperimentMode;
        }
        private void Connect_DisconnectAction()
        {
            lock (locker)
            {
                if (_port != null)
                {
                    _port.Dispose();
                    _port = null;
                    CurrentState = _NOCONNECTION;
                }
                else
                {
                    _port = new Communication.Port(SelectedPort, BAUD_RATE);
                    CurrentState = _READY;
                }
                StartExperiment.RaiseCanExecuteChanged();
            }
        }

        private void AddPropertyContainerAction()
        {
            ExperimentPropertiesViewModel evm = new ExperimentPropertiesViewModel();
            if (evm.Show())
            {
                Properties.Properties.Add(evm.Properties);
                Properties.SaveList();
            }
        }
        private void DeletePropertyContainerAction()
        {
            if (CurrentPropertyContainer == null)
                return;
            else
            {
                Properties.Properties.Remove(CurrentPropertyContainer);
                Properties.SaveList();
            }
        }
        private void ReadProperties()
        {
            if (CurrentPropertyContainer == null)
                return;

            ExperimentFrequency = CurrentPropertyContainer.ExperimentFrequency;
            ExperimentWeight = CurrentPropertyContainer.ExperimentWeight;
            ExperimentTemperature = CurrentPropertyContainer.ExperimentTemperature;
            UseTermo = CurrentPropertyContainer.UseTermo;
            ExperimentDuration = CurrentPropertyContainer.ExperimentDuration;            
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
