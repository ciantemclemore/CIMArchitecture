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

        public CIMCompiler(CIMFactory _factory) 
        {
            factory = _factory;
            ReadCommands(factory.UserInput);
        }

        public void ReadCommands(List<string> userInput)
        {
            var tokens = SplitCommandsIntoTokens(userInput);

            ProcessCommands(tokens, factory);
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
            var parameters = new List<string>();

            //Get remaining parameters used
            int j = 0;
            for (int i = 1; i < command.Count; i++) 
            {
                parameters[j] = command[i];
                j++;
            }

            MethodInfo method = typeof(CIMCompiler).GetMethod(instruction.FunctionName);

            if (method != null) 
            {
                method.Invoke(this, new object[] { parameters });
            }
        }

        private List<List<string>> SplitCommandsIntoTokens(List<string> commands) 
        {
            List<List<string>> parsedCommands = new List<List<string>>();

            foreach (var cmd in commands) 
            {
                parsedCommands.Add(cmd.Split(' ').ToList());
            }
            return parsedCommands;
        }


    }
}
