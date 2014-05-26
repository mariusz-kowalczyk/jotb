using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Library
{
    class Fun
    {
        /**
         * <summary>
         * Sprawdza czy istnieje element w tablicy
         * </summary>
         */
        public static bool inArray<T>(T needle, T[] array) {
            foreach (T el in array)
            {
                if (el.Equals(needle))
                {
                    return true;
                }
            }
            return false;
        }
        /*public float floatParse(string zmienna)
        {
            //Szmienna
        }*/
    }
}
