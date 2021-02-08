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

            ClearData();
        }

        private void ProcessCommands(List<List<string>> commands, CIMFactory factory)
        {
            foreach (var command in commands)
            {
                var instruction = ProcessCommand(command, factory);
                factory.PrintResults(instruction);
            }
        }

        private string ProcessCommand(List<string> command, CIMFactory factory) 
        {
            //Get instruction for method call
            var instruction = factory.Instructions[command[0]];

            //Store the rest as parameters for the function call
            var parameters = new object[command.Count];
            parameters[0] = instruction.Name;

            //Get remaining parameters used
            for (int i = 1; i < command.Count; i++) 
            {
                parameters[i] = command[i];
            }

            //Get the method from the CIMFactory
            MethodInfo method = typeof(CIMFactory).GetMethod(instruction.FunctionName);

            //we want to get the full binary instruction from the method
            string binaryInstruction = null;

            if (method != null) 
            {
                //Call the method in the factory and provide its parameters
                binaryInstruction = (string)method.Invoke(factory, parameters);
            }
            return binaryInstruction;
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
