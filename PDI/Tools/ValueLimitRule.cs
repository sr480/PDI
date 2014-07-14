using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PDI.Tools
{
    class ValueLimitRule : ValidationRule
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                double val = Convert.ToDouble(value);
                if (val > Max)
                    return new ValidationResult(false, "Не больше " + Max);
                if (val < Min)
                    return new ValidationResult(false, "Не менее " + Min);
                return new ValidationResult(true, null);
            }
            catch
            {
                return new ValidationResult(false, "Неверный формат");
            }
        }
    }
}
