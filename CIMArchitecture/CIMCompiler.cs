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
        private CIMFactory factory;
        private List<string> commands = new List<string>();
        public const int _max16BitValue = 65536;
        public const int _max8BitValue = 256;

        public CIMCompiler(CIMFactory _factory, List<string> _commands) 
        {
            factory = _factory;
            commands = _commands;
            ExecuteCommands(commands);
        }

        public void ExecuteCommands(List<string> commands)
        {
            var tokens = SplitCommandsIntoTokens(commands);

            ProcessCommands(tokens, factory);

            factory.PrintResults();

            ClearData();
        }

        private void ProcessCommands(List<List<string>> commands, CIMFactory _factory)
        {
            foreach (var command in commands)
            {
                ProcessCommand(command, _factory);
            }
        }

        private void ProcessCommand(List<string> command, CIMFactory _factory) 
        {
            //Get instruction for method call
            var instruction = _factory.Instructions[command[0]];

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

            if (method != null) 
            {
                //Call the method in the factory and provide its parameters
                method.Invoke(_factory, parameters);
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

        private void ClearData() 
        {
            commands.Clear();
        }

        public static bool IsValidValue(int value, int limit)
        {
            return value <= limit;
        }

        public Register ConvertToRegister(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (factory.Registers.ContainsKey(source))
                {
                    return factory.Registers[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }

        public Instruction ConvertToInstruction(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (factory.Instructions.ContainsKey(source))
                {
                    return factory.Instructions[source];
                }
            }
            throw new Exception("Source string is null or empty");
        }
    }
}
