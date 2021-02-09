using CIMArchitecture.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CIMArchitecture
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = GetConfigurationData(@"Database\Configuration.json");

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
                            new CIMCompiler(configuration);
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
        }

        private static int DisplayMenu() 
        {
            Console.WriteLine("1. IDE");
            Console.WriteLine("4. Exit");
            return Int32.Parse(Console.ReadLine());
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
