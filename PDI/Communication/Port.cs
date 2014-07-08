using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace PDI.Communication
{
    public class Port : IDisposable
    {
        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        private const int MAX_RESENDS = 3;
        private int _resends = 0;
        private CRC16 _crcHelper;
        private SerialPort _port;
        private BaseCommand _lastCommand;
        private byte[] _respondeBuf;
        private int _bufReadCursor = 0;

        public Port(string portName, int baudRate)
        {
            _crcHelper = new CRC16();
            _port = new SerialPort(portName, baudRate);
            _port.DataReceived += _port_DataReceived;
            _port.Open();
        }

        public void SendCommand(BaseCommand command)
        {
            if (_lastCommand != null)
                throw new Exception("Обработка предыдущей команды не завершена");

            _lastCommand = command;
            _respondeBuf = new byte[_lastCommand.DataLen];

            _resends = 0;

            Send();
        }

        private void Send()
        {
            _bufReadCursor = 0;

            byte[] commandBuf = new byte[_lastCommand.Message.Length + 2];
            _lastCommand.Message.CopyTo(commandBuf, 0);
            _crcHelper.Calculate(_lastCommand.Message).CopyTo(commandBuf, _lastCommand.Message.Length);

            _port.Write(commandBuf, 0, commandBuf.Length);
            _resends++;
            Console.WriteLine("Отправка сообщения:");
            foreach (var bt in commandBuf)
            {
                Console.Write(bt.ToString("X2") + " ");
            }
            Console.WriteLine("Попытка {0} из {1}", _resends, MAX_RESENDS);
        }

        void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_lastCommand == null)
            {
                _port.ReadExisting();
                Console.WriteLine("Получен неожиданный ответ");
                return;
            }

            int len = _port.BytesToRead;
            if (_bufReadCursor + len > _lastCommand.DataLen)
                throw new Exception("Длина ответа превысила ожидаемую");

            int dataread = _port.Read(_respondeBuf, _bufReadCursor, len);
            if (dataread == 0)
                return;

            Console.WriteLine("Получено ссобщение:");
            for (int pos = _bufReadCursor; pos < _bufReadCursor + dataread; pos++)
                Console.Write(_respondeBuf[pos].ToString("X2") + " ");
            _bufReadCursor += dataread;
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(_respondeBuf, 0, _bufReadCursor));
            if (_bufReadCursor == _lastCommand.DataLen)
                OnRespondeRecieved();
        }

        void OnRespondeRecieved()
        {
            int len = _respondeBuf.Length;
            byte[] cCrc = _crcHelper.Calculate(_respondeBuf, len - 2);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            for (int pos = 0; pos < len; pos++)
                Console.Write(_respondeBuf[pos].ToString("X2"));

            if (_respondeBuf[len - 2] != cCrc[0] |
               _respondeBuf[len - 1] != cCrc[1])
            {
                Console.WriteLine("Ошибка контрольной суммы");
                Resend();
                return;
            }



            _lastCommand.GenerateRespond(_respondeBuf);
        }

        private void Resend()
        {
            if (_resends < MAX_RESENDS)
                Send();
            else
                throw new Exception("Превышено количество повторных запросов.");
        }

        public void Dispose()
        {
            if (_port != null)
                _port.Dispose();
        }
    }
}
