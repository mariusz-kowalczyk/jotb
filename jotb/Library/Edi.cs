using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jotb.Library
{
    class Edi
    {

        /**
         * <summary>
         * Wyszukuje w ciągu znaków znaki (+, ', ?, :) i stawia przed nimi znak ?
         * </summary>
         */
        public static string Filter(string str)
        {
            string newStr = "";
            char[] preChars = new char[] {'+', '\'', '?', ':'};
            foreach (char c in str)
            {
                if (Fun.inArray(c, preChars))
                {
                    newStr += "?";
                }
                newStr += c;
            }
            return newStr;
        }

        /**
         * <summary>
         * Rozbija stringa na tablicą, jeżeli separator jest poprzedzony znakiem zapytania to go pomija
         * </summary>
         */
        public static string[] Split(string str, char separator)
        {
            List<string> results = new List<string>();
            string buffor = "";
            int n = str.Length;
            bool br = false;
            for (int i = 0; i < n; i++)
            {
                if ((i == 0 || str[i - 1] != '?') && str[i] == separator)
                {
                    results.Add(buffor);
                    buffor = "";
                    br = true;
                    continue;
                }
                br = false;
                buffor += str[i];
            }
            if (buffor != "" || br)
            {
                results.Add(buffor);
            }

            return results.ToArray();
        }

        /**
         * Zwraca segmenty z pliku edi które są oddzilone od siebie apostrofem.
         */
        public static string[] ParseToSegmets(string strEdi)
        {
            return Split(strEdi, '\'');
        }

        /**
         * <summary>
         * Funkcja zwraca tablicę z argumentami
         * </summary>
         */
        public static string[] ParseToArguments(string Segment) {
            return Split(Segment, '+');
        }

        /**
         * <summary>
         * Zwraca tablicę rozbitą za pomocą dwukropka
         * </summary>
         */
        public static string[] ParseToParameters(string Argument)
        {
            return Split(Argument, ':');
        }
    }
}
