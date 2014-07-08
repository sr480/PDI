using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public class RequestExperimentStateCommand : BaseCommand
    {
        public override int DataLen
        {
            get { return 1612; }
        }

        public event ExperimentStateRecievedEventHandler RespondRecieved;

        public RequestExperimentStateCommand() : base(new byte[] { 0x01 }) { }

        public override void OnRespondRecieved(byte[] data)
        {
            ExperimentStateRecievedEventArgs ea = new ExperimentStateRecievedEventArgs
                (
                    GetTensos(data),
                    GetTermo(data, 0),
                    GetTermo(data, 1),
                    GetTermo(data, 2),
                    GetTermo(data, 3),
                    GetPosition(data)
                );

            if (RespondRecieved != null)
                RespondRecieved(this, ea);
        }

        private double[] GetTensos(byte[] data)
        {
            double[] result = new double[800];
            for (int pos = 0; pos < 800; pos++)
                result[pos] = ((data[pos * 2] << 8) + data[pos * 2 + 1]);
            return result;
        }
        private double GetTermo(byte[] data, int termoId)
        {
            return BytesToTemperature.GetTemperature(data[1600 + termoId * 2], data[1600 + termoId * 2 + 1]);
        }
        private double GetPosition(byte[] data)
        {
            return ((data[1609] << 8) + data[1610]) / 200.0; //1000 импульсов на 5 мм
        }
    }
    public delegate void ExperimentStateRecievedEventHandler(object sender, ExperimentStateRecievedEventArgs eventArgs);

    public class ExperimentStateRecievedEventArgs : EventArgs
    {
        public double[] Tensos { get; private set; }
        public double TD1 { get; private set; }
        public double TD2 { get; private set; }
        public double TD3 { get; private set; }
        public double TD4 { get; private set; }
        public double Position { get; private set; }

        public ExperimentStateRecievedEventArgs(double[] tensos,
                                                        double td1,
            double td2,
            double td3,
            double td4,
            double position)
        {
            Tensos = tensos;
            TD1 = td1;
            TD2 = td2;
            TD3 = td3;
            TD4 = td4;
            Position = position;
        }
    }
}
