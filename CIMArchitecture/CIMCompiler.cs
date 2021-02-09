using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace CIMArchitecture
{
    /// <summary>
    /// CIMCompiler is responsible for decoding instructions
    /// </summary>
    class CIMCompiler
    {
        private readonly CIMFactory _cimFactory;

        private readonly List<string> _commands = new List<string>();

        private List<Register> UsedRegisters { get; set; } = new List<Register>();

        private List<Instruction> UsedInstructions { get; set; } = new List<Instruction>();

        private List<string> FullBinaryInstruction { get; set; } = new List<string>();

        public const int _max16BitValue = 65536;

        public const int _max8BitValue = 256;

        public CIMCompiler(CIMFactory factory, List<string> commands) 
        {
            _cimFactory = factory;
            _commands = commands;
            ExecuteCommands(_commands, _cimFactory);
        }

        public void ExecuteCommands(List<string> userCommands, CIMFactory factory)
        {
            var tokens = SplitCommandsIntoTokens(userCommands);

            ProcessCommands(tokens, factory);
            PrintState(factory);
            PrintResults();

            ClearData();
        }

        private void ProcessCommands(List<List<string>> tokenLists, CIMFactory factory)
        {
            foreach (var tokenList in tokenLists)
            {
                ProcessCommand(tokenList, factory);
            }
        }

        private void ProcessCommand(List<string> tokenList, CIMFactory factory) 
        {
            //Get instruction for method call
            var instruction = factory.Instructions[tokenList[0]];
            
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
            MethodInfo method = typeof(CIMFactory).GetMethod(instruction.FunctionName);

            if (method != null) 
            {
                //Call the method in the factory and provide its parameters
                FullBinaryInstruction.Add((string)method.Invoke(factory, parameters));
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

        public static void PrintState(CIMFactory factory) 
        {
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,14}", "CIM State");
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,-12} {1,-8}", "Registers", "Value");
            foreach (var kvp in factory.Registers) 
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
            _commands.Clear();
        }

        public static bool IsValidValue(int value, int limit)
        {
            return value <= limit;
        }

        public Register ConvertToRegister(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_cimFactory.Registers.ContainsKey(source))
                {
                    return _cimFactory.Registers[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }

        public Instruction ConvertToInstruction(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (_cimFactory.Instructions.ContainsKey(source))
                {
                    return _cimFactory.Instructions[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }
    }
}
