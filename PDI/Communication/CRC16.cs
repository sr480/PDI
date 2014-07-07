using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDI.Communication
{
    public class CRC16
    {
        private UInt16[] _table;
        private UInt16 _polynom;

        public CRC16()
        {
            _polynom = 0x8005;
            _table = new UInt16[256];
            calcCRCTable();
        }

        public byte[] Calculate(byte[] data, int len)
        {
            UInt16 crc = Calc_CRC(data, len);
            return new byte[] { (byte)(crc >> 8), (byte)crc };
        }

        public byte[] Calculate(byte[] data)
        {
            UInt16 crc = Calc_CRC(data, data.Length);
            return new byte[] {(byte)(crc>>8), (byte)crc };
        }

        private UInt16 Calc_CRC(byte[] buf, int len)
        {
            ushort crc = 0;
            for (int i = 0; i < len; i++)
            {
                byte index = (byte)((crc >> 8) ^ buf[i]);
                crc = (ushort)(_table[index] ^ (crc << 8));
            }
            return crc;
        }
        
        private void calcCRCTable()
        {
            UInt16 value;
            for (int i = 0; i < _table.Length; ++i)
            {
                value = (UInt16)(i << 8);

                for (byte j = 0; j < 8; ++j)
                {
                    if ((value & (1 << 15)) != 0)
                    {
                        value = (UInt16)((value << 1) ^ _polynom);
                    }
                    else
                    {
                        value <<= 1;
                    }
                }
                _table[i] = value;
            }
        }
    }
}
