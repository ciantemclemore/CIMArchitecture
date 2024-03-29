﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CIMArchitecture.Models
{
    public class Result
    {
        public Instruction Instruction { get; set; }

        public string BinaryString { get; set; }

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }

        public const string DefaultErrorMessage = "Value Not Supported";
        public const string NoErrorMessage = "None";
    }
}
