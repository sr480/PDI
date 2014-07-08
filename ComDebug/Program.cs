using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            var crc = new PDI.Communication.CRC16();
            var c = crc.Calculate(new byte[] {0x31, 0x45});

            PDI.Communication.Port prt = new PDI.Communication.Port("COM4", 9600);
            PDI.Communication.RequestExperimentStateCommand cmd = new PDI.Communication.RequestExperimentStateCommand();
            cmd.RespondRecieved += cmd_RespondRecieved;
            prt.SendCommand(cmd);

            var timeStamp = DateTime.Now;
            Console.ReadKey();

        }

        static void cmd_RespondRecieved(object sender, PDI.Communication.RequestExperimentStateRecievedEventArgs eventArgs)
        {
            Console.WriteLine("Ответ получен");
        }
    }
}
