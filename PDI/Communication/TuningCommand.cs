using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public class TuningCommand : BaseCommand
    {

        public override int DataLen
        {
            get { return 3; }
        }

        public TuningCommand(int period, int acceleration)
            : base(new byte[] { 0x0f, MicsToByte(period), MicsToByte(acceleration) })
        {   }

        private static byte MicsToByte(int mcs)
        {
            if(mcs > 51000)
                throw new Exception("Длительность не может превышать 51000 мкс");
            return (byte)(mcs / 200);
        }

        public override void OnRespondRecieved(byte[] data)
        {
            ;
        }
    }
}
