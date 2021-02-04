using System;
using System.Collections.Generic;

namespace CIMArchitecture
{
    class Program
    {
        public static List<string> InstructionContainer = new List<string>();
        static void Main(string[] args)
        {
            DisplayMenu();
            
            int selection = Int32.Parse(Console.ReadLine());

            switch (selection) 
            {
                case 1:
                    IDE(InstructionContainer);
                    break;
                default: throw new Exception("Invalid selection");
            }
        }

        private static void DisplayMenu() 
        {
            Console.WriteLine("1. IDE");
            Console.WriteLine("2. Print Instructions");
            Console.WriteLine("3. Print Registers");
            Console.WriteLine("4. Exit");
        }

        private static void IDE(List<string> container) 
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
                container.Add(instruction);
            }
            foreach (var instruction in container) 
            {
                Console.WriteLine(instruction);
            }
        }
    }
}
