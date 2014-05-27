using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace jotb.Library
{
    class Barcode
    {
        public static bool isValidCode11(string input)
        {
            return Regex.IsMatch(input, @"^(\d|\-)+$");
        }

        public static bool isValidCode39(string input)
        {
            return Regex.IsMatch(input, @"^([a-zA-Z0-9\-\.\$\/\+\%\s])+$");
        }

        public static bool isValidCode128(string input)
        {
            return Regex.IsMatch(input, @".+");
        }

        public static bool isValidEan8(string input)
        {
            return Regex.IsMatch(input, @"^(\d){7}$");
        }

        public static bool isValidEan13(string input)
        {
            return Regex.IsMatch(input, @"^(\d){12}$");
        }
    }
}
