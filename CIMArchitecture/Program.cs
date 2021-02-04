using System;
using System.Collections.Generic;

namespace CIMArchitecture
{
    class Program
    {
        static void Main(string[] args)
        {
            CIMFactory factory = new CIMFactory();

            int userSelection = DisplayMenu();

            if (userSelection != 4) 
            {
                bool runProg = true;

                while (runProg) 
                {
                    int selection = userSelection;
                    switch (selection)
                    {
                        case 1:
                            IDE(factory.UserInputInstructions);
                            break;
                        default: throw new Exception("Invalid selection");
                    }
                    selection = DisplayMenu();

                    if (selection == 4) 
                    {
                        runProg = false;
                    }
                }
            }

            //Display user instructions if any: TEST
            foreach (var instruction in factory.UserInputInstructions) 
            {
                Console.WriteLine($"{instruction}");
            }
        }

        private static int DisplayMenu() 
        {
            Console.WriteLine("1. IDE");
            Console.WriteLine("4. Exit");

            return Int32.Parse(Console.ReadLine());
        }

        private static List<string> IDE(List<string> instructionContainer) 
        {
            string key = "run";
            while (true) 
            {
                Console.WriteLine("Enter instructions:");
                string instruction = Console.ReadLine();

                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase)) 
                {
                    break;
                }
                instructionContainer.Add(instruction);
            }
            return instructionContainer;
        }
    }
}
