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
        private Dictionary<string, Instruction> _availableInstructions;
        private Dictionary<string, Register> _availableRegisters;
        private List<string> _userInput;

        public CIMCompiler(Dictionary<string, Instruction> instructions, Dictionary<string, Register> registers, List<string> commands) 
        {
            _userInput = commands;
            _availableInstructions = instructions;
            ReadCommands(_userInput);
        }

        public void ReadCommands(List<string> userInput)
        {
            var commands = SplitCommands(userInput);

            ProcessCommands(commands);
        }

        private void ProcessCommands(List<List<string>> commands)
        {
            foreach (var command in commands)
            {
                ProcessCommand(command);
            }
        }

        private void ProcessCommand(List<string> command) 
        {
            //Get instruction for method call
            var instruction = _availableInstructions[command[0]];

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

        private List<List<string>> SplitCommands(List<string> commands) 
        {
            List<List<string>> parsedCommands = new List<List<string>>();

            foreach (var cmd in commands) 
            {
                parsedCommands.Add(cmd.Split(' ').ToList());
            }
            return parsedCommands;
        }

        
        private void LoadImmediate(string destination, string value) 
        {
            //Get destination register
            var destReg = _availableRegisters[destination];

            int numValue;
            var IsNumber = Int32.TryParse(value, out numValue);

            if (numValue > 65536) 
            {
                //throw error
            }
        }

        private void AddTwoNumbers(Register destination, int first, int second) 
        {
        
        }


    }
}
