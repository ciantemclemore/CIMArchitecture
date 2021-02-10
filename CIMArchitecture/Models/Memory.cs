using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture.Models
{
    public static class Memory
    {
        private static Queue<int> _memoryBlock = new Queue<int>();

        public static bool Create(int size) 
        {
            bool status = false;

            if (_memoryBlock == null) 
            {
                _memoryBlock = new Queue<int>(size);
                status = true;
            }
            return status;
        }

        public static int GetFromMemory() 
        {
            return _memoryBlock.Count > 0 ? _memoryBlock.Dequeue() : 0;
        }

        public static void StoreInMemory(int item)
        {
            _memoryBlock.Enqueue(item);
        }

        public static bool MemoryCopy(int size) 
        {
            var temp = new Queue<int>(size);
            bool status = false;

            if (temp.Count >= _memoryBlock.Count) 
            {
                foreach (var block in _memoryBlock) 
                {
                    temp.Enqueue(block);
                }
                _memoryBlock = new Queue<int>(temp);
                status = true;
            }
            return status;
        }

        public static void ClearMemory() 
        {
            _memoryBlock.Clear();
        }

        public static int GetCount() 
        {
            return _memoryBlock.Count;
        }

        public static int[] AsArray() 
        {
            return _memoryBlock.ToArray();
        }
    }
}
