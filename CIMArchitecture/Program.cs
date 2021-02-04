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
                            var commands = factory.GatherUserInstructions();
                            var compiler = new CIMCompiler(factory.Instructions, factory.Registers, commands);
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
            foreach (var instruction in factory.UserInput) 
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
    }
}
