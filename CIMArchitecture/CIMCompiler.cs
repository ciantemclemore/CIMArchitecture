using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CIMArchitecture
{
    /// <summary>
    /// CIMCompiler is responsible for decoding instructions
    /// </summary>
    class CIMCompiler
    {
        private List<Instruction> _availableInstructions;
        private List<string> _commands;

        public CIMCompiler(List<Instruction> instructions, List<string> commands) 
        {
            _commands = commands;
            _availableInstructions = instructions;
            ReadCommands(_commands);
        }

        public void ReadCommands(List<string> commands) 
        {
            var instructions = SplitCommands(commands);

            foreach(var instr in instructions) 
            {
                var count = 1;
                Console.WriteLine($"Instruction {count}");
                foreach (var cmd in instr) 
                {
                    Console.WriteLine($"\t{cmd}");    
                }
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
    }
}
