﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    class RequestExperimentStateCommand : BaseCommand
    {
        public override int DataLen
        {
            get { return 1610; }
        }

        public event RequestExperimentStateRecievedEventHandler RespondRecieved;

        double[] _recivedTenso = new double[800];
        public double TD1 { get; private set; }

        public RequestExperimentStateCommand() : base(new byte[] { 0x01 }) { }

        public override void GenerateRespond(byte[] data)
        {
            RequestExperimentStateRecievedEventArgs ea = new RequestExperimentStateRecievedEventArgs
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
            return (data[1600 + termoId * 2] << 8) + data[1600 + termoId * 2 + 1];
        }
        private double GetPosition(byte[] data)
        {
            return ((data[1609] << 8) + data[1610]) / 200.0; //1000 импульсов на 5 мм
        }
    }
    delegate void RequestExperimentStateRecievedEventHandler(object sender, RequestExperimentStateRecievedEventArgs eventArgs);

    class RequestExperimentStateRecievedEventArgs : EventArgs
    {
        public double[] Tensos { get; private set; }
        public double TD1 { get; private set; }
        public double TD2 { get; private set; }
        public double TD3 { get; private set; }
        public double TD4 { get; private set; }
        public double Position { get; private set; }

        public RequestExperimentStateRecievedEventArgs(double[] tensos,
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