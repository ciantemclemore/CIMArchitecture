using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CIMArchitecture
{
    /// <summary>
    /// The CIMFactory is responsible for gathering and storing instruction data.
    /// </summary>
    class CIMFactory
    {
        public List<Instruction> CIMInstructions { get; }

        public List<string> UserInputInstructions { get; set; } = new List<string>();

        public CIMFactory() 
        {
            CIMInstructions = GetInstructionsFromJson();
        }

        public List<Instruction> GetInstructionsFromJson() 
        {
            var instructions = new List<Instruction>();
            try
            {
                using (StreamReader sr = new StreamReader("Database/InstructionDatabase.json"))
                {
                    instructions = JsonSerializer.Deserialize<List<Instruction>>(sr.ReadToEnd());
                }
            }
            catch (IOException e) 
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return instructions;
        }
    }
}
