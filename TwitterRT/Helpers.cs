using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitterRT
{
    static class Helpers
    {
        public static Random rnd = new Random(); 

        public static string RandomString(int size)
        {
            string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random _rng = new Random();
            
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        public static string Spintax(string str)
        {
            // Loop over string until all patterns exhausted.
            string pattern = "{[^{}]*}";
            Match m = Regex.Match(str, pattern);
            while (m.Success)
            {
                // Get random choice and replace pattern match.
                string seg = str.Substring(m.Index + 1, m.Length - 2);
                string[] choices = seg.Split('|');
                str = str.Substring(0, m.Index) + choices[Helpers.rnd.Next(choices.Length)] + str.Substring(m.Index + m.Length);
                m = Regex.Match(str, pattern);
            }

            // Return the modified string.
            return str;
        }
    }
}
