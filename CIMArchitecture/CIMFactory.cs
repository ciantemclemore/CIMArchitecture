using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CIMArchitecture
{
    /// <summary>
    /// The CIMFactory is responsible for gathering, storing, and providing instruction data.
    /// </summary>
    class CIMFactory
    {
        public Dictionary<string, Instruction> Instructions { get; } = new Dictionary<string, Instruction>();

        public Dictionary<string, Register> Registers { get; } = new Dictionary<string, Register>();

        private List<Register> UsedRegisters { get; set; } = new List<Register>();

        private List<Instruction> UsedInstructions { get; set; } = new List<Instruction>();

        private const string _instructionPath = "Database/InstructionDatabase.json";
        private const string _registerPath = "Database/RegisterDatabase.json";
        

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

        public void PrintResults()
        {
            //Header
            Console.WriteLine("{0, -15} {1,-12} {2,-11} {3,-9}", "Instructions", "Registers", "Contents", "Binary");

            //Now print table results
            foreach (var instr in UsedInstructions)
            {
                foreach (var reg in UsedRegisters)
                {
                    Console.WriteLine("{0,-15} {1,-12} {2,-11} {3,-9}", instr.Name, reg.Name, reg.DataValue, reg.BitValue);
                }
            }

            ClearData();
        }

        private void ClearData()
        {
            UsedRegisters.Clear();
            UsedInstructions.Clear();
        }

        #region Instruction Methods

        public void LoadImmediate(string instructionName, string source, string constant)
        {
            //Get destination register and instruction
            var destReg = Registers[source];
            var instruction = Instructions[instructionName];

            //Store used registers and instruction for printing results
            if (!UsedRegisters.Contains(destReg)) 
            {
                UsedRegisters.Add(destReg);
            }
            if (!UsedInstructions.Contains(instruction)) 
            {
                UsedInstructions.Add(instruction);
            }

            if (destReg != null)
            {
                int _value;
                Int32.TryParse(constant, out _value);

                if (CIMCompiler.IsValidValue(_value, CIMCompiler._max16BitValue))
                {
                    destReg.DataValue += _value;
                }
            }
        }

        private void AddTwoNumbers(Register destination, int first, int second)
        {

        }
        #endregion
    }
}
