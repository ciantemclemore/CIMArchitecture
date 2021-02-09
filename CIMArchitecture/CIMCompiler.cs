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

        private List<string> _commands { get; set; } = new List<string>();

        private Dictionary<string, Instruction> _resultData = new Dictionary<string, Instruction>();

        private const int _max16BitValue = 65536;

        private const int _max8BitValue = 256;

        public CIMCompiler(Configuration config) 
        {
            _configuration = config;
        }



        public Dictionary<string, Instruction> ExecuteCommands()
        {
            var tokens = SplitCommandsIntoTokens(_commands);

            ProcessCommands(tokens);

            var results = new Dictionary<string, Instruction>(_resultData);
            
            ClearData();
            
            return results;
        }

        private void ProcessCommands(List<List<string>> tokenLists)
        {
            foreach (var tokenList in tokenLists)
            {
                ProcessCommand(tokenList);
            }
        }

        private void ProcessCommand(List<string> tokenList) 
        {
            //Get instruction for method call
            var instruction = _configuration.Instructions[tokenList[0]];

            //Store the rest as parameters for the function call
            var parameters = new object[tokenList.Count];

            //Get remaining parameters used
            for (int i = 0; i < tokenList.Count; i++) 
            {
                parameters[i] = tokenList[i];
            }

            //Get the method from the CIMFactory
            MethodInfo method = typeof(CIMCompiler).GetMethod(instruction.FunctionName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (method != null) 
            {
                //Call the method in the factory and provide its parameters
                var binaryInstruction = (string)method.Invoke(this, parameters);

                _resultData.Add(binaryInstruction, instruction);
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

        private List<string> SplitCommandsIntoTokens(string command)
        {
            return command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public bool IsValidCommand(string command) 
        {
            var tokens = SplitCommandsIntoTokens(command);

            //Check the instruction first before going over parameters
            var instruction = tokens[0].ToInstruction(_configuration);

            //If its not a valid instruction, its not a valid command
            if (instruction == null) return false;

            //Check to see which format the command falls under, and check for validity
            if (instruction.Format.Equals("C")) return IsValidCFormat(tokens);
            else if (instruction.Format.Equals("I")) return IsValidIFormat(tokens);
            else return IsValidMFormat(tokens);
        }

        private bool IsValidCFormat(List<string> parameters) 
       {
            //C-Formats have immediate versions as well. Check to see if
            //the user has provided the right arguments based on instruction name
            var instruction = parameters[0].ToInstruction(_configuration);
            var lastParameter = parameters[parameters.Count - 1].ToRegister(_configuration);

            if (!instruction.Name.EndsWith('i') && lastParameter == null) return false;
            if (instruction.Name.EndsWith('i') && lastParameter != null) return false;

            for (int i = 1; i < parameters.Count ; i++) 
            {
                var parameter = parameters[i].ToRegister(_configuration);

                if (i == parameters.Count - 1 && parameter == null) 
                {
                    return (IsValidValue(Int32.Parse(parameters[i]), _max8BitValue));
                }

                if (parameter == null) return false;
            }
            return true;
        }

        private bool IsValidIFormat(List<string> parameters)
        {
            //Odd case, couldn't think of a convention to check for these two single instructions
            const string x = "cdeleteall";
            const string y = "cclear";

            if (parameters[0].Equals(x) && parameters.Count > 1 || parameters[0].Equals(y) && parameters.Count > 1)
            {
                return false;
            }

            if (parameters.Count > 1 && parameters[1].ToRegister(_configuration) == null)
            {
                return IsValidValue(Int32.Parse(parameters[1]), _max16BitValue);
            }
            return true;
        }

        private bool IsValidMFormat(List<string> parameters)
        {
            var destinationRegister = parameters[1].ToRegister(_configuration);
            var secondParameter = parameters[2].ToRegister(_configuration);

            //Here we check to see if the first parameter is a valid register
            //If first parameter is valid, we check to see if the second parameter is valid
            //Second parameter must be a valid immediate value or register
            if (destinationRegister == null) return false;

            if (secondParameter == null)
            {
                return IsValidValue(Int32.Parse(parameters[2]), _max16BitValue);
            }
            return true;
        }

        public void QueueCommand(string command)  
        {
            _commands.Add(command);
        }

        private void ClearData() 
        {
            _commands.Clear();
            _resultData.Clear();
        }

        private bool IsValidValue(int value, int limit)
        {
            return value <= limit;
        }

        #region C-Format Instruction Methods
        private string Add(string instructionName, string source, string first, string second) 
        {
            var instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];
            var firstReg = _configuration.Registers[first];
            var secondReg = _configuration.Registers[second];
            destReg.DataValue = (firstReg.DataValue + secondReg.DataValue);

            return $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }

        private string AddImmediate(string instructionName, string source, string first, string second) 
        {
            var instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];
            var firstReg = _configuration.Registers[first];
            var immediate = Int32.Parse(second);
            destReg.DataValue = immediate;

            return $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }

        private string Subtract(string instructionName, string source, string first, string second) 
        {
            var instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];
            var firstReg = _configuration.Registers[first];
            var secondReg = _configuration.Registers[second];
            destReg.DataValue = (firstReg.DataValue - secondReg.DataValue);

            return $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }

        private string SubtractImmediate(string instructionName, string source, string first, string second) 
        {
            var instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];
            var firstReg = _configuration.Registers[first];
            var immediate = Int32.Parse(second);
            destReg.DataValue = (firstReg.DataValue - immediate);

            return $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }
        #endregion

        #region I-Format Instruction Methods
        private string DeleteAll(string instructionName) 
        {
            var instruction = _configuration.Instructions[instructionName];

            //Delete all values out of registers
            foreach (var reg in _configuration.Registers) 
            {
                reg.Value.DataValue = 0;
            }
            return $"{0.ToBinary(24)}{instruction.Value.ToBinary(8)}";
        }
        #endregion

        #region M-Format Instruction Methods
        private string LoadImmediate(string instructionName, string source, string first)
        {
            //Get destination register and instruction
            var instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];

            //determine if 'first' is a register or immediate
            Register _firstReg = first.ToRegister(_configuration);
            string binString = string.Empty;

            //If it is not a register, it is an immediate value
            if (_firstReg == null)
            {
                var value = Int32.Parse(first);
                destReg.DataValue = value;
                binString = value.ToBinary(16);
            }
            else
            {
                destReg.DataValue = _firstReg.DataValue;
                binString = _firstReg.BitValue.ToBinary(16);
            }
            return $"{binString}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
        }
        #endregion
    }
}
