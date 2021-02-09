using CIMArchitecture.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace CIMArchitecture
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = GetConfigurationData(@"Database\Configuration.json");
            var compiler = new CIMCompiler(configuration);

            bool runProg = true;

            while (runProg)
            {
                int userSelection = DisplayMenu();

                switch (userSelection)
                {
                    case 1:
                        //Gather the user's commands
                        GatherUserInstructionCommands(configuration, compiler);
                        var results = compiler.ExecuteCommands();
                        PrintState(configuration);
                        PrintResults(results);
                        break;
                    case 4:
                        runProg = false;
                        break;
                    default: Console.WriteLine("Invalid selection. Try Again:");
                        break;
                }
            }
            
        }

        private static int DisplayMenu() 
        {
            Console.WriteLine("1. IDE");
            Console.WriteLine("4. Exit");
            
            int selectionValue;
            var isValid = Int32.TryParse(Console.ReadLine(), out selectionValue);

            if (isValid) 
            {
                return selectionValue;
            }

            //Just return an invalid selction to display menu again
            Console.WriteLine();
            return 5;
        }

        private static void GatherUserInstructionCommands(Configuration config, CIMCompiler compiler)
        {
            string key = "run";

            Console.WriteLine();
            PrintState(config);
            Console.WriteLine("Enter instructions: (Press enter after entering each instruction)");
            Console.WriteLine("Enter 'RUN' command once finished to compile");

            while (true)
            {
                string instruction = Console.ReadLine();
                var isValid = compiler.IsValidCommand(instruction);

                //If user enters 'run' command, its time to execute all commands, if any
                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase)) break;

                if (isValid)
                {
                    //if the user enters a valid command, add to queue for execution
                    compiler.QueueCommand(instruction);
                }
                else 
                {
                    Console.WriteLine("Invalid instruction, register, or format. Enter command again:");
                }
            }
        }

        private static void PrintState(Configuration config)
        {
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,14}", "CIM State");
            Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,-12} {1,-8}", "Registers", "Value");
            foreach (var kvp in config.Registers)
            {
                Console.WriteLine("\t\t\t\t\t\t\t\t\t{0,-12} {1,-8}", kvp.Value.Name, kvp.Value.DataValue);
            }
        }

        public static void PrintResults(Dictionary<string, Instruction> results)
        {
            Console.WriteLine();

            //Header
            Console.WriteLine("{0, -14} {1,-16} {2,-18}", "Name", "OPCODE", "32-Bit Instruction");

            int i = 0;
            foreach (var kvp in results)
            {
                //Fix the spacing... opcodes are 8 characters so need to compensate
                Console.WriteLine("{0,-14} {1,-16} {2,-18}", kvp.Value.Name, kvp.Value.Value, kvp.Key);
                i++;

            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private static Configuration GetConfigurationData(string path) 
        {
            var jsonData = string.Empty;
            var configuration = new Configuration();
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    jsonData = sr.ReadToEnd();
                }
                configuration = JsonSerializer.Deserialize<Configuration>(jsonData);

            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return configuration;
        }
    }
}
