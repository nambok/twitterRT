using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterRT
{
    static class Helpers
    {
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
    }
}
