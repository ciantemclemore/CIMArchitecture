using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture.Models
{
    public static class Memory
    {
        private static int _memSize;
        private static int[] _memoryBlock;

        public static void Create(int size) 
        {
            _memSize = size;
            _memoryBlock = new int[size];
            for (int i = 0; i < _memSize; i++) 
            {
                //-1 signifies an empty slot
                _memoryBlock[i] = -1;
            }
        }

        public static int GetFromMemory(int value) 
        {
            int result = -1;

            for (int i = 0; i < _memoryBlock.Length; i++) 
            {
                if (_memoryBlock[i] == value) 
                {
                    result = _memoryBlock[i];
                    break;
                }
            }
            return result;
        }

        public static bool StoreInMemory(int item)
        {
            bool status = false;
            if (_memoryBlock != null) 
            {
                for (int i = 0; i < _memSize; i++) 
                {
                    if (_memoryBlock[i] == -1) 
                    {
                        _memoryBlock[i] = item;
                        status = true;
                        return status;
                    }
                }
            }
            return status;
        }

        public static bool MemoryCopy(int size) 
        {
            var temp = new int[size];

            if (_memoryBlock != null) 
            {
                for (int i = 0; i < size; i++) 
                {
                    if (i < _memoryBlock.Length && _memoryBlock[i] != 0) 
                    {
                        temp[i] = _memoryBlock[i];
                    }
                }
                return true;
            }
            return false;
        }

        public static void ClearMemory() 
        {
            for (int i = 0; i < _memSize; i++) 
            {
                _memoryBlock[i] = -1;
            }
        }

        public static int GetCount() 
        {
            if (_memoryBlock != null) 
            {
                return _memoryBlock.Length;
            }
            return default;
        }

        public static int[] GetMemBlock() 
        {
            return _memoryBlock;
        }
    }
}
