using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDI.Communication;

namespace ComDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("выбери порт:");
            var ports = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
                Console.WriteLine("{0}. {1}", i, ports[i]);

            int portId = -1;
            while (portId < 0)
            {
                Console.WriteLine("Введите номер порта: ");
                string i = Console.ReadLine();
                if (int.TryParse(i, out portId))
                    break;
            }
            Port port;
            try
            {
                port = new Port(ports[portId], 1000000, true);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine("Программа будет закрыта :(");
                Console.ReadKey();
                return;
            }
            while (true)
            {
                try
                {
                    Console.WriteLine("Введи команду без КС в виде ХХ, для нескольких байт отделяй пробелом ХХ ХХ ХХ (пример: 01 f3 a2):");
                    string input = Console.ReadLine();
                    string[] btStrings = input.Split(new char[] { ' ' });
                    byte[] bts = btStrings.Select(b => Convert.ToByte(b, 16)).ToArray();
                    port.CancelLastTransmit();
                    port.SendCommand(new CustomCommand(bts));
                }
                catch (Exception err)
                { Console.WriteLine(err.Message); }
            }
            
        }
    }
    class CustomCommand : BaseCommand
    {
        public override int DataLen
        {
            get { return 2000; }
        }

        public CustomCommand(byte[] bt)
            : base(bt)
        { }

        public override void OnRespondRecieved(byte[] data)
        {
            Console.WriteLine("Получен ответ");
            foreach (byte bt in data)
                Console.Write(bt.ToString("X2") + " ");
            Console.WriteLine();
        }
    }
}
