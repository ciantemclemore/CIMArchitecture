using CIMArchitecture.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Linq;
using MoreLinq;

namespace CIMArchitecture
{
    class Program
    {
        static void Main(string[] args)
        {
            //Increase window size by 25%
            Console.SetWindowSize(Convert.ToInt32(Console.WindowWidth * 1.25), Convert.ToInt32(Console.WindowHeight * 1.25));

            var configuration = GetConfigurationData(@"Database\Configuration_Min.json");
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
                var commandValidation = compiler.IsValidCommand(instruction);

                //If user enters 'run' command, its time to execute all commands, if any
                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase)) break;

                if (commandValidation.IsValid)
                {
                    //if the user enters a valid command, add to queue for execution
                    compiler.QueueCommand(instruction);
                }
                else 
                {
                    Console.WriteLine(commandValidation.Message);
                }
            }
        }

        private static void PrintState(Configuration config)
        {
            Console.WriteLine("\t\t\t\t\t\t\t\t\t\t{0,24}","CIM State");
            Console.WriteLine("\t\t\t\t\t\t\t\t\t\t-----------------------------------------");
            Console.WriteLine("\t\t\t\t\t\t\t\t\t\t{0,-20}", "Memory Storage");
            Console.Write("\t\t\t\t\t\t\t\t\t\t");

            //Print Memory Table
            if (Memory.GetCount() > 0)
            {
                var rowCount = (Memory.GetCount() / 7) + 1;
                var colCount = 7;
                var memIndex = 0;
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        if (memIndex < Memory.GetCount())
                        {
                            Console.Write("{0} ", Memory.AsArray()[memIndex]);
                            memIndex++;
                        }
                    }
                    Console.WriteLine();
                    Console.Write("\t\t\t\t\t\t\t\t\t\t");

                }
            }
            else 
            {
                Console.Write("Empty");
            }

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("\t\t\t\t\t\t\t\t\t\t{0,-20} {1}", "Registers", "Values");
            foreach (var kvp in config.Registers)
            {
                Console.WriteLine("\t\t\t\t\t\t\t\t\t\t{0,-20} {1}", kvp.Value.Name, kvp.Value.DataValue);
            }
        }

        public static void PrintResults(List<Result> results)
        {
            Console.WriteLine();

            //Header
            Console.WriteLine("{0, -14} {1,-16} {2, -29} {3,-18}", "Name", "OPCODE", "Error", "32-Bit Instruction");

            int i = 0;
            foreach (var result in results)
            {
                //Fix the spacing... opcodes are 8 characters so need to compensate
                Console.WriteLine("{0,-14} {1,-16} {2, -29} {3,-18}", result.Instruction.Name, result.Instruction.Value, result.ErrorMessage, result.BinaryString);
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
