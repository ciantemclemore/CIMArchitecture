using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CIMArchitecture
{
    /// <summary>
    /// The CIMFactory is responsible for gathering and storing instruction data.
    /// </summary>
    class CIMFactory
    {
        public Dictionary<string, Instruction> Instructions { get; } = new Dictionary<string, Instruction>();

        public Dictionary<string, Register> Registers { get; } = new Dictionary<string, Register>();

        public List<string> UserInput { get; } = new List<string>();

        private const string _instructionPath = "Database/InstructionDatabase.json";
        private const string _registerPath = "Database/RegisterDatabase.json";
        private const int _max16BitValue = 65536;
        private const int _max8BitValue = 256;

        public CIMFactory()
        {
            var instructionJson = GetJsonData(_instructionPath);
            var registerJson = GetJsonData(_registerPath);

            Instructions = StoreInstructions(instructionJson);
            Registers = StoreRegisters(registerJson);
        }

        public string GetJsonData(string path)
        {
            string data = string.Empty;

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    data = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return data;
        }

        public Dictionary<string, Instruction> StoreInstructions(string jsonString)
        {
            var instructions = JsonSerializer.Deserialize<List<Instruction>>(jsonString);
            var instructionDictionary = new Dictionary<string, Instruction>();

            if (instructions != null)
            {
                foreach (var instr in instructions)
                {
                    if (!Instructions.ContainsKey(instr.Name))
                    {
                        instructionDictionary.Add(instr.Name, instr);
                    }
                }
            }

            return instructionDictionary;
        }

        public Dictionary<string, Register> StoreRegisters(string jsonString)
        {
            var registers = JsonSerializer.Deserialize<List<Register>>(jsonString);
            var registerDictionary = new Dictionary<string, Register>();

            if (registers != null)
            {
                foreach (var reg in registers)
                {
                    if (!Registers.ContainsKey(reg.Name))
                    {
                        registerDictionary.Add(reg.Name, reg);
                    }
                }
            }

            return registerDictionary;
        }

        public void GatherUserInstructionCommands()
        {
            string key = "run";
            while (true)
            {
                Console.WriteLine("Enter instructions:");
                string instruction = Console.ReadLine();

                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                UserInput.Add(instruction);
            }
        }

        public void LoadImmediate(string source, string constant)
        {
            //Get destination register
            var destReg = Registers[source];

            if (destReg != null)
            {
                int _value;
                Int32.TryParse(constant, out _value);

                if (IsValidValue(_value, _max16BitValue))
                {
                    destReg.DataValue = _value;
                }
            }
            Console.WriteLine($"Registers used: \t{destReg.Name}");
            Console.WriteLine($"Register value: \t{destReg.DataValue}");
            Console.WriteLine($"Binary Instruction: \t{Instructions["cli"].Value}{Registers[source].BitValue}");
        }

        private void AddTwoNumbers(Register destination, int first, int second)
        {

        }

        public Register ConvertToRegister(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (Registers.ContainsKey(source))
                {
                    return Registers[source];
                }                
            }
            throw new Exception("Source string is null or empty");            
        }

        public Instruction ConvertToInstruction(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (Instructions.ContainsKey(source))
                {
                    return Instructions[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }

        private bool IsValidValue(int value, int limit)
        {
            return value <= limit;
        }

    }
}
