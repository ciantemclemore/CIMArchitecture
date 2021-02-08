using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture
{
    public static class Utility
    {
        public static string ToBinary(this int value, int length)
        {
            int[] storage = new int[length];
            string binaryString = string.Empty;

            for (int i = 0; i < length; i++)
            {
                storage[length - 1 - i] = value % 2;
                value /= 2;
                binaryString += storage[i].ToString();
            }
            return binaryString;
        }
    }
}
