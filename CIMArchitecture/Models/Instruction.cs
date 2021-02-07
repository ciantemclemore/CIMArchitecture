using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CIMArchitecture
{
    /// <summary>
    /// Defines the structure for an instruction
    /// </summary>
    class Instruction : IEquatable<Instruction>
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string FunctionName { get; set; }

        public bool Equals([AllowNull] Instruction other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Instruction objAsPart = obj as Instruction;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
