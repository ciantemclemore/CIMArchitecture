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

        private List<Result> _results = new List<Result>();

        private const int _max16BitValue = 65536;

        private const int _max8BitValue = 256;

        public CIMCompiler(Configuration config)
        {
            _configuration = config;
        }



        public List<Result> ExecuteCommands()
        {
            var tokens = SplitCommandsIntoTokens(_commands);

            ProcessCommands(tokens);

            var results = new List<Result>(_results);

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
                var result = (Result)method.Invoke(this, parameters);

                _results.Add(result);
                //_resultData.Add(binaryInstruction, instruction);
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

        public CommandValidation IsValidCommand(string command)
        {
            var tokens = SplitCommandsIntoTokens(command);

            //Check the instruction first before going over parameters
            var instruction = tokens[0].ToInstruction(_configuration);

            //If its not a valid instruction, its not a valid command
            if (instruction == null) return new CommandValidation() { IsValid = false, Message = "Invalid Instruction. Try again:" };

            //Check to see which format the command falls under, and check for validity
            if (instruction.Format.Equals("C")) return IsValidCFormat(tokens);
            else if (instruction.Format.Equals("I")) return IsValidIFormat(tokens);
            else return IsValidMFormat(tokens);
        }

        private CommandValidation IsValidCFormat(List<string> parameters)
        {
            //C-Formats have immediate versions as well. Check to see if
            //the user has provided the right arguments based on instruction name
            var instruction = parameters[0].ToInstruction(_configuration);
            var lastParameter = parameters[parameters.Count - 1].ToRegister(_configuration);

            if (!instruction.Name.EndsWith('i') && lastParameter == null)
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };

            if (instruction.Name.EndsWith('i') && lastParameter != null)
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };

            for (int i = 1; i < parameters.Count; i++)
            {
                var parameter = parameters[i].ToRegister(_configuration);

                if (i == parameters.Count - 1 && parameter == null)
                {
                    var cmdValidation = new CommandValidation();
                    cmdValidation.IsValid = IsValidValue(Int32.Parse(parameters[i]), _max8BitValue);
                    cmdValidation.Message = cmdValidation.IsValid ? string.Empty : "Value not supported. Try again:";
                    return cmdValidation;
                }

                if (parameter == null)
                    return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };
            }
            return new CommandValidation();
        }

        private CommandValidation IsValidIFormat(List<string> parameters)
        {
            //Odd case, couldn't think of a convention to check for these two single instructions
            const string x = "cdeleteall";
            const string y = "cclear";

            if (parameters[0].Equals(x) && parameters.Count > 1 || parameters[0].Equals(y) && parameters.Count > 1)
            {
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };
            }

            if ((!parameters[0].Equals(x)) && parameters.Count < 2 || (!parameters[0].Equals(y)) && parameters.Count < 2) 
            {
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };
            }

            if (parameters.Count > 2) return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };

            if (parameters.Count > 1 && parameters[1].ToRegister(_configuration) == null)
            {
                var cmdValidation = new CommandValidation();
                cmdValidation.IsValid = IsValidValue(Int32.Parse(parameters[1]), _max16BitValue);
                cmdValidation.Message = cmdValidation.IsValid ? string.Empty : "Value not supported. Try again:";
                return cmdValidation;
            }
            return new CommandValidation();
        }

        private CommandValidation IsValidMFormat(List<string> parameters)
        {
            var instruction = parameters[0].ToInstruction(_configuration);
            var destinationRegister = parameters[1].ToRegister(_configuration);
            var lastParameter = parameters[parameters.Count - 1].ToRegister(_configuration);

            //M-Formats have immediate versions as well. Check to see if
            //the user has provided the right arguments based on instruction name
            if (!instruction.Name.EndsWith('i') && lastParameter == null)
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };

            if (instruction.Name.EndsWith('i') && lastParameter != null)
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" };

            //Here we check to see if the first parameter is a valid register
            //If first parameter is valid, we check to see if the second parameter is valid
            //Second parameter must be a valid immediate value or register
            if (destinationRegister == null)
                return new CommandValidation() { IsValid = false, Message = "Incorrect instruction format. Try again:" }; ;

            if (lastParameter == null)
            {
                var cmdValidation = new CommandValidation();
                cmdValidation.IsValid = IsValidValue(Int32.Parse(parameters[2]), _max16BitValue);
                cmdValidation.Message = cmdValidation.IsValid ? string.Empty : "Value not supported. Try again:";
                return cmdValidation;
            }
            return new CommandValidation();
        }

        public void QueueCommand(string command)
        {
            _commands.Add(command);
        }

        private void ClearData()
        {
            _commands.Clear();
            _results.Clear();
        }

        private bool IsValidValue(int value, int limit)
        {
            return Math.Abs(value) <= limit;
        }

        //Need a way to return message to the user if command values are outside the supported range value
        //Max-range is 2^16 = 65k
        #region C-Format Instruction Methods
        private Result Add(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue + secondReg.DataValue, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue + secondReg.DataValue : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result AddImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue + immediate, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue + immediate : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Subtract(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue - secondReg.DataValue, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue - secondReg.DataValue : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result SubtractImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue - immediate, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue - immediate : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Multiple(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            var destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue * secondReg.DataValue, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue * secondReg.DataValue : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result MultiplyImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = IsValidValue(firstReg.DataValue * immediate, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue * immediate : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Divide(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = (secondReg.DataValue != 0) && IsValidValue(firstReg.DataValue / secondReg.DataValue, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue / secondReg.DataValue : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result DivideImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            bool isValid = (immediate != 0) && IsValidValue(firstReg.DataValue / immediate, _max16BitValue);
            destReg.DataValue = isValid ? firstReg.DataValue / immediate : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Power(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = firstReg.DataValue;

            for (int i = 0; i < secondReg.DataValue; i++)
            {
                value *= value;
            }

            bool isValid = IsValidValue(value, _max16BitValue);
            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result PowerImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = firstReg.DataValue;

            for (int i = 0; i < immediate; i++)
            {
                value *= value;
            }

            bool isValid = IsValidValue(value, _max16BitValue);
            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Modulo(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = Math.Abs(firstReg.DataValue % secondReg.DataValue);

            bool isValid = IsValidValue(value, _max16BitValue);
            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result ModuloImmediate(string instructionName, string source, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            int immediate = Int32.Parse(second);
            string binString = $"{immediate.ToBinary(8)}{firstReg.BitValue.ToBinary(8)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = Math.Abs(firstReg.DataValue % immediate);

            bool isValid = IsValidValue(value, _max16BitValue);
            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        #endregion

        #region I-Format Instruction Methods
        private Result DeleteAll(string instructionName)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            string binString = $"{0.ToBinary(24)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            //Delete all values out of registers
            foreach (var reg in _configuration.Registers)
            {
                reg.Value.DataValue = 0;
            }
            return result;
        }

        private Result Delete(string instructionName, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register firstReg = _configuration.Registers[first];
            string binString = $"{firstReg.BitValue.ToBinary(24)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            //Set value back to default
            firstReg.DataValue = 0;

            return result;
        }

        private Result CreateMemory(string instructionName, string first) 
        {

            return new Result();    
        }
        #endregion

        #region M-Format Instruction Methods
        private Result LoadImmediate(string instructionName, string source, string first)
        {
            //Get destination register and instruction
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            int immediate = int.Parse(first);
            string binString = $"{immediate.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            bool isValid;

            isValid = IsValidValue(immediate, _max16BitValue);
            destReg.DataValue = isValid ? immediate : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Square(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            string binString = $"{firstReg.BitValue.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = firstReg.DataValue * firstReg.DataValue;
            bool isValid = IsValidValue(value, _max16BitValue);

            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result SquareImmediate(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            int immediate = Int32.Parse(first);
            string binString = $"{immediate.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = immediate * immediate;
            bool isValid = IsValidValue(value, _max16BitValue);

            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Cube(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            string binString = $"{firstReg.BitValue.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = firstReg.DataValue * firstReg.DataValue * firstReg.DataValue;
            bool isValid = IsValidValue(value, _max16BitValue);

            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result CubeImmediate(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            int immediate = Int32.Parse(first);
            string binString = $"{immediate.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };
            int value = immediate * immediate * immediate;
            bool isValid = IsValidValue(value, _max16BitValue);

            destReg.DataValue = isValid ? value : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }
        private Result Factorial(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            Register firstReg = _configuration.Registers[first];
            string binString = $"{firstReg.BitValue.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            int total = 1;
            var temp = firstReg.DataValue;
            bool isValid;

            //Simple factorial function
            while (temp > 0 && temp != 1)
            {
                total = total * temp;
                temp -= 1;
            }

            isValid = IsValidValue(total, _max16BitValue);
            destReg.DataValue = isValid ? total : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result FactorialImmediate(string instructionName, string source, string first)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register destReg = _configuration.Registers[source];
            int immediate = Int32.Parse(first);
            string binString = $"{immediate.ToBinary(16)}{destReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            int total = 1;
            var temp = immediate;
            bool isValid;

            //Simple factorial function
            while (temp > 0 && temp != 1)
            {
                total = total * temp;
                temp -= 1;
            }

            isValid = IsValidValue(total, _max16BitValue);
            destReg.DataValue = isValid ? total : destReg.DataValue;
            result.IsError = !isValid;

            return result;
        }

        private Result Move(string instructionName, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(16)}{firstReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            //Move second register value into first. Kind of like a load immediate register
            firstReg.DataValue = secondReg.DataValue;

            return result;
        }

        private Result Swap(string instructionName, string first, string second)
        {
            Instruction instruction = _configuration.Instructions[instructionName];
            Register firstReg = _configuration.Registers[first];
            Register secondReg = _configuration.Registers[second];
            string binString = $"{secondReg.BitValue.ToBinary(16)}{firstReg.BitValue.ToBinary(8)}{instruction.Value.ToBinary(8)}";
            Result result = new Result() { Instruction = instruction, BinaryString = binString };

            //Swap the two register values
            var temp = firstReg.DataValue;
            firstReg.DataValue = secondReg.DataValue;
            secondReg.DataValue = temp;

            return result;
        }
        #endregion
    }
}
