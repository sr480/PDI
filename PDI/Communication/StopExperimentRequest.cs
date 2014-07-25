using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    class StopExperimentRequest : BaseCommand
    {
        public override int DataLen
        {
            get { return 3; }
        }

        public StopExperimentRequest() :
            base(new byte[] { 0x0f, 0x00, 0x00, 0x00 })
        { }

        public override void OnRespondRecieved(byte[] data)
        {
            ;
        }
    }
}
