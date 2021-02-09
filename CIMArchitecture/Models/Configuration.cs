using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture.Models
{
    public class Configuration
    {
        public Dictionary<string, Register> Registers { get; set; }

        public Dictionary<string, Instruction> Instructions { get; set; }
    }
}
