using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture.Models
{
    public static class Memory
    {
        private static int[] _memoryBlock;

        public static void Create(int size) 
        {
            _memoryBlock = new int[size];
            for (int i = 0; i < _memoryBlock.Length; i++) 
            {
                //-1 signifies an empty slot
                _memoryBlock[i] = -1;
            }
        }

        public static int LoadFromMemory(int value) 
        {
            //Value not in memory
            int exitCode1 = -1;

            //Memory null
            int exitCode2 = -2;

            if (_memoryBlock != null)
            {
                for (int i = 0; i < _memoryBlock.Length; i++)
                {
                    if (_memoryBlock[i] == value)
                    {
                        return _memoryBlock[i];
                    }
                }
                return exitCode1;
            }else
                return exitCode2;
        }

        public static CommandValidation StoreInMemory(int item)
        {
            if (_memoryBlock != null)
            {
                for (int i = 0; i < _memoryBlock.Length; i++)
                {
                    if (_memoryBlock[i] == -1)
                    {
                        _memoryBlock[i] = item;
                        return new CommandValidation() { IsValid = true, Message = Result.NoErrorMessage };
                    }
                }
            }
            else 
            {
                return new CommandValidation() { IsValid = false, Message = "Memory was not created" };
            }
            return new CommandValidation() { IsValid = false, Message = "Memory is full" };
        }

        public static CommandValidation MemoryCopy(int size) 
        {
            var temp = new int[size];
            int count = 0;

            if (_memoryBlock != null)
            {   
                for (int i = 0; i < temp.Length; i++)
                {
                    if (i == _memoryBlock.Length) break;
                    if (_memoryBlock[i] == -1) break;

                    if (i < _memoryBlock.Length && _memoryBlock[i] != -1)
                    {
                        temp[i] = _memoryBlock[i];
                        count++;
                    }
                }
                //Now specify new array size and initialize empty spots as -1
                _memoryBlock = new int[size];
                
                for(int i = 0; i < _memoryBlock.Length; i++) 
                {
                    _memoryBlock[i] = -1;
                }

                //Now copy temp to memoryBlock array
                for (int i = 0; i < count; i++) 
                {
                    if (_memoryBlock[i] == -1) 
                    {
                        _memoryBlock[i] = temp[i];
                    }
                }
            }
            else 
            {
                return new CommandValidation() { IsValid = false, Message = "Memory was not created" };
            }
            return new CommandValidation() { IsValid = true, Message = Result.NoErrorMessage }; ;
        }

        public static CommandValidation ClearMemory() 
        {
            if (_memoryBlock != null)
            {
                for (int i = 0; i < _memoryBlock.Length; i++)
                {
                    _memoryBlock[i] = -1;
                }
            }
            else 
            {
                return new CommandValidation() { IsValid = false, Message = "Memory was not created" };
            }
            return new CommandValidation() { IsValid = true, Message = Result.NoErrorMessage }; ;
        }

        public static int GetCountOfValues() 
        {
            int count = 0;
            if (_memoryBlock != null) 
            {
                for (int i = 0; i < _memoryBlock.Length; i++)
                {
                    if (_memoryBlock[i] != -1)
                        count++;
                }
                return count;
            }
            return -1;
        }

        public static int GetCount() 
        {
            if (_memoryBlock != null) 
            {
                return _memoryBlock.Length;
            }
            return -1;
        }

        public static int[] GetMemBlock() 
        {
            return _memoryBlock;
        }
    }
}
