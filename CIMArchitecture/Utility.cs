using CIMArchitecture.Models;
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
                binaryString = storage[length - 1 - i].ToString() + binaryString;
            }
            return binaryString;
        }

        public static Register ToRegister(this string source, Configuration _configuration)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_configuration.Registers.ContainsKey(source))
                {
                    return _configuration.Registers[source];
                }
            }
            return null;
        }

        public static Instruction ToInstruction(this string source, Configuration _configuration)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_configuration.Instructions.ContainsKey(source))
                {
                    return _configuration.Instructions[source];
                }
            }
            return null;
        }
    }
}
