using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    class StartExperimentRequest : BaseCommand
    {
        public override int DataLen
        {
            get { return 3; }
        }

        public StartExperimentRequest(int freq, int temp, int weight, int duration) :
            base(new byte[] { 0x0f, 0x03, 0xff, 0xff })

            //base(new byte[] {0x02, 
        //                 (byte)freq,
        //                 (byte)temp,
        //                 (byte)weight, (byte)(weight>>8),
        //                 (byte)duration, (byte)(duration>>8), (byte)(duration>>16)})
        { }

        public override void OnRespondRecieved(byte[] data)
        {
            ;
        }
    }
}
