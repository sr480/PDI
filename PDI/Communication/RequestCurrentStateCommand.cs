using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public class RequestTemperatureCommand : BaseCommand
    {
        public override int DataLen
        {
            get { return 10; }
        }
        public event TemperatureRecievedEventHandler RespondRecieved;

        public RequestTemperatureCommand()
            : base(new byte[] { 0x03 }) { }

        public override void OnRespondRecieved(byte[] data)
        {
            if(RespondRecieved != null)
                RespondRecieved(this, new TemperatureRecievedEventArgs(
                    BytesToTemperature.GetTemperature(data[1], data[0]),
                    BytesToTemperature.GetTemperature(data[3], data[2]),
                    BytesToTemperature.GetTemperature(data[5], data[4]),
                    BytesToTemperature.GetTemperature(data[7], data[6])));
                
        }
    }
    public delegate void TemperatureRecievedEventHandler(object sender, TemperatureRecievedEventArgs e);
    public class TemperatureRecievedEventArgs : EventArgs
    {
        public double TD1 { get; private set; }
        public double TD2 { get; private set; }
        public double TD3 { get; private set; }
        public double TD4 { get; private set; }

        public TemperatureRecievedEventArgs(double td1,
                                            double td2,
                                            double td3,
                                            double td4)
        {
            TD1 = td1;
            TD2 = td2;
            TD3 = td3;
            TD4 = td4;
         
        }
    }

}
