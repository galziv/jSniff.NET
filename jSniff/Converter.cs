using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jSniff
{
    public static class Converter
    {
        public static DateTime ConvertJsDateToDateTime(string date)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(date));
        }
    }
}
