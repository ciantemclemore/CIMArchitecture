using CIMArchitecture.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CIMArchitecture
{
    /// <summary>
    /// CIMCompiler is responsible for decoding instructions
    /// </summary>
    class CIMCompiler
    {
        private readonly Configuration _configuration;

        private readonly List<string> _userCommands = new List<string>();

        private List<Instruction> UsedInstructions { get; set; } = new List<Instruction>();

        private List<string> FullBinaryInstruction { get; set; } = new List<string>();

        public const int _max16BitValue = 65536;

        public const int _max8BitValue = 256;

        public CIMCompiler(Configuration config) 
        {
            _configuration = config;
            _userCommands = GatherUserInstructionCommands(config);
            ExecuteCommands(_userCommands, config);
        }

        private List<string> GatherUserInstructionCommands(Configuration config)
        {
            string key = "run";
            var userInput = new List<string>();

            Console.WriteLine();
            PrintState(config);
            Console.WriteLine("Enter instructions: (Press enter after entering each instruction)");
            Console.WriteLine("Enter 'RUN' command once finished to compile");


            while (true)
            {

                string instruction = Console.ReadLine();

                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                userInput.Add(instruction);
            }
            return userInput;
        }

        public void ExecuteCommands(List<string> userCommands, Configuration config)
        {
            var tokens = SplitCommandsIntoTokens(userCommands);

            ProcessCommands(tokens, config);
            PrintState(config);
            PrintResults();
        }

        private void ProcessCommands(List<List<string>> tokenLists, Configuration factory)
        {
            foreach (var tokenList in tokenLists)
            {
                ProcessCommand(tokenList, factory);
            }
        }

        private void ProcessCommand(List<string> tokenList, Configuration config) 
        {
            //Get instruction for method call
            var instruction = config.Instructions[tokenList[0]];
            
            //Needs to check if valid instruction
            UsedInstructions.Add(instruction);

            //Store the rest as parameters for the function call
            var parameters = new object[tokenList.Count];
            parameters[0] = instruction.Name;

            //Get remaining parameters used
            for (int i = 1; i < tokenList.Count; i++) 
            {
                parameters[i] = tokenList[i];
            }

            //Get the method from the CIMFactory
            MethodInfo method = typeof(CIMCompiler).GetMethod(instruction.FunctionName);

            if (method != null) 
            {
                //Call the method in the factory and provide its parameters
                var binaryInstruction = (string)method.Invoke(this, parameters);
                FullBinaryInstruction.Add(binaryInstruction);
            }
        }

        private List<List<string>> SplitCommandsIntoTokens(List<string> commands) 
        {
            List<List<string>> parsedCommands = new List<List<string>>();

            foreach (var cmd in commands) 
            {
                parsedCommands.Add(cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());
            }
            return parsedCommands;
        }

        private List<Register> ParseRegisters(List<string> tokens) 
        {
            var regList = new List<Register>();

            foreach (var register in tokens) 
            {
                if (register.StartsWith('c')) 
                {
                    regList.Add(ConvertToRegister(register));
                }
            }
            return regList;
        }

        public void PrintState(Configuration config) 
        {
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,14}", "CIM State");
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,-12} {1,-8}", "Registers", "Value");
            foreach (var kvp in config.Registers) 
            {
                Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,-12} {1,-8}", kvp.Value.Name, kvp.Value.DataValue);
            }
        }
        public void PrintResults()
        {
            Console.WriteLine();

            //Header
            Console.WriteLine("{0, -14} {1,-16} {2,-18}", "Name", "OPCODE", "32-Bit Instruction");

            int i = 0;
            foreach (var instr in UsedInstructions) 
            {
                //Fix the spacing... opcodes are 8 characters so need to compensate
                Console.WriteLine("{0,-14} {1,-16} {2,-18}", instr.Name, instr.Value, FullBinaryInstruction[i]);
                i++;

            }
            ClearData();
            Console.WriteLine();
        }

        private void ClearData() 
        {
            _userCommands.Clear();
            UsedInstructions.Clear();
            FullBinaryInstruction.Clear();
        }

        public static bool IsValidValue(int value, int limit)
        {
            return value <= limit;
        }

        public Register ConvertToRegister(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_configuration.Registers.ContainsKey(source))
                {
                    return _configuration.Registers[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }

        public Instruction ConvertToInstruction(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_configuration.Instructions.ContainsKey(source))
                {
                    return _configuration.Instructions[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }

        #region Instruction Methods
        public string LoadImmediate(string instructionName, string source, string constant)
        {
            //Get destination register and instruction
            var destReg = _configuration.Registers[source];
            var instruction = _configuration.Instructions[instructionName];

            if (destReg != null && instruction != null)
            {
                int _value;
                Int32.TryParse(constant, out _value);

                if (CIMCompiler.IsValidValue(_value, CIMCompiler._max16BitValue))
                {
                    destReg.DataValue += _value;
                }
            }
            return $"{destReg.DataValue.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }
        #endregion

    }
}
