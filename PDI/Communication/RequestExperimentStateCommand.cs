﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    [Flags]
    public enum State : byte
    {
        Ready = 0,
        GetReady = 1,
        Operating = 2,

        StepperError = 32,
        ServoError = 128
    }

    public class RequestExperimentStateCommand : BaseCommand
    {
        public override int DataLen
        {
            get { return 1615; }
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
                    GetCycles(data),
                    GetState(data),                    
                    GetPosition(data),
                    GetAverage(data)
                );

            if (RespondRecieved != null)
                RespondRecieved(this, ea);
        }

        private double[] GetTensos(byte[] data)
        {
            double[] result = new double[800];
            for (int pos = 0; pos < 800; pos++)
                result[pos] = BytesToWeight.GetWeight(data[pos * 2], data[pos * 2 + 1]);
            return result;
        }

        private double GetAverage(byte[] data)
        {
            double avg = 0;
            for (int pos = 0; pos < 800; pos++)
                avg += (data[pos * 2 + 1] << 8) + data[pos * 2];
            return avg / 800.0;
        }

        private double GetTermo(byte[] data, int termoId)
        {
            return BytesToTemperature.GetTemperature(data[1602 + termoId * 2 + 1], data[1602 + termoId * 2]);
        }
        private int GetCycles(byte[] data)
        {
            return data[1608] + (data[1609] << 8) + (data[1610] << 16) + (data[1611] << 24);
        }
        private byte GetState(byte[] data)
        {
            return data[1612];
        }
        private double GetPosition(byte[] data)
        {
            return ((data[1601] << 8) + data[1600]) / 160.0; //1600 импульсов на 5 мм
        }
    }
    public delegate void ExperimentStateRecievedEventHandler(object sender, ExperimentStateRecievedEventArgs e);

    public class ExperimentStateRecievedEventArgs : EventArgs
    {
        public double[] Tensos { get; private set; }
        public double TD1 { get; private set; }
        public double TD2 { get; private set; }
        public double TD3 { get; private set; }
        public double Position { get; private set; }
        public int Cycles { get; private set; }
        public byte State { get; private set; }
        public double AverageTenso { get; private set; }

        public ExperimentStateRecievedEventArgs(double[] tensos,
                                                        double td1,
            double td2,
            double td3,
            int cycles,
            byte state,
            double position,
            double average)
        {
            Tensos = tensos;
            TD1 = td1;
            TD2 = td2;
            TD3 = td3;
            State = state;
            Cycles = cycles;
            Position = position;
            AverageTenso = average;
        }
    }
}
