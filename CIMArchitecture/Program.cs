using System;
using System.Collections.Generic;

namespace CIMArchitecture
{
    class Program
    {
        private static List<string> UserInput = new List<string>();

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
                            GatherUserInstructionCommands();
                            new CIMCompiler(factory, UserInput);
                            break;
                        default: throw new Exception("Invalid selection");
                    }


                    selection = DisplayMenu();

                    if (selection == 4)
                    {
                        runProg = false;
                    }

                    //Before restarting, clean up
                    UserInput.Clear();
                }
            }
        }

        private static int DisplayMenu() 
        {
            Console.WriteLine("1. IDE");
            Console.WriteLine("4. Exit");

            return Int32.Parse(Console.ReadLine());
        }

        private static void GatherUserInstructionCommands()
        {
            string key = "run";

            Console.WriteLine("Enter instructions: (Press enter after entering each instruction)");
            Console.WriteLine("Enter 'RUN' command once finished to compile");

            while (true)
            {

                string instruction = Console.ReadLine();

                if (instruction.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                UserInput.Add(instruction);
            }
        }
    }
}
