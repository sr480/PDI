using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public abstract class BaseCommand
    {
        public byte[] Message
        { get; private set; }
        public abstract int DataLen
        { get; }
        

        public BaseCommand(byte[] message)
        {
            Message = message;
        }

        public abstract void GenerateRespond(byte[] data);
    }
}
