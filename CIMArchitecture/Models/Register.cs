using System;
using System.Diagnostics.CodeAnalysis;

namespace CIMArchitecture
{
    /// <summary>
    /// Defines the structure for a register.
    /// </summary>
    class Register: IEquatable<Register>
    {
        public string Name { get; set; }

        public int DataValue { get; set; }

        public string BitValue { get; set; }

        public bool Equals([AllowNull] Register other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Register objAsPart = obj as Register;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return DataValue;
        }
    }
}
