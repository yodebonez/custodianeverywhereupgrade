using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataStore.ExtensionMethods
{
    public static class MyExtensions
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", string.Empty);
        }
    }
}
